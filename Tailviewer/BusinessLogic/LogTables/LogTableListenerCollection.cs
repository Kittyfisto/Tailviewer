using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class LogTableListenerCollection
	{
		private readonly Dictionary<ILogTableListener, LogFileListenerNotifier> _notifiers;
		private readonly object _syncRoot;
		private readonly ILogTable _logTable;

		public LogTableListenerCollection(ILogTable logTable)
		{
			if (logTable == null)
				throw new ArgumentNullException("logTable");

			_logTable = logTable;
			_notifiers = new Dictionary<ILogTableListener, LogFileListenerNotifier>();
			_syncRoot = new object();
		}

		public void AddListener(ILogTableListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			lock (_syncRoot)
			{
				var notifier = new LogFileListenerNotifier(_logTable, listener, maximumWaitTime, maximumLineCount);
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

		public void OnRead(LogEntryIndex index, int count)
		{
			lock (_syncRoot)
			{
				foreach (var notifier in _notifiers.Values)
				{
					notifier.EmitChanged(new LogTableModification(index, count));
				}
			}
		}

		public void OnSchemaChanged(ILogTableSchema schema)
		{
			lock (_syncRoot)
			{
				foreach (var notifier in _notifiers.Values)
				{
					notifier.EmitChanged(new LogTableModification(schema));
				}
			}
		}
	}
}