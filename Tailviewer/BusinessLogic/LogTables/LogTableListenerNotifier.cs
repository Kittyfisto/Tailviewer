using System;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class LogTableListenerNotifier
	{
		private readonly ILogTableListener _listener;
		private readonly ILogTable _logTable;
		private readonly int _maximumCount;
		private readonly TimeSpan _maximumTime;
		private int _lastNumberOfEntries;
		private DateTime _lastReportedTime;
		private bool _sentAnyData;

		public LogTableListenerNotifier(ILogTable logTable,
			ILogTableListener listener,
			TimeSpan maximumTime,
			int maximumCount)
		{
			if (logTable == null)
				throw new ArgumentNullException(nameof(logTable));

			_logTable = logTable;
			_listener = listener;
			_maximumTime = maximumTime;
			_maximumCount = maximumCount;

			Reset();

			_listener.OnLogTableModified(logTable, LogTableModification.Reset);
		}

		public void EmitChanged(LogTableModification modification)
		{
			_listener.OnLogTableModified(_logTable, modification);
		}

		public void OnRead(int numberOfEntriesRead)
		{
			if (numberOfEntriesRead >= 0)
			{
				var now = DateTime.Now;
				if (now - _lastReportedTime >= _maximumTime)
					Flush(numberOfEntriesRead, now);
				else if (numberOfEntriesRead - _lastNumberOfEntries >= _maximumCount)
					Flush(numberOfEntriesRead, now);
			}
			else if (_sentAnyData) //< We want to avoid sending multiple successive reset events
			{
				Reset();
				_listener.OnLogTableModified(_logTable, LogTableModification.Reset);
			}
		}

		public void Invalidate(int firstIndex, int count)
		{
			int lastIndex = Math.Min(firstIndex + count, _lastNumberOfEntries);
			int invalidateCount = lastIndex - firstIndex;
			// When the start index of the invalidation is greater than the last reported index
			// then this means that our listeners haven't even gotten the change yet and thus
			// they don't need to be notified of the invalidation either.
			if (invalidateCount > 0)
			{
				var section = LogTableModification.Invalidate(firstIndex, invalidateCount);
				_listener.OnLogTableModified(_logTable, section);
				_lastNumberOfEntries = firstIndex;
			}
		}

		public void Flush(int numberOfLinesRead, DateTime now)
		{
			Report(numberOfLinesRead, now);
			_sentAnyData = true;
		}

		private void Reset()
		{
			_lastNumberOfEntries = 0;
			_lastReportedTime = DateTime.Now;
			_sentAnyData = false;
		}

		private void Report(int numberOfLinesRead, DateTime now)
		{
			// We may never report all lines in one go if the listener specified
			// that he only wants to receive batches of N.
			// Therefore we invoke the listener multiple times.
			int count;
			while ((count = numberOfLinesRead - _lastNumberOfEntries) > 0)
			{
				count = Math.Min(count, _maximumCount);
				var section = new LogTableModification(_lastNumberOfEntries, count);
				_listener.OnLogTableModified(_logTable, section);

				_lastNumberOfEntries += count;
				_lastReportedTime = now;
			}
		}
	}
}