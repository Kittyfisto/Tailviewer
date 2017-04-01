using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class SQLiteLogTableTest
	{
		private ManualTaskScheduler _scheduler;
		private LogDataCache _cache;

		[SetUp]
		public void SetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_cache = new LogDataCache();
		}

		[Test]
		public void TestCtor1()
		{
			var table = new SQLiteLogTable(_scheduler, _cache, "foo.db");
			table.Exists.Should().BeFalse("Because we haven't checked for the existance and yet, and not existing is the default assumption, for now");
			table.RowCount.Should().Be(0, "Because the database doesn't exist and thus no data could've been retrieved");
			table.Schema.Should().NotBeNull();
			table.Schema.TableName.Should().BeEmpty("Because the database doesn't exist and thus no table name could be known");
			table.Schema.ColumnHeaders.Should().NotBeNull();
			table.Schema.ColumnHeaders.Should().BeEmpty();
		}

		[Test]
		public void TestCtor2()
		{
			new Action(() => new SQLiteLogTable(null, _cache, "foo.txt")).ShouldThrow<ArgumentNullException>();
			new Action(() => new SQLiteLogTable(_scheduler, null, "foo.db")).ShouldThrow<ArgumentNullException>();
			new Action(() => new SQLiteLogTable(_scheduler, _cache, null)).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that the constructor creates a periodic task")]
		public void TestCtor3()
		{
			_scheduler.PeriodicTasks.Should().BeEmpty();

			var table = new SQLiteLogTable(_scheduler, _cache, @"C:\bar\foo.db");

			_scheduler.PeriodicTaskCount.Should().Be(1);
			var task = _scheduler.PeriodicTasks.First();
			task.Name.Should().Be(@"C:\bar\foo.db", "because the implementation should've given the task a descriptive name");
		}

		[Test]
		[Description("Verifies that Dispose() removes the periodic task created by the ctor")]
		public void TestDispose()
		{
			_scheduler.PeriodicTasks.Should().BeEmpty();

			var table = new SQLiteLogTable(_scheduler, _cache, @"C:\bar\foo.db");
			_scheduler.PeriodicTaskCount.Should().Be(1);
			table.Dispose();
			_scheduler.PeriodicTaskCount.Should().Be(0);
		}

		[Test]
		public void TestAddListener1()
		{
			var table = new SQLiteLogTable(_scheduler, _cache, "foo.db");
			var listener = new Mock<ILogTableListener>();
			new Action(() => table.AddListener(listener.Object, TimeSpan.Zero, 100)).ShouldNotThrow();
			table.RemoveListener(listener.Object).Should().BeTrue();
		}

		[Test]
		public void TestAddListener2()
		{
			var table = new SQLiteLogTable(_scheduler, _cache, "foo.db");
			var listener1 = new Mock<ILogTableListener>();
			var listener2 = new Mock<ILogTableListener>();
			table.AddListener(listener1.Object, TimeSpan.Zero, 100);
			table.AddListener(listener2.Object, TimeSpan.Zero, 200);

			table.RemoveListener(listener1.Object).Should().BeTrue("Because we should've successfully removed this listener");
			table.RemoveListener(listener1.Object).Should().BeFalse("Because this listener is no longer part of the collection and thus removing it should've failed");

			table.RemoveListener(listener2.Object).Should().BeTrue("Because we should've successfully removed this listener");
			table.RemoveListener(listener2.Object).Should().BeFalse("Because this listener is no longer part of the collection and thus removing it should've failed");
		}

		[Test]
		public void TestToString()
		{
			var table = new SQLiteLogTable(_scheduler, _cache, "mydatabase.db");
			table.ToString().Should().Be("mydatabase.db");
		}
	}
}