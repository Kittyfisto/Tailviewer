using System;
using System.Data.SQLite;
using log4net.Appender;
using log4net.Core;

namespace SQLiteLogger
{
	public sealed class SqLiteAppender
		: IAppender
	{
		private readonly SQLiteConnection _connection;

		public SqLiteAppender(SQLiteConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException("connection");

			_connection = connection;

			const string sql = "CREATE TABLE log (timestamp DATETIME, thread TEXT, level TEXT, logger TEXT, message TEXT)";
			var command = new SQLiteCommand(sql, _connection);
			command.ExecuteNonQuery();
		}

		public void Close()
		{
			
		}

		public void DoAppend(LoggingEvent loggingEvent)
		{
			try
			{
				const string sql = "INSERT INTO log (timestamp, thread, level, logger, message) VALUES (datetime(),@1,@2,@3,@4)";
				var command = new SQLiteCommand(sql, _connection);
				// For the sake of this logger it doesn't matter if we log the timestamp of the event or the time when we execute the query.
				//command.Parameters.Add(loggingEvent.TimeStamp);
				command.Parameters.AddWithValue("@1", loggingEvent.ThreadName);
				command.Parameters.AddWithValue("@2", loggingEvent.Level.ToString());
				command.Parameters.AddWithValue("@3", loggingEvent.LoggerName);
				command.Parameters.AddWithValue("@4", loggingEvent.RenderedMessage);
				command.ExecuteNonQuery();
			}
			catch (Exception)
			{}
		}

		public string Name { get; set; }
	}
}