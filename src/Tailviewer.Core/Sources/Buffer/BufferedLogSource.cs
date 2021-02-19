using System;
using System.Collections.Generic;
using System.Threading;
using Tailviewer.Core.Buffers;

namespace Tailviewer.Core.Sources.Buffer
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class BufferedLogSource
		: ILogSource
		, ILogSourceListener
	{
		private readonly object _syncRoot;
		private readonly ProxyLogListenerCollection _listeners;
		private readonly ITaskScheduler _taskScheduler;
		private readonly ILogSource _source;
		private readonly PagedLogBuffer _buffer;

		public BufferedLogSource(ITaskScheduler taskScheduler, ILogSource source, TimeSpan maximumWaitTime, int pageSize = 1000, int maxNumPages = 10)
		{
			_syncRoot = new object();
			_taskScheduler = taskScheduler;
			_source = source;
			_listeners = new ProxyLogListenerCollection(source, this);
			_buffer = new PagedLogBuffer(pageSize, maxNumPages, source.Columns);
			_source.AddListener(this, maximumWaitTime, pageSize);
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
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
			if (TryReadFromCache(sourceIndices, destination, destinationIndex, queryOptions))
				return;

			// Whelp, we gotta fetch from the source instead.
			// Note: We don't lock this part because this would block any other thread currently
			//       trying to read data from cache, etc.. We only block when we need to and only
			//       for a short amount of time!
			_source.GetEntries(sourceIndices, destination, destinationIndex, queryOptions);

			// However now that we got some data, we could try to add it to our cache
			AddToCache(sourceIndices, destination, destinationIndex, queryOptions);
		}

		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			return _source.GetLogLineIndexOfOriginalLineIndex(originalLineIndex);
		}

		#endregion

		#region Implementation of ILogSourceListener

		public void OnLogFileModified(ILogSource logSource, LogFileSection section)
		{
			lock (_syncRoot)
			{
				if (section.IsReset)
				{
					_buffer.Clear();
				}
				else if (section.IsInvalidate)
				{
					_buffer.ResizeTo((int) section.Index);
				}
				else
				{
					_buffer.ResizeTo((int) (section.Index + section.Count));
				}
			}
		}

		#endregion

		[ThreadSafe]
		private bool TryReadFromCache(IReadOnlyList<LogLineIndex> sourceIndices, ILogBuffer destination, int destinationIndex, LogSourceQueryOptions queryOptions)
		{
			// For whatever reason some people prefer to read directly from the source (this might not be a real use case but whatever..)
			if ((queryOptions.QueryMode & LogSourceQueryMode.FromCache) != LogSourceQueryMode.FromCache)
				return false;

			lock (_syncRoot)
			{
				if (_buffer.TryGetEntries(sourceIndices, destination, destinationIndex))
					return true;

				// For some clients (GUI) it is permissible to serve partial requests and to default out the rest.
				// The ui will try again at a later date until it gets what it needs.
				if ((queryOptions.QueryMode & LogSourceQueryMode.AllowPartialRead) == LogSourceQueryMode.AllowPartialRead)
				{
					destination.FillDefault(destinationIndex, sourceIndices.Count);
					return true;
				}
			}

			return false;
		}

		[ThreadSafe]
		private void AddToCache(IReadOnlyList<LogLineIndex> sourceIndices,
		                        ILogBuffer destination,
		                        int destinationIndex,
		                        LogSourceQueryOptions queryOptions)
		{
			if ((queryOptions.QueryMode & LogSourceQueryMode.DontCache) == LogSourceQueryMode.DontCache)
				return;

			if (sourceIndices is LogFileSection contiguousSection)
			{
				lock (_syncRoot)
				{
					_buffer.Add(contiguousSection, destination, destinationIndex);
				}
			}
		}
	}
}
