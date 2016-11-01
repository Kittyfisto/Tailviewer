using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class SQLiteLogTable
		: ILogTable
	{
		private readonly ITaskScheduler _scheduler;
		private readonly IPeriodicTask _task;
		private readonly LogTableListenerCollection _listeners;
		private readonly string _fileName;

		#region Data

		private readonly object _syncRoot;
		private readonly List<LogTableRow> _rows;
		private SQLiteLogTableSchema _schema;
		private DateTime _lastModified;

		#endregion

		public SQLiteLogTable(ITaskScheduler scheduler, string fileName)
		{
			if (scheduler == null)
				throw new ArgumentNullException("scheduler");
			if (fileName == null)
				throw new ArgumentNullException("fileName");

			_fileName = fileName;
			_listeners = new LogTableListenerCollection(this);
			_rows = new List<LogTableRow>();
			_syncRoot = new object();

			_schema = new SQLiteLogTableSchema(string.Empty);

			_scheduler = scheduler;
			_task = _scheduler.StartPeriodic(Update);
		}

		private TimeSpan Update()
		{
			var info = new FileInfo(_fileName);
			if (info.Exists)
			{
				var modified = info.LastWriteTime;
				if (modified != _lastModified)
				{
					UpdateFromDatabase();
					_lastModified = modified;
				}
			}
			else
			{
				_listeners.OnRead(LogEntryIndex.Invalid, 0);
			}

			return TimeSpan.FromMilliseconds(100);
		}

		private void UpdateFromDatabase()
		{
			var connectionString = string.Format("Data Source={0};Version=3;", _fileName);
			using (var connection = new SQLiteConnection(connectionString))
			{
				connection.Open();

				string tableName;
				if (TryFindTable(connection, out tableName))
				{
					var schema = GetSchema(connection, tableName);
					if (!Equals(schema, _schema))
					{
						_schema = schema;

						lock (_syncRoot)
						{
							_rows.Clear();
						}

						_listeners.OnSchemaChanged(_schema);
						_listeners.OnRead(LogEntryIndex.Invalid, 0);
					}
				}
				else
				{
					
				}
			}
		}

		private static bool TryFindTable(SQLiteConnection connection, out string tableName)
		{
			// TODO: Find the proper table based on name and maybe columns...
			tableName = "log";
			return true;
		}

		private static SQLiteLogTableSchema GetSchema(SQLiteConnection connection, string tableName)
		{
			string sql = string.Format("PRAGMA table_info({0})", tableName);
			using (var command = new SQLiteCommand(sql, connection))
			using (var reader = command.ExecuteReader())
			{
				var values = new object[reader.FieldCount];
				var columns = new List<SQLiteColumnHeader>();

				while (reader.Read())
				{
					reader.GetValues(values);

					var name = values[1] as string;
					var type = values[2] as string;

					columns.Add(new SQLiteColumnHeader(name, type));
				}

				return new SQLiteLogTableSchema(tableName, columns);
			}
		}

		public int RowCount
		{
			get
			{
				lock (_syncRoot)
				{
					return _rows.Count;
				}
			}
		}

		public SQLiteLogTableSchema Schema
		{
			get { return _schema; }
		}

		ILogTableSchema ILogTable.Schema
		{
			get { return Schema; }
		}

		public LogTableRow this[int index]
		{
			get
			{
				lock (_syncRoot)
				{
					return _rows[index];
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
			_scheduler.StopPeriodic(_task);
		}
	}
}