using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Core.LogTables;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class LogTableListenerCollectionTest
	{
		private Mock<ILogTable> _table;
		private LogTableListenerCollection _collection;
		private Mock<ILogTableListener> _listener;
		private List<LogTableModification> _modifications;

		[SetUp]
		public void Setup()
		{
			_table = new Mock<ILogTable>();
			_collection = new LogTableListenerCollection(_table.Object);

			_modifications = new List<LogTableModification>();
			_listener = new Mock<ILogTableListener>();
			_listener.Setup(x => x.OnLogTableModified(It.IsAny<ILogTable>(), It.IsAny<LogTableModification>()))
				.Callback((ILogTable _table, LogTableModification modification) =>
				{
					_modifications.Add(modification);
				});
		}

		[Test]
		public void TestAddListener1()
		{
			_collection.AddListener(_listener.Object, TimeSpan.Zero, 1);
			_modifications.Should().Equal(LogTableModification.Reset);
		}

		[Test]
		public void TestAddListener2()
		{
			_collection.AddListener(_listener.Object, TimeSpan.Zero, 1);
			new Action(() => _collection.AddListener(_listener.Object, TimeSpan.Zero, 1)).Should().NotThrow();

			_modifications.Should().Equal(new object[] {LogTableModification.Reset},
				"because the listener should've only been added once");

			_collection.RemoveListener(_listener.Object);
			_collection.OnRead(1);

			_modifications.Should().Equal(new object[] { LogTableModification.Reset },
				"because the listener shouldn't retrieve any more callbacks");
		}
	}
}