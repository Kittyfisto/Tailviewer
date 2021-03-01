using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.Core.Buffers;
using Tailviewer.Core.Columns;

namespace Tailviewer.Core.Sources.Buffer
{
	/// <summary>
	///    Responsible for buffering entire log entries in memory so they may be retrieved later one a bit quicker.
	/// </summary>
	[DebuggerTypeProxy(typeof(LogSourceDebuggerVisualization))]
	internal sealed class PageBufferedLogSource
		: ILogSource
		, ILogSourceListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		///     Tests if the log entry was successfully retrieved.
		/// </summary>
		/// <remarks>
		///     This column is necessary for when a client requests to fetch data *only* from the cache *and* wants
		///     to check which entries were successfully retrieved (and especially which ones were not).
		/// </remarks>
		public static readonly IColumnDescriptor<RetrievalState> RetrievalState;

		private readonly object _syncRoot;
		private readonly ProxyLogListenerCollection _listeners;
		private readonly IReadOnlyList<IColumnDescriptor> _cachedColumns;
		private readonly ITaskScheduler _taskScheduler;
		private readonly ILogSource _source;
		private readonly int _maxNumPages;
		private readonly PagedLogBuffer _buffer;
		private readonly ConcurrentQueue<LogSourceSection> _fetchQueue;
		private readonly IPeriodicTask _fetchTask;
		private readonly LogBufferArray _fetchBuffer;

		public const int DefaultPageSize = 1000;
		public const int DefaultMaxPageCount = 10;

		static PageBufferedLogSource()
		{
			RetrievalState = new WellKnownColumnDescriptor<RetrievalState>("retrieval_state", Buffer.RetrievalState.NotInSource);
		}

		public PageBufferedLogSource(ITaskScheduler taskScheduler, ILogSource source, TimeSpan maximumWaitTime, int pageSize = DefaultPageSize, int maxNumPages = DefaultMaxPageCount)
			: this(taskScheduler, source, maximumWaitTime, new IColumnDescriptor[0], pageSize, maxNumPages)
		{ }

		public PageBufferedLogSource(ITaskScheduler taskScheduler, ILogSource source, TimeSpan maximumWaitTime, IReadOnlyList<IColumnDescriptor> nonCachedColumns, int pageSize = DefaultPageSize, int maxNumPages = DefaultMaxPageCount)
		{
			_syncRoot = new object();
			_taskScheduler = taskScheduler;
			_source = source;
			_maxNumPages = maxNumPages;
			_listeners = new ProxyLogListenerCollection(source, this);
			_cachedColumns = source.Columns.Except(nonCachedColumns).ToList();
			_buffer = new PagedLogBuffer(pageSize, maxNumPages, _cachedColumns);
			_fetchQueue = new ConcurrentQueue<LogSourceSection>();
			_source.AddListener(this, maximumWaitTime, pageSize);

			_fetchBuffer = new LogBufferArray(pageSize, _cachedColumns);
			_fetchTask = _taskScheduler.StartPeriodic(FetchPagesFromSource, maximumWaitTime);
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_taskScheduler.StopPeriodic(_fetchTask);

			lock (_syncRoot)
			{
				_buffer.Clear();
			}
		}

		#endregion

		#region Implementation of ILogSource

		public IReadOnlyList<IColumnDescriptor> Columns
		{
			get
			{
				return _source.Columns;
			}
		}

		public void AddListener(ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogSourceListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public IReadOnlyList<IReadOnlyPropertyDescriptor> Properties => _source.Properties;

		public object GetProperty(IReadOnlyPropertyDescriptor property)
		{
			return _source.GetProperty(property);
		}

		public T GetProperty<T>(IReadOnlyPropertyDescriptor<T> property)
		{
			return _source.GetProperty(property);
		}

		public void SetProperty(IPropertyDescriptor property, object value)
		{
			_source.SetProperty(property, value);
		}

		public void SetProperty<T>(IPropertyDescriptor<T> property, T value)
		{
			_source.SetProperty(property, value);
		}

		public void GetAllProperties(IPropertiesBuffer destination)
		{
			_source.GetAllProperties(destination);
		}

		public void GetColumn<T>(IReadOnlyList<LogLineIndex> sourceIndices,
		                         IColumnDescriptor<T> column,
		                         T[] destination,
		                         int destinationIndex,
		                         LogSourceQueryOptions queryOptions)
		{
			if (sourceIndices == null)
				throw new ArgumentNullException(nameof(sourceIndices));

			GetEntries(sourceIndices, new SingleColumnLogBufferView<T>(column, destination, destinationIndex, sourceIndices.Count), 0, queryOptions);
		}

		public void GetEntries(IReadOnlyList<LogLineIndex> sourceIndices,
		                       ILogBuffer destination,
		                       int destinationIndex,
		                       LogSourceQueryOptions queryOptions)
		{
			if (TryReadFromCache(sourceIndices, destination, destinationIndex, queryOptions, out var accessedPageBoundaries))
				return;

			if ((queryOptions.QueryMode & LogSourceQueryMode.FetchForLater) == LogSourceQueryMode.FetchForLater)
				FetchForLater(accessedPageBoundaries);

			// For some clients (GUI) it is permissible to serve partial requests and to default out the rest.
			// The ui will try again at a later date until it gets what it needs.
			if ((queryOptions.QueryMode & LogSourceQueryMode.FromSource) == 0)
				return;

			// Whelp, we gotta fetch from the source instead.
			// Note: We don't lock this part because this would block any other thread currently
			//       trying to read data from cache, etc.. We only block when we need to and only
			//       for a short amount of time!
			_source.GetEntries(sourceIndices, destination, destinationIndex, queryOptions);

			// However now that we got some data, we could try to add it to our cache
			AddToCache(sourceIndices, destination, destinationIndex, queryOptions);
		}

		private void FetchForLater(IReadOnlyList<LogSourceSection> sectionsToFetch)
		{
			_fetchQueue.EnqueueMany(sectionsToFetch);
		}

		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			return _source.GetLogLineIndexOfOriginalLineIndex(originalLineIndex);
		}

		#endregion

		#region Implementation of ILogSourceListener

		public void OnLogFileModified(ILogSource logSource, LogSourceModification modification)
		{
			lock (_syncRoot)
			{
				if (modification.IsReset())
				{
					_buffer.Clear();
				}
				else if (modification.IsRemoved(out var removedSection))
				{
					_buffer.ResizeTo((int) removedSection.Index);
				}
				else if (modification.IsAppended(out var appendedSection))
				{
					_buffer.ResizeTo((int) (appendedSection.Index + appendedSection.Count));
				}
			}
		}

		#endregion

		[ThreadSafe]
		private bool TryReadFromCache(IReadOnlyList<LogLineIndex> sourceIndices,
		                              ILogBuffer destination, int destinationIndex,
		                              LogSourceQueryOptions queryOptions,
		                              out IReadOnlyList<LogSourceSection> accessedPageBoundaries)
		{
			accessedPageBoundaries = null;

			// For whatever reason some people prefer to read directly from the source (this might not be a real use case but whatever..)
			if ((queryOptions.QueryMode & LogSourceQueryMode.FromCache) != LogSourceQueryMode.FromCache)
				return false;

			lock (_syncRoot)
			{
				if (_buffer.TryGetEntries(sourceIndices, destination, destinationIndex, out accessedPageBoundaries))
					return true;
			}

			return false;
		}

		[ThreadSafe]
		private void AddToCache(IReadOnlyList<LogLineIndex> sourceIndices,
		                        ILogBuffer source,
		                        int sourceIndex,
		                        LogSourceQueryOptions queryOptions)
		{
			if ((queryOptions.QueryMode & LogSourceQueryMode.DontCache) == LogSourceQueryMode.DontCache)
				return;

			if (sourceIndices is LogSourceSection contiguousSection)
			{
				AddToCache(source, sourceIndex, contiguousSection);
			}
		}

		[ThreadSafe]
		private void AddToCache(ILogBuffer source, int destinationIndex, LogSourceSection contiguousSection)
		{
			lock (_syncRoot)
			{
				_buffer.TryAdd(contiguousSection, source, destinationIndex);
			}
		}

		private void FetchPagesFromSource()
		{
			// There's no point in fetching more data than can fit in the cache.
			// When that happens, we assume that the data accessed last is more important than
			// the one we accessed earlier...
			var sections = _fetchQueue.DequeueAll().Reverse().Take(_maxNumPages).Distinct().ToList();

			if (sections.Count > 0)
			{
				if(Log.IsDebugEnabled)
					Log.DebugFormat("Fetching data in {0} batches...", sections.Count);

				foreach (var section in sections)
				{
					// Yes, we could make this async and fetch data even faster, but we gotta
					// start somewhere...
					_source.GetEntries(section, _fetchBuffer);
					AddToCache(_fetchBuffer, 0, section);
				}
			}
		}
	}
}
