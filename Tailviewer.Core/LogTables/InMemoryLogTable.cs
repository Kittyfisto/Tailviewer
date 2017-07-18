using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Core.LogTables
{
	public sealed class InMemoryLogTable
		: ILogTable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly List<LogEntry> _entries;

		private readonly LogTableListenerCollection _listeners;
		private readonly object _syncRoot;

		public InMemoryLogTable(params IColumnHeader[] columns)
		{
			_listeners = new LogTableListenerCollection(this);
			Schema = new LogTableSchema(columns);
			_entries = new List<LogEntry>();
			_syncRoot = new object();
		}

		public int Count => _entries.Count;

		public DateTime LastModified { get; private set; }

		public bool Exists => true;

		public ILogTableSchema Schema { get; }

		public ITask<LogEntry> this[LogEntryIndex index]
		{
			get
			{
				lock (_syncRoot)
				{
					return Task2.FromResult(_entries[(int) index]);
				}
			}
		}

		public void AddListener(ILogTableListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public bool RemoveListener(ILogTableListener listener)
		{
			return _listeners.RemoveListener(listener);
		}

		public void Dispose()
		{
		}

		public void AddEntry(LogEntry entry)
		{
			lock (_syncRoot)
			{
				_entries.Add(entry);
				_listeners.OnRead(_entries.Count);
				Touch();
			}
		}

		public void RemoveFrom(LogEntryIndex index)
		{
			lock (_syncRoot)
			{
				if (index < 0)
				{
					Log.WarnFormat("Invalid index '{0}'", index);
					return;
				}

				if (index > _entries.Count)
				{
					Log.WarnFormat("Invalid index '{0}', Count is '{1}'", index, _entries.Count);
					return;
				}

				var available = _entries.Count - index;
				_entries.RemoveRange((int) index, available);
				_listeners.Invalidate((int) index, available);
				Touch();
			}
		}

		private void Touch()
		{
			LastModified = DateTime.Now;
		}

		public void Clear()
		{
			lock (_syncRoot)
			{
				var count = _entries.Count;
				if (count > 0)
				{
					_entries.Clear();
					_listeners.Invalidate(0, count);
					Touch();
				}
			}
		}
	}
}