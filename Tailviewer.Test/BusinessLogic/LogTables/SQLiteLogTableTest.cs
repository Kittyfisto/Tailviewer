using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class SQLiteLogTableTest
	{
		[Test]
		public void TestCtor1()
		{
			var table = new SQLiteLogTable(new ManualTaskScheduler(), "foo.db");
			table.RowCount.Should().Be(0, "Because the database doesn't exist and thus no data could've been retrieved");
			table.Schema.Should().NotBeNull();
			table.Schema.TableName.Should().BeEmpty("Because the database doesn't exist and thus no table name could be known");
			table.Schema.ColumnHeaders.Should().NotBeNull();
			table.Schema.ColumnHeaders.Should().BeEmpty();
		}

		[Test]
		public void TestCtor2()
		{
			new Action(() => new SQLiteLogTable(null, "foo.txt")).ShouldThrow<ArgumentNullException>();
			new Action(() => new SQLiteLogTable(new ManualTaskScheduler(), null)).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestAddListener1()
		{
			var table = new SQLiteLogTable(new ManualTaskScheduler(), "foo.db");
			var listener = new Mock<ILogTableListener>();
			new Action(() => table.AddListener(listener.Object, TimeSpan.Zero, 100)).ShouldNotThrow();
			table.RemoveListener(listener.Object).Should().BeTrue();
		}

		[Test]
		public void TestAddListener2()
		{
			var table = new SQLiteLogTable(new ManualTaskScheduler(), "foo.db");
			var listener1 = new Mock<ILogTableListener>();
			var listener2 = new Mock<ILogTableListener>();
			table.AddListener(listener1.Object, TimeSpan.Zero, 100);
			table.AddListener(listener2.Object, TimeSpan.Zero, 200);

			table.RemoveListener(listener1.Object).Should().BeTrue("Because we should've successfully removed this listener");
			table.RemoveListener(listener1.Object).Should().BeFalse("Because this listener is no longer part of the collection and thus removing it should've failed");

			table.RemoveListener(listener2.Object).Should().BeTrue("Because we should've successfully removed this listener");
			table.RemoveListener(listener2.Object).Should().BeFalse("Because this listener is no longer part of the collection and thus removing it should've failed");
		}
	}
}