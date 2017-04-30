using System;
using System.Collections.Generic;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class LogTableListenerCollection
	{
		private readonly Dictionary<ILogTableListener, LogTableListenerNotifier> _notifiers;
		private readonly object _syncRoot;
		private readonly ILogTable _logTable;
		private int _currentEntryIndex;

		public LogTableListenerCollection(ILogTable logTable)
		{
			if (logTable == null)
				throw new ArgumentNullException(nameof(logTable));

			_logTable = logTable;
			_notifiers = new Dictionary<ILogTableListener, LogTableListenerNotifier>();
			_syncRoot = new object();
		}

		public void AddListener(ILogTableListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			lock (_syncRoot)
			{
				if (!_notifiers.ContainsKey(listener))
				{
					var notifier = new LogTableListenerNotifier(_logTable, listener, maximumWaitTime, maximumLineCount);
					_notifiers.Add(listener, notifier);
					notifier.OnRead(_currentEntryIndex);
				}
			}
		}

		public bool RemoveListener(ILogTableListener listener)
		{
			lock (_syncRoot)
			{
				return _notifiers.Remove(listener);
			}
		}

		public void OnRead(int numberOfEntriesRead)
		{
			lock (_syncRoot)
			{
				foreach (LogTableListenerNotifier notifier in _notifiers.Values)
				{
					notifier.OnRead(numberOfEntriesRead);
				}
				_currentEntryIndex = numberOfEntriesRead;
			}
		}

		public void Invalidate(int firstIndex, int count)
		{
			lock (_syncRoot)
			{
				foreach (LogTableListenerNotifier notifier in _notifiers.Values)
				{
					notifier.Invalidate(firstIndex, count);
				}
				_currentEntryIndex = firstIndex;
			}
		}

		public void Reset()
		{
			OnRead(-1);
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