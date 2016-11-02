using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using log4net;

namespace Tailviewer.BusinessLogic.LogTables
{
	public sealed class SQLiteLogTable
		: ILogTable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly LogDataAccessQueue<LogEntryIndex, LogEntry> _accessQueue;
		private readonly LogDataCache _cache;

		private readonly string _fileName;
		private readonly LogTableListenerCollection _listeners;

		private readonly ITaskScheduler _scheduler;
		private readonly IPeriodicTask _task;

		#region Data

		private bool _exists;
		private DateTime _lastModified;
		private int _rowCount;
		private SQLiteSchema _schema;

		#endregion

		public SQLiteLogTable(ITaskScheduler scheduler, LogDataCache cache, string fileName)
		{
			if (scheduler == null)
				throw new ArgumentNullException("scheduler");
			if (cache == null)
				throw new ArgumentNullException("cache");
			if (fileName == null)
				throw new ArgumentNullException("fileName");

			_scheduler = scheduler;
			_cache = cache;
			_fileName = fileName;

			_listeners = new LogTableListenerCollection(this);
			_accessQueue = new LogDataAccessQueue<LogEntryIndex, LogEntry>();

			_schema = new SQLiteSchema(string.Empty);

			_task = _scheduler.StartPeriodic(Update, ToString());
		}

		public SQLiteSchema Schema
		{
			get { return _schema; }
		}

		public int RowCount
		{
			get { return _rowCount; }
		}

		public bool Exists
		{
			get { return _exists; }
		}

		ILogTableSchema ILogTable.Schema
		{
			get { return Schema; }
		}

		public ITask<LogEntry> this[LogEntryIndex index]
		{
			get { return _accessQueue[index]; }
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
			_accessQueue.Dispose();
			_scheduler.StopPeriodic(_task);
		}

		public override string ToString()
		{
			return _fileName;
		}

		private TimeSpan Update()
		{
			try
			{
				CheckDatabaseAndUpdate();
			}
			catch (SecurityException e)
			{
				Log.ErrorFormat("Caught exception while checking SQLITE database {0}: {1}",
				                _fileName,
				                e);
			}
			catch (IOException e)
			{
				Log.ErrorFormat("Caught exception while checking SQLITE database {0}: {1}",
				                _fileName,
				                e);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while checking SQLITE database {0}: {1}",
				                _fileName,
				                e);
			}

			return TimeSpan.FromMilliseconds(100);
		}

		/// <summary>
		///     Tests if the database file exists/is reachable and then updates the schema from the database.
		///     At last, resource access requests will either be satisfied, or denied.
		/// </summary>
		private void CheckDatabaseAndUpdate()
		{
			var info = new FileInfo(_fileName);
			if (info.Exists)
			{
				if (!_exists)
				{
					Log.DebugFormat("SQLITE Database {0} was created (again)", _fileName);
				}

				_exists = true;

				try
				{
					UpdateFromDatabase(info.LastWriteTime);
				}
				catch (SQLiteException e)
				{
					Log.ErrorFormat("Caught exception while reading data from SQLITE database {0}: {1}", _fileName, e);
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception while reading data from SQLITE database {0}: {1}", _fileName, e);
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

				// We currently don't have access to the database and therefore
				// we can simply reject all access to data.
				_accessQueue.ExecuteAll(new NoLogDataAccessor<LogEntryIndex, LogEntry>());
			}
		}

		/// <summary>
		///     Tries to open a connection to the database, updates the current schema (if necessary) and then
		///     satisfies all pending resource access requests.
		/// </summary>
		/// <param name="lastWriteTime"></param>
		private void UpdateFromDatabase(DateTime lastWriteTime)
		{
			string connectionString = string.Format("Data Source={0};Version=3;", _fileName);
			using (var connection = new SQLiteConnection(connectionString))
			{
				connection.Open();

				if (lastWriteTime != _lastModified)
				{
					Log.DebugFormat("SQLITE Database {0} has been touched since our last update {1}, retrieving schema and # of rows",
					                _fileName,
					                _lastModified);

					UpdateSchema(connection);
					UpdateRowCount(connection);

					_lastModified = lastWriteTime;
				}
				else
				{
					Log.DebugFormat("SQLITE Database {0} hasn't been touched since {1}, skipping schema update",
					                _fileName,
					                lastWriteTime);
				}

				// We have access to the database and therefore we
				// must now try to satisfy all access to its data.
				_accessQueue.ExecuteAll(new SQLiteAccessor(connection, _schema, _cache, this));
			}
		}

		/// <summary>
		///     Reads the current schema from the given database and then forces a schema update
		///     of all listeners in case the schema has changed compared to the previous one.
		/// </summary>
		/// <param name="connection"></param>
		private void UpdateSchema(SQLiteConnection connection)
		{
			string tableName;
			if (TryFindTable(connection, out tableName))
			{
				SQLiteSchema schema = GetSchema(connection, tableName);
				TryChangeSchema(schema);
			}
			else
			{
				Log.WarnFormat("Unable to find a fitting table in database {0}", _fileName);

				var schema = new SQLiteSchema(string.Empty);
				TryChangeSchema(schema);
			}
		}

		/// <summary>
		///     Changes the schema of this table, if necessary.
		/// </summary>
		/// <param name="schema"></param>
		private void TryChangeSchema(SQLiteSchema schema)
		{
			if (!Equals(_schema, schema))
			{
				Log.DebugFormat("Schema of {0} changed from {1} to {2}", _fileName,
				                _schema, schema);

				_schema = schema;
				_rowCount = 0;
				_cache.Remove(this);
				_listeners.OnSchemaChanged(_schema);
				_listeners.OnRead(LogEntryIndex.Invalid, 0);
			}
		}

		private void UpdateRowCount(SQLiteConnection connection)
		{
			int rowCount = Count(connection, _schema.TableName);
			if (rowCount < _rowCount)
			{
				int invalidateCount = _rowCount - rowCount;
				_rowCount = rowCount;
				_listeners.OnRead(rowCount, invalidateCount, true);
			}
			else if (rowCount > _rowCount)
			{
				int old = _rowCount;
				_rowCount = rowCount;
				_listeners.OnRead(old, rowCount - old);
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

		private static bool TryFindTable(SQLiteConnection connection, out string tableName)
		{
			// TODO: Find the proper table based on name and maybe columns...
			tableName = "log";
			return true;
		}

		private static SQLiteSchema GetSchema(SQLiteConnection connection, string tableName)
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

				return new SQLiteSchema(tableName, columns);
			}
		}
	}
}