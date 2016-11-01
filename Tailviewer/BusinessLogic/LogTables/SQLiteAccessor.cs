using System;
using System.Data.SQLite;
using System.Diagnostics.Contracts;

namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	///     Provides read-only access to a specific table of an sqlite database.
	///     All lookups are first forwarded to the given cache. When a lookup could not
	///     be satisfied by the cache then the database is queried.
	/// </summary>
	public sealed class SQLiteAccessor
		: ILogDataAccessor<LogEntryIndex, LogEntry>
	{
		private readonly LogDataCache _cache;
		private readonly SQLiteConnection _connection;
		private readonly ILogTable _logTable;
		private readonly SQLiteSchema _schema;

		/// <summary>
		/// </summary>
		/// <param name="connection">The connection to use - does not take ownership of the connection</param>
		/// <param name="schema"></param>
		/// <param name="cache"></param>
		/// <param name="logTable"></param>
		public SQLiteAccessor(SQLiteConnection connection, SQLiteSchema schema, LogDataCache cache, ILogTable logTable)
		{
			if (connection == null)
				throw new ArgumentNullException("connection");
			if (schema == null)
				throw new ArgumentNullException("schema");
			if (cache == null)
				throw new ArgumentNullException("cache");
			if (logTable == null)
				throw new ArgumentNullException("logTable");

			_connection = connection;
			_schema = schema;
			_cache = cache;
			_logTable = logTable;
		}

		public bool TryAccess(LogEntryIndex index, out LogEntry data)
		{
			if (_cache.TryGetValue(_logTable, index, out data))
			{
				return true;
			}

			if (TryQuery(index, out data))
			{
				_cache.Add(_logTable, index, data);
				return true;
			}

			// The data is really not there (maybe the datasource just got deleted/flushed).
			return false;
		}

		private bool TryQuery(LogEntryIndex index, out LogEntry entry)
		{
			// This might be subject to change:
			// On the one hand, bulk access to retrieve data from the database is surely
			// a clever thing to do (in order to improve performance).
			// On the other hand, we don't want to scan the entire database when we're
			// accessing data at the same interval as we're requesting in one go.
			// I see two possible solutions:
			// - Either LogDataAccessQueue becomes smart enough batching access to consecutive regions
			// OR
			// - We retrieve entries in bulk
			// I prefer to the first solution, but we'll have to measure this first...

			string sql = string.Format("SELECT * FROM {0} LIMIT {1} OFFSET {2}", _schema.TableName, 1, index.Value);
			using (var command = new SQLiteCommand(sql, _connection))
			using (SQLiteDataReader reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					entry = ReadRow(reader);
					return true;
				}

				entry = default(LogEntry);
				return false;
			}
		}

		[Pure]
		private LogEntry ReadRow(SQLiteDataReader reader)
		{
			var fields = new object[reader.FieldCount];
			for (int i = 0; i < fields.Length; ++i)
			{
				fields[i] = reader.GetValue(i);
			}
			return new LogEntry(fields);
		}
	}
}