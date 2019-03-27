using System;
using System.Data.SQLite;
using System.IO;
using LoggerCore;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace SQLiteLogger
{
	class Application
		: LoggerApplication
	{
		private readonly SQLiteConnection _connection;
		private readonly Hierarchy _hierarchy;
		private readonly SqLiteAppender _appender;

		public Application()
		{
			var fileName = Path.Combine(Directory.GetCurrentDirectory(), @"..\Live\SQLiteLogger.db");
			var connectionString = string.Format("Data Source={0};Version=3;", fileName);

			if (File.Exists(fileName))
				File.Delete(fileName);

			SQLiteConnection.CreateFile(fileName);
			_connection = new SQLiteConnection(connectionString);
			_connection.Open();

			Console.WriteLine("Logging to {0}", fileName);
			_hierarchy = (Hierarchy)LogManager.GetRepository();

			_appender = new SqLiteAppender(_connection);
			_hierarchy.Root.AddAppender(_appender);

			_hierarchy.Root.Level = Level.Debug;
			_hierarchy.Configured = true;
		}

		public override void Dispose()
		{
			_connection.Dispose();
			base.Dispose();
		}

		static int Main(string[] args)
		{
			return Run<Application>();
		}
	}
}
