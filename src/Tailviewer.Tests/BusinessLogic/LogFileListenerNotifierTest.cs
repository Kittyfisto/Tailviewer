using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core.Sources;

namespace Tailviewer.Tests.BusinessLogic
{
	[TestFixture]
	public sealed class LogFileListenerNotifierTest
	{
		[SetUp]
		public void SetUp()
		{
			_logFile = new Mock<ILogSource>();
			_listener = new Mock<ILogSourceListener>();
			_modifications = new List<LogSourceModification>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
			         .Callback((ILogSource file, LogSourceModification modification) => _modifications.Add(modification));
		}

		private Mock<ILogSourceListener> _listener;
		private List<LogSourceModification> _modifications;
		private Mock<ILogSource> _logFile;

		[Test]
		public void TestCurrentLineChanged1()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.Zero, 1);
			notifier.OnRead(1);
			notifier.OnRead(2);
			notifier.OnRead(3);
			notifier.OnRead(4);

			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset(),
					LogSourceModification.Appended(0, 1),
					LogSourceModification.Appended(1, 1),
					LogSourceModification.Appended(2, 1),
					LogSourceModification.Appended(3, 1)
				});
		}

		[Test]
		public void TestCurrentLineChanged2()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromHours(1), 4);
			_modifications.Clear();

			notifier.OnRead(1);
			_modifications.Should().BeEmpty();
			notifier.OnRead(2);
			_modifications.Should().BeEmpty();
			notifier.OnRead(3);
			_modifications.Should().BeEmpty();

			notifier.OnRead(4);
			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Appended(0, 4)
				});
		}

		[Test]
		public void TestCurrentLineChanged3()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromHours(1), 4);
			notifier.OnRead(4);
			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset(),
					LogSourceModification.Appended(0, 4)
				});
		}

		[Test]
		public void TestCurrentLineChanged4()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromHours(1), 1000);
			notifier.OnRead(1000);
			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset(),
					LogSourceModification.Appended(0, 1000)
				});
			notifier.OnRead(2000);
			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset(),
					LogSourceModification.Appended(0, 1000),
					LogSourceModification.Appended(1000, 1000)
				});
		}

		[Test]
		public void TestCurrentLineChanged5()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromHours(1), 1000);
			notifier.OnRead(2000);
			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset(),
					LogSourceModification.Appended(0, 1000),
					LogSourceModification.Appended(1000, 1000)
				});
		}

		[Test]
		public void TestCurrentLineChanged6()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.FromHours(1), 1000);
			notifier.OnRead(-1);
			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset()
				});
		}

		[Test]
		public void TestInvalidate1()
		{
			var notifier = new LogSourceListenerNotifier(_logFile.Object, _listener.Object, TimeSpan.Zero, 1);
			notifier.OnRead(1);
			notifier.Remove(0, 1);
			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset(),
					LogSourceModification.Appended(0, 1),
					LogSourceModification.Removed(0, 1)
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
			notifier.Remove(0, 12);
			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset(),
					LogSourceModification.Appended(0, 10),
					LogSourceModification.Removed(0, 10)
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
			notifier.Remove(0, 22);
			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset(),
					LogSourceModification.Appended(0, 10),
					LogSourceModification.Appended(10, 10),
					LogSourceModification.Removed(0, 20)
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
			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset(),
					LogSourceModification.Appended(0, 9)
				});

			notifier.OnRead(35);
			notifier.Remove(10, 25);
			_modifications.Should().Equal(new[]
				{
					LogSourceModification.Reset(),
					LogSourceModification.Appended(0, 9)
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
			_modifications.Should().Equal(new[] {LogSourceModification.Reset()});
			notifier.OnRead(-1);
			_modifications.Should().Equal(new[] {LogSourceModification.Reset()});
			notifier.OnRead(-1);
			_modifications.Should().Equal(new[] {LogSourceModification.Reset()});
		}
	}
}