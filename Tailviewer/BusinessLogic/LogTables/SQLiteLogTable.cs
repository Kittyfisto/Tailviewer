using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class SQLiteLogTable
		: ILogTable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly string _fileName;
		private readonly LogTableListenerCollection _listeners;

		private readonly ITaskScheduler _scheduler;
		private readonly IPeriodicTask _task;

		#region Data

		private readonly List<LogTableRow> _rows;
		private readonly object _syncRoot;
		private bool _exists;
		private DateTime _lastModified;
		private SQLiteLogTableSchema _schema;

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
			_task = _scheduler.StartPeriodic(Update, ToString());
		}

		public SQLiteLogTableSchema Schema
		{
			get { return _schema; }
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

		public bool Exists
		{
			get { return _exists; }
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

		public override string ToString()
		{
			return _fileName;
		}

		private TimeSpan Update()
		{
			var info = new FileInfo(_fileName);
			if (info.Exists)
			{
				if (!_exists)
				{
					Log.DebugFormat("SQLITE Database {0} was created (again)", _fileName);
				}

				_exists = true;

				DateTime modified = info.LastWriteTime;
				if (modified != _lastModified)
				{
					Log.DebugFormat("SQLITE Database {0} has been touched since our last update {1}, updating again", _fileName,
					                _lastModified);

					UpdateFromDatabase();

					_lastModified = modified;
				}
				else
				{
					Log.DebugFormat("SQLITE Database {0} hasn't been touched since {1}, skipping updated", _fileName, modified);
				}
			}
			else
			{
				if (_exists)
				{
					Log.DebugFormat("SQLITE Database {0} was deleted/is no longer reachable", _fileName);
				}

				_exists = false;
				_listeners.OnRead(LogEntryIndex.Invalid, 0);
			}

			return TimeSpan.FromMilliseconds(100);
		}

		private void UpdateFromDatabase()
		{
			string connectionString = string.Format("Data Source={0};Version=3;", _fileName);
			using (var connection = new SQLiteConnection(connectionString))
			{
				connection.Open();

				string tableName;
				if (TryFindTable(connection, out tableName))
				{
					SQLiteLogTableSchema schema = GetSchema(connection, tableName);
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

					int rowCount = Count(connection, tableName);
					if (rowCount < _rows.Count)
					{
						int invalidateCount = _rows.Count - rowCount;
						InvalidateSection(rowCount, invalidateCount);
					}
					else if (rowCount > _rows.Count)
					{
						ReadSection(connection, tableName, _rows.Count, rowCount - _rows.Count);
					}
				}
				else
				{
				}
			}
		}

		/// <summary>
		///     Counts the number of rows in the given table.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="tableName"></param>
		/// <returns></returns>
		private static int Count(SQLiteConnection connection, string tableName)
		{
			string sql = string.Format("SELECT COUNT(*) FROM {0}", tableName);
			using (var command = new SQLiteCommand(sql, connection))
			{
				object value = command.ExecuteScalar();
				int count = Convert.ToInt32(value);
				return count;
			}
		}

		/// <summary>
		///     Invalidates a section of rows from this table.
		/// </summary>
		/// <param name="rowCount"></param>
		/// <param name="invalidateCount"></param>
		private void InvalidateSection(int rowCount, int invalidateCount)
		{
			lock (_syncRoot)
			{
				_rows.RemoveRange(rowCount, invalidateCount);
			}

			_listeners.OnRead(rowCount, invalidateCount, true);
		}

		/// <summary>
		///     Reads a section of rows from the given table.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="tableName"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		private void ReadSection(SQLiteConnection connection, string tableName, int index, int count)
		{
			string sql = string.Format("SELECT * FROM {0} LIMIT {1} OFFSET {2}", tableName, count, index);
			using (var command = new SQLiteCommand(sql, connection))
			using (SQLiteDataReader reader = command.ExecuteReader())
			{
				int i = 0;
				while (reader.Read())
				{
					var row = ReadRow(reader);

					lock (_syncRoot)
					{
						_rows.Add(row);
					}

					_listeners.OnRead(index + i, 1);

					++i;
				}
			}
		}

		[Pure]
		private LogTableRow ReadRow(SQLiteDataReader reader)
		{
			var fields = new object[reader.FieldCount];
			for (int i = 0; i < fields.Length; ++i)
			{
				fields[i] = reader.GetValue(i);
			}
			return new LogTableRow(fields);
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
			using (SQLiteDataReader reader = command.ExecuteReader())
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
	}
}