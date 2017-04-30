using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogTables;
using Tailviewer.BusinessLogic.LogTables.Parsers;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class InMemoryLogTableTest
	{
		private Mock<ILogTableListener> _listener;
		private List<LogTableModification> _modifications;

		[SetUp]
		public void Setup()
		{
			_modifications = new List<LogTableModification>();
			_listener = new Mock<ILogTableListener>();
			_listener.Setup(x => x.OnLogTableModified(It.IsAny<ILogTable>(), It.IsAny<LogTableModification>()))
				.Callback((ILogTable table, LogTableModification modification) =>
				{
					_modifications.Add(modification);
				});
		}

		[Test]
		public void TestConstruction()
		{
			var table = new InMemoryLogTable();
			table.Exists.Should().BeTrue();
			table.LastModified.Should().Be(DateTime.MinValue);
			table.Count.Should().Be(0);
			table.Schema.Should().NotBeNull();
			table.Schema.ColumnHeaders.Should().NotBeNull();
			table.Schema.ColumnHeaders.Should().BeEmpty();
		}

		[Test]
		public void TestAddEntry1()
		{
			var table = new InMemoryLogTable();
			table.AddEntry(new LogEntry());
			table.Count.Should().Be(1);
		}

		[Test]
		public void TestAddEntry2()
		{
			var table = new InMemoryLogTable(new ColumnHeader("Name"));
			table.AddEntry(new LogEntry("Foo"));
			var task = table[0];
			task.Should().NotBeNull();
			task.Wait(TimeSpan.FromSeconds(1));
			var entry = task.Result;
			entry.Fields.Should().Equal(new object[] {"Foo"});
		}

		[Test]
		public void TestAddEntry3()
		{
			var table = new InMemoryLogTable();
			table.AddListener(_listener.Object, TimeSpan.Zero, 1);
			table.AddEntry(new LogEntry());
			_modifications.Should()
				.Equal(new object[]
				{
					LogTableModification.Reset,
					new LogTableModification(0, 1)
				});
		}
	}
}