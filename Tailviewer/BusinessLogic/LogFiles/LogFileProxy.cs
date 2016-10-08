using System;
using Metrolib;

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
		private readonly LogFileListenerCollection _listeners;
		private readonly object _syncRoot;
		private ILogFile _innerLogFile;

		public LogFileProxy()
		{
			_listeners = new LogFileListenerCollection(this);
			_syncRoot = new object();
		}

		public LogFileProxy(ILogFile innerLogFile)
			: this()
		{
			InnerLogFile = innerLogFile;
		}

		public ILogFile InnerLogFile
		{
			get { return _innerLogFile; }
			set
			{
				// We need to synchronize this method with the one where we forward
				// modified events. This is because the correctness of data depends on the
				// listeners receiving events in the correct order.
				// Without a lock, it might be possible to have the following order:
				// - Reset
				// - OnLogFileModified(oldLogFile, huge section)
				// - OnLogFileModified(newLogFile, 0 - n)
				// Obviously this would result in a incorrect view of the log file.
				lock (_syncRoot)
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
					_listeners.Reset();

					if (_innerLogFile != null)
					{
						_innerLogFile.AddListener(this, TimeSpan.Zero, 1000);
					}
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

		public void Wait()
		{
			ILogFile logFile = _innerLogFile;
			if (logFile != null)
				logFile.Wait();
		}

		public bool Wait(TimeSpan waitTime)
		{
			ILogFile logFile = _innerLogFile;
			if (logFile != null)
				return logFile.Wait(waitTime);

			return true;
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public override string ToString()
		{
			var logFile = _innerLogFile;
			if (logFile != null)
				return logFile.ToString();

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
			// See InnerLogFile.Set for the reason why.
			// TLDR: Ensure correct order of events sent to listeners.
			lock (_syncRoot)
			{
				// If, for some reason, we receive an event from a previous log file,
				// then we ignore it so our listeners are not confused.
				if (logFile != _innerLogFile)
					return;

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
	}
}