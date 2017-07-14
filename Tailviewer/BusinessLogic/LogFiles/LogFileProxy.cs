using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Metrolib;
using log4net;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Fully represents another <see cref="ILogFile" /> which can be replaced over the lifetime
	///     of the proxy.
	/// </summary>
	/// <remarks>
	///     Exists so that specialized <see cref="ILogFile" /> implementations don't need to be concerned about re-use
	///     or certain changes (i.e. <see cref="FilteredLogFile" /> doesn't need to implement the change of a filter).
	/// </remarks>
	public sealed class LogFileProxy
		: ILogFile
		, ILogFileListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ITaskScheduler _taskScheduler;
		private readonly LogFileListenerCollection _listeners;
		private readonly ConcurrentQueue<KeyValuePair<ILogFile, LogFileSection>> _pendingSections;
		private readonly IPeriodicTask _task;
		private ILogFile _innerLogFile;
		private bool _isDisposed;
		private readonly TimeSpan _maximumWaitTime;

		public LogFileProxy(ITaskScheduler taskScheduler, TimeSpan maximumWaitTime)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));

			_taskScheduler = taskScheduler;
			_pendingSections = new ConcurrentQueue<KeyValuePair<ILogFile, LogFileSection>>();
			_listeners = new LogFileListenerCollection(this);

			_task = _taskScheduler.StartPeriodic(RunOnce, "Log File Proxy");
			_maximumWaitTime = maximumWaitTime;
		}

		public LogFileProxy(ITaskScheduler taskScheduler, TimeSpan maximumWaitTime, ILogFile innerLogFile)
			: this(taskScheduler, maximumWaitTime)
		{
			InnerLogFile = innerLogFile;
		}

		private TimeSpan RunOnce()
		{
			KeyValuePair<ILogFile, LogFileSection> pair;
			while (_pendingSections.TryDequeue(out pair))
			{
				var sender = pair.Key;
				var innerLogFile = _innerLogFile;
				var section = pair.Value;
				if (sender != innerLogFile)
				{
					// If, for some reason, we receive an event from a previous log file,
					// then we ignore it so our listeners are not confused.
					Log.DebugFormat(
						"Skipping pending modification '{0}' from '{1}' because it is no longer our current log file '{2}'",
						section, sender, innerLogFile);
				}
				else
				{
					if (section.IsReset)
					{
						_listeners.Reset();
					}
					else if (section.IsInvalidate)
					{
						_listeners.Invalidate((int)section.Index, section.Count);
					}
					else
					{
						_listeners.OnRead((int)(section.Index + section.Count));
					}
				}
			}

			// This line is extremely important because listeners are allowed to limit how often they are notified.
			// This means that even when there is NO modification to the source, we still need to notify the collection
			// so it can check if enough time has ellapsed to finally notify listener.
			_listeners.OnRead(_listeners.CurrentLineIndex);
			return TimeSpan.FromMilliseconds(10);
		}

		public ILogFile InnerLogFile
		{
			get { return _innerLogFile; }
			set
			{
				if (value == _innerLogFile)
					return;

				_innerLogFile?.RemoveListener(this);

				_innerLogFile = value;

				// We're now representing a different log file.
				// To the outside, we model this as a simple reset, followed
				// by the content of the new logfile...
				_pendingSections.Enqueue(new KeyValuePair<ILogFile, LogFileSection>(_innerLogFile, LogFileSection.Reset));

				_innerLogFile?.AddListener(this, _maximumWaitTime, 10000);
			}
		}

		public void Dispose()
		{
			ILogFile logFile = _innerLogFile;
			logFile?.Dispose();
			_taskScheduler.StopPeriodic(_task);
			_isDisposed = true;
		}

		public bool IsDisposed => _isDisposed;

		public DateTime? StartTimestamp => _innerLogFile?.StartTimestamp;

		public DateTime LastModified
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.LastModified;

				// Maybe this property should be nullable as well?
				return DateTime.MinValue;
			}
		}

		public Size Size
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.Size;

				return Size.Zero;
			}
		}

		public bool Exists
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.Exists;

				return false;
			}
		}

		public bool EndOfSourceReached
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.EndOfSourceReached;

				return false;
			}
		}

		public int Count
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.Count;

				return 0;
			}
		}

		public int OriginalCount
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.OriginalCount;

				return 0;
			}
		}

		public int MaxCharactersPerLine
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.MaxCharactersPerLine;

				return 0;
			}
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public override string ToString()
		{
			var logFile = _innerLogFile;
			if (logFile != null)
				return string.Format("{0} (Proxy)", logFile);

			return "<Empty>";
		}

		public void RemoveListener(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			ILogFile logFile = _innerLogFile;
			if (logFile != null)
			{
				logFile.GetSection(section, dest);
			}
			else
			{
				throw new IndexOutOfRangeException();
			}
		}

		public LogLineIndex GetLogLineIndexOfOriginalLineIndex(LogLineIndex originalLineIndex)
		{
			var logFile = _innerLogFile;
			if (logFile != null)
			{
				return logFile.GetLogLineIndexOfOriginalLineIndex(originalLineIndex);
			}

			return LogLineIndex.Invalid;
		}

		public LogLineIndex GetOriginalIndexFrom(LogLineIndex index)
		{
			var logFile = _innerLogFile;
			if (logFile != null)
			{
				return logFile.GetOriginalIndexFrom(index);
			}

			return LogLineIndex.Invalid;
		}

		public void GetOriginalIndicesFrom(LogFileSection section, LogLineIndex[] originalIndices)
		{
			if (originalIndices == null)
				throw new ArgumentNullException(nameof(originalIndices));
			if (section.Count > originalIndices.Length)
				throw new ArgumentOutOfRangeException(nameof(originalIndices));

			var logFile = _innerLogFile;
			if (logFile != null)
			{
				logFile.GetOriginalIndicesFrom(section, originalIndices);
			}
			else
			{
				for(int i = 0; i < section.Count; ++i)
					originalIndices[i] = LogLineIndex.Invalid;
			}
		}

		public void GetOriginalIndicesFrom(IReadOnlyList<LogLineIndex> indices, LogLineIndex[] originalIndices)
		{
			if (indices == null)
				throw new ArgumentNullException(nameof(indices));
			if (originalIndices == null)
				throw new ArgumentNullException(nameof(originalIndices));
			if (indices.Count > originalIndices.Length)
				throw new ArgumentOutOfRangeException();

			var logFile = _innerLogFile;
			if (logFile != null)
			{
				logFile.GetOriginalIndicesFrom(indices, originalIndices);
			}
			else
			{
				for (int i = 0; i < indices.Count; ++i)
				{
					originalIndices[i] = LogLineIndex.Invalid;
				}
			}
		}

		public LogLine GetLine(int index)
		{
			ILogFile logFile = _innerLogFile;
			if (logFile != null)
			{
				return logFile.GetLine(index);
			}

			throw new IndexOutOfRangeException();
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_pendingSections.Enqueue(new KeyValuePair<ILogFile, LogFileSection>(logFile, section));
		}
	}
}