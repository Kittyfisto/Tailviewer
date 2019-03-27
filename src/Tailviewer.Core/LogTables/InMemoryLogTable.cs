using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;

namespace Tailviewer.Core.LogTables
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class InMemoryLogTable
		: ILogTable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly List<LogEntry> _entries;

		private readonly LogTableListenerCollection _listeners;
		private readonly object _syncRoot;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		/// <param name="columns"></param>
		public InMemoryLogTable(params IColumnHeader[] columns)
		{
			_listeners = new LogTableListenerCollection(this);
			Schema = new LogTableSchema(columns);
			_entries = new List<LogEntry>();
			_syncRoot = new object();
		}

		/// <inheritdoc />
		public int Count => _entries.Count;

		/// <inheritdoc />
		public DateTime LastModified { get; private set; }

		/// <inheritdoc />
		public bool Exists => true;

		/// <inheritdoc />
		public ILogTableSchema Schema { get; }

		/// <inheritdoc />
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

		/// <inheritdoc />
		public void AddListener(ILogTableListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		/// <inheritdoc />
		public bool RemoveListener(ILogTableListener listener)
		{
			return _listeners.RemoveListener(listener);
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entry"></param>
		public void AddEntry(LogEntry entry)
		{
			lock (_syncRoot)
			{
				_entries.Add(entry);
				_listeners.OnRead(_entries.Count);
				Touch();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
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

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			lock (_syncRoot)
			{
				var count = _entries.Count;
				if (count > 0)
				{
					_entries.Clear();
					_listeners.Invalidate(firstIndex: 0, count: count);
					Touch();
				}
			}
		}
	}
}