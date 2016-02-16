using System;

namespace Tailviewer.BusinessLogic
{
	public sealed class LogFileListenerNotifier
	{
		private readonly ILogFileListener _listener;
		private readonly TimeSpan _maximumTime;
		private readonly int _maximumCount;
		private DateTime _lastReportedTime;
		private int _lastNumberOfLines;
		private readonly ILogFile _logFile;

		public LogFileListenerNotifier(ILogFile logFile, ILogFileListener listener, TimeSpan maximumTime, int maximumCount)
		{
			if (logFile == null) throw new ArgumentNullException("logFile");
			if (listener == null) throw new ArgumentNullException("listener");

			_logFile = logFile;
			_listener = listener;
			_maximumTime = maximumTime;
			_maximumCount = maximumCount;

			Reset();
		}

		private void Reset()
		{
			_lastNumberOfLines = 0;
			_lastReportedTime = DateTime.Now;
		}

		public void OnRead(int numberOfLinesRead)
		{
			if (numberOfLinesRead >= 0)
			{
				var now = DateTime.Now;
				if (now - _lastReportedTime >= _maximumTime)
				{
					Report(numberOfLinesRead, now);
				}
				else if (numberOfLinesRead - _lastNumberOfLines >= _maximumCount)
				{
					Report(numberOfLinesRead, now);
				}
			}
			else
			{
				Reset();
				_listener.OnLogFileModified(_logFile, LogFileSection.Reset);
			}
		}

		private void Report(int numberOfLinesRead, DateTime now)
		{
			// We may never report all lines in one go if the listener specified
			// that he only wants to receive batches of N.
			// Therefore we invoke the listener multiple times.
			int count;
			while ((count = numberOfLinesRead - _lastNumberOfLines) > 0)
			{
				count = Math.Min(count, _maximumCount);
				var section = new LogFileSection(_lastNumberOfLines, count);
				_listener.OnLogFileModified(_logFile, section);

				_lastNumberOfLines += count;
				_lastReportedTime = now;
			}
		}

		public void Invalidate(int firstIndex, int count)
		{
			var section = new LogFileSection(firstIndex, count, true);
			_listener.OnLogFileModified(_logFile, section);
		}
	}
}
