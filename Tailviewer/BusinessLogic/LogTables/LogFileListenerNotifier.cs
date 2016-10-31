using System;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class LogFileListenerNotifier
	{
		private readonly ILogTableListener _listener;
		private readonly TimeSpan _maximumWaitTime;
		private readonly int _maximumLineCount;

		public LogFileListenerNotifier(ILogTableListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listener = listener;
			_maximumWaitTime = maximumWaitTime;
			_maximumLineCount = maximumLineCount;
		}
	}
}