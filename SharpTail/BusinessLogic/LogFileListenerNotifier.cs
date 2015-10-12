using System;

namespace SharpTail.BusinessLogic
{
	public sealed class LogFileListenerNotifier
	{
		private readonly ILogFileListener _listener;
		private readonly TimeSpan _maximumTime;
		private readonly int _maximumCount;
		private DateTime _lastReportedTime;
		private int _lastNumberOfLines;

		public LogFileListenerNotifier(ILogFileListener listener, TimeSpan maximumTime, int maximumCount)
		{
			_listener = listener;
			_maximumTime = maximumTime;
			_maximumCount = maximumCount;

			_lastNumberOfLines = 0;
			_lastReportedTime = DateTime.Now;
		}

		public void OnRead(int numberOfLinesRead)
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
				_listener.OnLogFileModified(section);

				_lastNumberOfLines += count;
				_lastReportedTime = now;
			}
		}
	}
}
