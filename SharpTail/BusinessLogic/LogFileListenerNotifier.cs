using System;

namespace SharpTail.BusinessLogic
{
	public sealed class LogFileListenerNotifier
	{
		private readonly ILogFileListener _listener;
		private readonly TimeSpan _maximumTime;
		private readonly int _maximumCount;
		private DateTime _lastReportedTime;
		private int _lastReportedLine;

		public LogFileListenerNotifier(ILogFileListener listener, TimeSpan maximumTime, int maximumCount)
		{
			_listener = listener;
			_maximumTime = maximumTime;
			_maximumCount = maximumCount;

			_lastReportedLine = -1;
			_lastReportedTime = DateTime.Now;
		}

		public void OnLineRead(int currentLineIndex)
		{
			var now = DateTime.Now;
			if (now - _lastReportedTime >= _maximumTime)
			{
				Report(currentLineIndex, now);
			}
			else if (currentLineIndex - _lastReportedLine >= _maximumCount)
			{
				Report(currentLineIndex, now);
			}
		}

		private void Report(int currentLineIndex, DateTime now)
		{
			// We may never report all lines in one go if the listener specified
			// that he only wants to receive batches of N.
			// Therefore we invoke the listener multiple times.
			int count;
			while ((count = currentLineIndex - _lastReportedLine) > 0)
			{
				count = Math.Min(count, _maximumCount);
				var section = new LogFileSection(_lastReportedLine + 1, count);
				_listener.OnLogFileModified(section);

				_lastReportedLine += count;
				_lastReportedTime = now;
			}
		}
	}
}
