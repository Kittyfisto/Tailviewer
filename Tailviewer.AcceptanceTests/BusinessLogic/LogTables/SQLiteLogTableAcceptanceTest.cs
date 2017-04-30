using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class SQLiteLogTableAcceptanceTest
	{
		private ManualTaskScheduler _scheduler;
		private SQLiteLogTable _table;
		private string _fileName;
		private SQLiteConnection _connection;
		private LogDataCache _cache;

		[SetUp]
		public void SetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_cache = new LogDataCache();

			_fileName = GetFileName();
			_table = new SQLiteLogTable(_scheduler, _cache, _fileName);
		}

		private static string GetFileName()
		{
			var fileName = string.Format("{0}.db", TestContext.CurrentContext.Test.Name);
			var fullFileName = Path.Combine(Path.GetTempPath(), "Tailviewer", fileName);
			var directory = Path.GetDirectoryName(fullFileName);

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(fullFileName);
			}
			else
			{
				if (File.Exists(fullFileName))
					File.Delete(fullFileName);
			}

			return fullFileName;
		}

		private void CreateDatabase()
		{
			SQLiteConnection.CreateFile(_fileName);
			var connectionString = string.Format("Data Source={0};Version=3;", _fileName);
			_connection = new SQLiteConnection(connectionString);
			_connection.Open();
		}

		private void DeleteDatabase()
		{
			if (_connection != null)
			{
				_connection.Dispose();
				_connection.Dispose();
			}

			if (File.Exists(_fileName))
				File.Delete(_fileName);
		}

		private void CreateTable()
		{
			const string sql = "CREATE TABLE log (timestamp DATETIME, thread TEXT, level TEXT, logger TEXT, message TEXT)";
			var command = new SQLiteCommand(sql, _connection);
			command.ExecuteNonQuery();
		}

		private void AddRow(DateTime timestamp, string thread, string level, string logger, string message)
		{
			try
			{
				const string sql = "INSERT INTO log (timestamp, thread, level, logger, message) VALUES (@1,@2,@3,@4,@5)";
				var command = new SQLiteCommand(sql, _connection);
				command.Parameters.AddWithValue("@1", timestamp);
				command.Parameters.AddWithValue("@2", thread);
				command.Parameters.AddWithValue("@3", level);
				command.Parameters.AddWithValue("@4", logger);
				command.Parameters.AddWithValue("@5", message);
				command.ExecuteNonQuery();
			}
			catch (Exception)
			{ }
		}

		[Test]
		[Description("Verifies that the table doesn't throw when database doesn't exist")]
		public void TestExists1()
		{
			var task = _scheduler.PeriodicTasks.First();

			_scheduler.RunOnce();
			_table.Exists.Should().BeFalse("because the database still doesn't exist");

			task.NumFailures.Should().Be(0, "Because the implementation shouldn't have thrown an exception while updating");
		}

		[Test]
		[Description("Verifies that when the database exists, then the Exists property is set to true")]
		public void TestExists2()
		{
			CreateDatabase();

			_scheduler.RunOnce();

			// What do we actually want to achieve with this property.
			// Test if the file exists on disk or if the actual table is present?
			// For now I'm going to assume that the file exists, but maybe we need
			// a more refined system in the future that allows us to deal
			// with more errors than this...
			_table.Exists.Should().BeTrue();
		}

		[Test]
		[Description("Verifies that when the database is deleted, then the Exists property is set to false again")]
		public void TestExists3()
		{
			CreateDatabase();
			_scheduler.RunOnce();
			_table.Exists.Should().BeTrue();

			DeleteDatabase();
			_scheduler.RunOnce();
			_table.Exists.Should().BeFalse("Because the database has been deleted and thus the log table should've changed the property");
		}

		[Test]
		[Description("Verifies that when the table runs for the first time, it automatically retrieves the schema for the log table")]
		public void TestTableHeader()
		{
			CreateDatabase();
			CreateTable();

			var oldSchema = _table.Schema;

			_scheduler.RunOnce();

			_table.Schema.Should().NotBeNull();
			_table.Schema.Should().NotBeSameAs(oldSchema, "because a schema is supposed to be immutable and thus the table should've created a new schema");
			_table.Schema.TableName.Should().Be("log");
			_table.Schema.ColumnHeaders.ElementAt(0).Should().Be(new SQLiteColumnHeader("timestamp", SQLiteDataType.DateTime));
			_table.Schema.ColumnHeaders.ElementAt(1).Should().Be(new SQLiteColumnHeader("thread", SQLiteDataType.Text));
			_table.Schema.ColumnHeaders.ElementAt(2).Should().Be(new SQLiteColumnHeader("level", SQLiteDataType.Text));
			_table.Schema.ColumnHeaders.ElementAt(3).Should().Be(new SQLiteColumnHeader("logger", SQLiteDataType.Text));
			_table.Schema.ColumnHeaders.ElementAt(4).Should().Be(new SQLiteColumnHeader("message", SQLiteDataType.Text));
		}

		[Test]
		[Description("Verifies that the table is able to detect changes to the database")]
		public void TestOneLogMessage()
		{
			CreateDatabase();
			CreateTable();

			AddRow(new DateTime(2016, 11, 1, 13, 06, 00), "1", "DEBUG", "Tailviewer.AcceptanceTest", "It's done");

			_scheduler.RunOnce();
			_table.Count.Should().Be(1, "Because the table should've retrieved the first row of the table");

			var task = _table[0];
			task.Should().NotBeNull();
			task.IsCompleted.Should().BeFalse("Because the access will be satisfied with the next dispatcher invocation");

			_scheduler.RunOnce();
			task.IsCompleted.Should().BeTrue();
			var entry = task.Result;
			entry.Should().Be(new LogEntry(
				                  new DateTime(2016, 11, 1, 13, 06, 00), "1", "DEBUG", "Tailviewer.AcceptanceTest", "It's done"
				                  ));
		}
	}
}