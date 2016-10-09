using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public sealed class LogFileListenerCollection
	{
		private readonly Dictionary<ILogFileListener, LogFileListenerNotifier> _listeners;
		private readonly ILogFile _logFile;
		private int _currentLineIndex;

		public LogFileListenerCollection(ILogFile logFile)
		{
			if (logFile == null) throw new ArgumentNullException("logFile");

			_logFile = logFile;
			_listeners = new Dictionary<ILogFileListener, LogFileListenerNotifier>();
			_currentLineIndex = -1;
		}

		public int CurrentLineIndex
		{
			get { return _currentLineIndex; }
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			lock (_listeners)
			{
				if (!_listeners.ContainsKey(listener))
				{
					var notifier = new LogFileListenerNotifier(_logFile, listener, maximumWaitTime, maximumLineCount);
					_listeners.Add(listener, notifier);
					notifier.OnRead(_currentLineIndex);
				}
			}
		}

		public void RemoveListener(ILogFileListener listener)
		{
			lock (_listeners)
			{
				_listeners.Remove(listener);
			}
		}

		public void Reset()
		{
			OnRead(-1);
		}

		public void OnRead(int numberOfLinesRead)
		{
			lock (_listeners)
			{
				foreach (LogFileListenerNotifier notifier in _listeners.Values)
				{
					notifier.OnRead(numberOfLinesRead);
				}
				_currentLineIndex = numberOfLinesRead;
			}
		}

		public void Invalidate(int firstIndex, int count)
		{
			lock (_listeners)
			{
				foreach (LogFileListenerNotifier notifier in _listeners.Values)
				{
					notifier.Invalidate(firstIndex, count);
				}
				_currentLineIndex = firstIndex;
			}
		}

		public void Flush()
		{
			lock (_listeners)
			{
				foreach (LogFileListenerNotifier notifier in _listeners.Values)
				{
					notifier.Flush(_currentLineIndex, DateTime.Now);
				}
			}
		}
	}
}