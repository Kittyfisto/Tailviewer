using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Metrolib;
using Tailviewer.BusinessLogic.Scheduling;
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

		public LogFileProxy(ITaskScheduler taskScheduler)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException("taskScheduler");

			_taskScheduler = taskScheduler;
			_pendingSections = new ConcurrentQueue<KeyValuePair<ILogFile, LogFileSection>>();
			_listeners = new LogFileListenerCollection(this);

			_task = _taskScheduler.StartPeriodic(RunOnce, TimeSpan.FromMilliseconds(100), "Log File Proxy");
		}

		public LogFileProxy(ITaskScheduler taskScheduler, ILogFile innerLogFile)
			: this(taskScheduler)
		{
			InnerLogFile = innerLogFile;
		}

		private void RunOnce()
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
					else if (section.InvalidateSection)
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
		}

		public ILogFile InnerLogFile
		{
			get { return _innerLogFile; }
			set
			{
				if (value == _innerLogFile)
					return;

				if (_innerLogFile != null)
				{
					_innerLogFile.RemoveListener(this);
				}

				_innerLogFile = value;

				// We're now representing a different log file.
				// To the outside, we model this as a simple reset, followed
				// by the content of the new logfile...
				_pendingSections.Enqueue(new KeyValuePair<ILogFile, LogFileSection>(_innerLogFile, LogFileSection.Reset));

				if (_innerLogFile != null)
				{
					_innerLogFile.AddListener(this, TimeSpan.Zero, 1000);
				}
			}
		}

		public void Dispose()
		{
			ILogFile logFile = _innerLogFile;
			if (logFile != null)
			{
				logFile.Dispose();
			}
			_taskScheduler.StopPeriodic(_task);
			_isDisposed = true;
		}

		public bool IsDisposed
		{
			get { return _isDisposed; }
		}

		public DateTime? StartTimestamp
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.StartTimestamp;

				return null;
			}
		}

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

		public Size FileSize
		{
			get
			{
				ILogFile logFile = _innerLogFile;
				if (logFile != null)
					return logFile.FileSize;

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
			// I think the traditional listeners collection doesn't work here because
			// we're not telling it constantly to check if the specified amount of time has elapsed
			// and therefore listeners of static files won't get the right notifications.
			// We can fix this temporarily by immediately forwarding events, but that can't be
			// the solution.
			_listeners.AddListener(listener, TimeSpan.Zero, maximumLineCount);
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