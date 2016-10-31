using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class SqLiteLogTableAcceptanceTest
	{
		private ManualTaskScheduler _scheduler;
		private SqLiteLogTable _table;
		private string _fileName;
		private SQLiteConnection _connection;

		[SetUp]
		public void SetUp()
		{
			_scheduler = new ManualTaskScheduler();

			_fileName = GetFileName();
			_table = new SqLiteLogTable(_scheduler, _fileName);
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

		private void CreateTable()
		{
			const string sql = "CREATE TABLE log (timestamp DATETIME, thread TEXT, level TEXT, logger TEXT, message TEXT)";
			var command = new SQLiteCommand(sql, _connection);
			command.ExecuteNonQuery();
		}

		[Test]
		public void TestTableHeader()
		{
			CreateDatabase();
			CreateTable();

			_scheduler.RunOnce();

			_table.Schema.ColumnHeaders.Select(x => x.Name).Should().Equal(new[]
				{
					"timestamp",
					"thread",
					"level",
					"logger",
					"message"
				});
		}
	}
}