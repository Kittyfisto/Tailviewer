using System;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class LogFileListenerNotifier
	{
		private readonly ILogTableListener _listener;
		private readonly ILogTable _logTable;
		private readonly int _maximumLineCount;
		private readonly TimeSpan _maximumWaitTime;

		public LogFileListenerNotifier(ILogTable logTable, ILogTableListener listener, TimeSpan maximumWaitTime,
		                               int maximumLineCount)
		{
			if (logTable == null)
				throw new ArgumentNullException(nameof(logTable));

			_logTable = logTable;
			_listener = listener;
			_maximumWaitTime = maximumWaitTime;
			_maximumLineCount = maximumLineCount;
		}

		public void EmitChanged(LogTableModification modification)
		{
			_listener.OnLogTableModified(_logTable, modification);
		}
	}
}