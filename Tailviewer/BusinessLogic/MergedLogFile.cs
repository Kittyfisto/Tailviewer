using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     Responsible for merging two or more <see cref="ILogFile" /> instances into one coherent view.
	///     <see cref="LogLine" />s which have no <see cref="LogLine.Timestamp" /> set (or who's parent doesn't)
	///     are discarded from this representation.
	/// </summary>
	internal sealed class MergedLogFile
		: ILogFile
		, ILogFileListener
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFile[] _sources;
		private readonly Task _readTask;
		private readonly List<Index> _indices;
		private readonly LogFileListenerCollection _listeners;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ConcurrentQueue<PendingModification> _pendingModifications;
		private int _otherCount;
		private int _debugCount;
		private int _infoCount;
		private int _warningCount;
		private int _errorCount;
		private int _fatalCount;
		private Size _fileSize;
		private DateTime _lastModified;

		struct PendingModification
		{
			public readonly ILogFile LogFile;
			public readonly LogFileSection Section;

			public PendingModification(ILogFile logFile, LogFileSection section)
			{
				LogFile = logFile;
				Section = section;
			}
		}

		/// <summary>
		/// Represents an index in the merged data-structure.
		/// Points towards a particular <see cref="LogLine"/> of a particular
		/// <see cref="ILogFile"/>.
		/// </summary>
		struct Index
		{
			public Index(int logLineIndex, ILogFile logFile)
			{
				LogLineIndex = logLineIndex;
				LogFile = logFile;
			}

			public readonly int LogLineIndex;
			public readonly ILogFile LogFile;
		}

		private const int BatchSize = 1000;

		public MergedLogFile(IEnumerable<ILogFile> sources)
			: this(sources.ToArray())
		{}

		public IEnumerable<ILogFile> Sources
		{
			get { return _sources; }
		}

		public MergedLogFile(params ILogFile[] sources)
		{
			if (sources == null) throw new ArgumentNullException("sources");
			if (sources.Any(x => x == null)) throw new ArgumentException("sources", "sources.Any(x => x == null)");

			_sources = sources;
			_cancellationTokenSource = new CancellationTokenSource();
			_readTask = new Task(Merge,
								 _cancellationTokenSource.Token,
								 _cancellationTokenSource.Token,
								 TaskCreationOptions.LongRunning);
			_listeners = new LogFileListenerCollection(this);
			_pendingModifications = new ConcurrentQueue<PendingModification>();
			_indices = new List<Index>();
		}

		private void Merge(object obj)
		{
			CancellationToken token = _cancellationTokenSource.Token;
			var entries = new LogLine[BatchSize];
			int currentSourceIndex = 0;
			var lastLogEntry = new List<LogLine>();

			while (!token.IsCancellationRequested)
			{
				PendingModification modification;
				while (_pendingModifications.TryDequeue(out modification))
				{
					if (modification.Section.IsReset)
					{
						Clear();
					}
					else if (modification.Section.InvalidateSection)
					{
						throw new NotImplementedException();
					}
					else
					{
						// We need to find out where this new entry (or entries) is/are to be inserted.
						// If the new entry is to be inserted at the end then we simply need to add it as
						// an index and we are done.
						// If the new entry is to be inserted anywhere else, then we need to invalidate
						// everything from that index on, insert the new line at the given index and then
						// issue another modification that includes everything from the newly inserted index
						// to the end.
					}

					_fileSize = _sources.Aggregate(Size.Zero, (a, file) => a + file.FileSize);
					_lastModified = _sources.Aggregate(DateTime.MinValue,
					                                   (a, file) =>
						                                   {
							                                   var modified = file.LastModified;
							                                   if (modified > a)
								                                   return modified;

							                                   return a;
						                                   }
						);
				}
			}
		}

		private void Clear()
		{
			lock (_indices)
			{
				_indices.Clear();
			}
			_listeners.OnRead(-1);
		}

		public void Start(TimeSpan maximumWaitTime)
		{
			if (_readTask.Status == TaskStatus.Created)
			{
				foreach (var logFile in _sources)
				{
					logFile.AddListener(this, maximumWaitTime, BatchSize);
				}
				_readTask.Start();
			}
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			try
			{
				_readTask.Wait();
			}
			catch (AggregateException e)
			{
				Log.DebugFormat("Caught exception while disposing: {0}", e);
			}
			_readTask.Dispose();
		}

		public int FatalCount
		{
			get { return _fatalCount; }
		}

		public void Wait()
		{
			throw new NotImplementedException();
		}

		public DateTime? StartTimestamp
		{
			get { throw new NotImplementedException(); }
		}

		public DateTime LastModified
		{
			get { return _lastModified; }
		}

		public Size FileSize
		{
			get { return _fileSize; }
		}

		public int Count
		{
			get
			{
				lock (_indices)
				{
					return _indices.Count;
				}
			}
		}

		public int OtherCount
		{
			get { return _otherCount; }
		}

		public int DebugCount
		{
			get { return _debugCount; }
		}

		public int InfoCount
		{
			get { return _infoCount; }
		}

		public int WarningCount
		{
			get { return _warningCount; }
		}

		public int ErrorCount
		{
			get { return _errorCount; }
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void Remove(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			for (int i = 0; i < section.Count; ++i)
			{
				// TODO: This seems rubbish - maybe I should change the interface to LogLineIndex altogether?
				dest[i] = GetLine((int)(section.Index + i));
			}
		}

		public LogLine GetLine(int index)
		{
			Index source;
			lock (_indices)
			{
				source = _indices[index];
			}

			var line = source.LogFile.GetLine(source.LogLineIndex);
			return line;
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_pendingModifications.Enqueue(new PendingModification(logFile, section));
		}
	}
}