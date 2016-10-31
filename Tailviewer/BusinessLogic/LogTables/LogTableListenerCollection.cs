using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class LogTableListenerCollection
	{
		private readonly Dictionary<ILogTableListener, LogFileListenerNotifier> _notifiers;
		private readonly object _syncRoot;

		public LogTableListenerCollection()
		{
			_notifiers = new Dictionary<ILogTableListener, LogFileListenerNotifier>();
			_syncRoot = new object();
		}

		public void AddListener(ILogTableListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			lock (_syncRoot)
			{
				var notifier = new LogFileListenerNotifier(listener, maximumWaitTime, maximumLineCount);
				_notifiers.Add(listener, notifier);
			}
		}

		public void RemoveListener(ILogTableListener listener)
		{
			lock (_syncRoot)
			{
				_notifiers.Remove(listener);
			}
		}
	}
}