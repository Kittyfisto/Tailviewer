using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogFileListenerNotifierTest
	{
		private Mock<ILogFileListener> _listener;
		private List<LogFileSection> _changes;

		[SetUp]
		public void SetUp()
		{
			_listener = new Mock<ILogFileListener>();
			_changes = new List<LogFileSection>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<LogFileSection>()))
			         .Callback((LogFileSection section) => _changes.Add(section));
		}

		[Test]
		public void TestCurrentLineChanged1()
		{
			var notifier = new LogFileListenerNotifier(_listener.Object, TimeSpan.Zero, 1);
			notifier.OnRead(1);
			notifier.OnRead(2);
			notifier.OnRead(3);
			notifier.OnRead(4);

			_changes.Should().Equal(new[]
				{
					new LogFileSection(0, 1),
					new LogFileSection(1, 1),
					new LogFileSection(2, 1),
					new LogFileSection(3, 1)
				});
		}

		[Test]
		public void TestCurrentLineChanged2()
		{
			var notifier = new LogFileListenerNotifier(_listener.Object, TimeSpan.FromHours(1), 4);
			notifier.OnRead(1);
			_changes.Should().BeEmpty();
			notifier.OnRead(2);
			_changes.Should().BeEmpty();
			notifier.OnRead(3);
			_changes.Should().BeEmpty();

			notifier.OnRead(4);
			_changes.Should().Equal(new[]
				{
					new LogFileSection(0, 4)
				});
		}

		[Test]
		public void TestCurrentLineChanged3()
		{
			var notifier = new LogFileListenerNotifier(_listener.Object, TimeSpan.FromHours(1), 4);
			notifier.OnRead(4);
			_changes.Should().Equal(new[]
				{
					new LogFileSection(0, 4)
				});
		}

		[Test]
		public void TestCurrentLineChanged4()
		{
			var notifier = new LogFileListenerNotifier(_listener.Object, TimeSpan.FromHours(1), 1000);
			notifier.OnRead(1000);
			_changes.Should().Equal(new[]
				{
					new LogFileSection(0, 1000)
				});
			notifier.OnRead(2000);
			_changes.Should().Equal(new[]
				{
					new LogFileSection(0, 1000),
					new LogFileSection(1000, 1000)
				});
		}

		[Test]
		public void TestCurrentLineChanged5()
		{
			var notifier = new LogFileListenerNotifier(_listener.Object, TimeSpan.FromHours(1), 1000);
			notifier.OnRead(2000);
			_changes.Should().Equal(new[]
				{
					new LogFileSection(0, 1000),
					new LogFileSection(1000, 1000)
				});
		}
	}
}