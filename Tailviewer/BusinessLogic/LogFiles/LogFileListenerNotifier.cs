using System;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public sealed class LogFileListenerNotifier
	{
		private readonly ILogFileListener _listener;
		private readonly ILogFile _logFile;
		private readonly int _maximumCount;
		private readonly TimeSpan _maximumTime;
		private int _lastNumberOfLines;
		private DateTime _lastReportedTime;
		private bool _sentAnyData;

		public LogFileListenerNotifier(ILogFile logFile, ILogFileListener listener, TimeSpan maximumTime, int maximumCount)
		{
			if (logFile == null) throw new ArgumentNullException(nameof(logFile));
			if (listener == null) throw new ArgumentNullException(nameof(listener));

			_logFile = logFile;
			_listener = listener;
			_maximumTime = maximumTime;
			_maximumCount = maximumCount;

			Reset();

			_listener.OnLogFileModified(logFile, LogFileSection.Reset);
		}

		public int LastNumberOfLines
		{
			get { return _lastNumberOfLines; }
		}

		private void Reset()
		{
			_lastNumberOfLines = 0;
			_lastReportedTime = DateTime.Now;
			_sentAnyData = false;
		}

		public void Flush(int numberOfLinesRead, DateTime now)
		{
			Report(numberOfLinesRead, now);
			_sentAnyData = true;
		}

		public void OnRead(int numberOfLinesRead)
		{
			if (numberOfLinesRead >= 0)
			{
				DateTime now = DateTime.Now;
				if (now - _lastReportedTime >= _maximumTime)
				{
					Flush(numberOfLinesRead, now);
				}
				else if (numberOfLinesRead - _lastNumberOfLines >= _maximumCount)
				{
					Flush(numberOfLinesRead, now);
				}
			}
			else if (_sentAnyData) //< We want to avoid sending multiple successive reset events
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
			int lastIndex = Math.Min(firstIndex + count, _lastNumberOfLines);
			int invalidateCount = lastIndex - firstIndex;
			// When the start index of the invalidation is greater than the last reported index
			// then this means that our listeners haven't even gotten the change yet and thus
			// they don't need to be notified of the invalidation either.
			if (invalidateCount > 0)
			{
				var section = new LogFileSection(firstIndex, invalidateCount, true);
				_listener.OnLogFileModified(_logFile, section);
				_lastNumberOfLines = firstIndex;
			}
		}
	}
}