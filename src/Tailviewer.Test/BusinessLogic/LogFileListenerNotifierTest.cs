using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Sources;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogFileListenerNotifierTest
	{
		[SetUp]
		public void SetUp()
		{
			_logFile = new Mock<ILogSource>();
			_listener = new Mock<ILogSourceListener>();
			_changes = new List<LogFileSection>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogFileSection>()))
			         .Callback((ILogSource file, LogFileSection section) => _changes.Add(section));
		}

		private Mock<ILogSourceListener> _listener;
		private List<LogFileSection> _changes;
		private Mock<ILogSource> _logFile;

		[Test]
		public void TestCurrentLineChanged1()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.Zero, 1);
			notifier.OnRead(1);
			notifier.OnRead(2);
			notifier.OnRead(3);
			notifier.OnRead(4);

			_changes.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 1),
					new LogFileSection(1, 1),
					new LogFileSection(2, 1),
					new LogFileSection(3, 1)
				});
		}

		[Test]
		public void TestCurrentLineChanged2()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromHours(1), 4);
			_changes.Clear();

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
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromHours(1), 4);
			notifier.OnRead(4);
			_changes.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 4)
				});
		}

		[Test]
		public void TestCurrentLineChanged4()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromHours(1), 1000);
			notifier.OnRead(1000);
			_changes.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 1000)
				});
			notifier.OnRead(2000);
			_changes.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 1000),
					new LogFileSection(1000, 1000)
				});
		}

		[Test]
		public void TestCurrentLineChanged5()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromHours(1), 1000);
			notifier.OnRead(2000);
			_changes.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 1000),
					new LogFileSection(1000, 1000)
				});
		}

		[Test]
		public void TestCurrentLineChanged6()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromHours(1), 1000);
			notifier.OnRead(-1);
			_changes.Should().Equal(new[]
				{
					LogFileSection.Reset
				});
		}

		[Test]
		public void TestInvalidate1()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.Zero, 1);
			notifier.OnRead(1);
			notifier.Invalidate(0, 1);
			_changes.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 1),
					LogFileSection.Invalidate(0, 1)
				});
			notifier.LastNumberOfLines.Should().Be(0);
		}

		[Test]
		[Description(
			"Verifies that the Invalidate() arguments are adjusted to reflect the changes that were actually propagated")]
		public void TestInvalidate2()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromSeconds(1), 10);
			notifier.OnRead(10);
			notifier.OnRead(12);
			notifier.Invalidate(0, 12);
			_changes.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 10),
					LogFileSection.Invalidate(0, 10)
				},
			                        "Because the notifier should've reported only the first 10 changes and therefore Invalidate() only had to invalidate those 10 changes"
				);
			notifier.LastNumberOfLines.Should().Be(0);
		}

		[Test]
		[Description(
			"Verifies that the Invalidate() arguments are adjusted to reflect the changes that were actually propagated")]
		public void TestInvalidate3()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromSeconds(1), 10);
			notifier.OnRead(10);
			notifier.OnRead(20);
			notifier.OnRead(22);
			notifier.Invalidate(0, 22);
			_changes.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 10),
					new LogFileSection(10, 10),
					LogFileSection.Invalidate(0, 20)
				},
			                        "Because the notifier should've reported only the first 10 changes and therefore Invalidate() only had to invalidate those 10 changes"
				);
			notifier.LastNumberOfLines.Should().Be(0);
		}

		[Test]
		[Description(
			"Verifies that the Invalidate() arguments are adjusted to reflect the changes that were actually propagated")]
		public void TestInvalidate4()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromMilliseconds(100), 100);
			notifier.OnRead(9);
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			notifier.OnRead(9);
			_changes.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 9)
				});

			notifier.OnRead(35);
			notifier.Invalidate(10, 25);
			_changes.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 9)
				},
			                        "Because the notifier should've reported only the first 10 changes and therefore Invalidate() only had to invalidate those 10 changes"
				);
			notifier.LastNumberOfLines.Should().Be(9);
		}

		[Test]
		[Description("Verifies that only the first of subsequent sequential reset events is forwarded")]
		public void TestInvalidate5()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromMilliseconds(100), 100);
			notifier.OnRead(1);
			notifier.OnRead(-1);
			_changes.Should().Equal(new[] {LogFileSection.Reset});
			notifier.OnRead(-1);
			_changes.Should().Equal(new[] {LogFileSection.Reset});
			notifier.OnRead(-1);
			_changes.Should().Equal(new[] {LogFileSection.Reset});
		}
	}
}