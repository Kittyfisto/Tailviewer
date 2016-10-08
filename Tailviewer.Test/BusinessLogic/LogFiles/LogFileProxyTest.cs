using System;
using System.Collections.Generic;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Scheduling;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFileProxyTest
		: AbstractTest
	{
		private Mock<ILogFile> _logFile;
		private LogFileListenerCollection _listeners;
		private Mock<ILogFileListener> _listener;
		private List<LogFileSection> _modifications;
		private TaskScheduler _scheduler;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_scheduler = new TaskScheduler();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_scheduler.Dispose();
		}

		[SetUp]
		public void Setup()
		{
			_logFile = new Mock<ILogFile>();
			_listeners = new LogFileListenerCollection(_logFile.Object);
			_logFile.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			        .Callback((ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount) => _listeners.AddListener(listener, maximumWaitTime, maximumLineCount));
			_logFile.Setup(x => x.RemoveListener(It.IsAny<ILogFileListener>()))
			        .Callback((ILogFileListener listener) => _listeners.RemoveListener(listener));

			_listener = new Mock<ILogFileListener>();
			_modifications = new List<LogFileSection>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
			         .Callback((ILogFile logFile, LogFileSection section) => _modifications.Add(section));
		}

		[Test]
		public void TestCtor1()
		{
			var proxy = new LogFileProxy(_scheduler);
			proxy.InnerLogFile.Should().BeNull();
			proxy.MaxCharactersPerLine.Should().Be(0);
			proxy.Exists.Should().BeFalse();
			proxy.FileSize.Should().Be(Size.Zero);
			proxy.StartTimestamp.Should().NotHaveValue();
			proxy.Count.Should().Be(0);

			new Action(() => proxy.GetLine(0)).ShouldThrow<IndexOutOfRangeException>();
			new Action(() => proxy.GetSection(new LogFileSection(0, 1))).ShouldThrow<IndexOutOfRangeException>();
		}

		[Test]
		[Description("Verifies that the proxy registers a listener on the inner log file")]
		public void TestCtor2()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			_logFile.Verify(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()), Times.Once);
		}

		[Test]
		[Description("Verifies that changing the inner log file causes the proxy to unregister the previously registered listener from the old file")]
		public void TestInnerLogFile1()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			proxy.InnerLogFile = null;
			_logFile.Verify(x => x.RemoveListener(It.IsAny<ILogFileListener>()), Times.Once);
		}

		[Test]
		public void TestGetLine()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			proxy.GetLine(42);
			_logFile.Verify(l => l.GetLine(It.Is<int>(x => x == 42)), Times.Once);
		}

		[Test]
		public void TestGetSection()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			proxy.GetSection(new LogFileSection(42, 101), new LogLine[101]);

			var expected = new LogFileSection(42, 101);
			_logFile.Verify(l => l.GetSection(It.Is<LogFileSection>(x => Equals(x, expected)),
			                                 It.IsAny<LogLine[]>()), Times.Once);
		}

		[Test]
		public void TestExists()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			_logFile.Setup(x => x.Exists).Returns(true);
			proxy.Exists.Should().BeTrue();
			_logFile.Setup(x => x.Exists).Returns(false);
			proxy.Exists.Should().BeFalse();
		}

		[Test]
		public void TestFileSize()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			_logFile.Setup(x => x.FileSize).Returns(Size.FromBytes(12));
			proxy.FileSize.Should().Be(Size.FromBytes(12));
			_logFile.Setup(x => x.FileSize).Returns(Size.OneMegabyte);
			proxy.FileSize.Should().Be(Size.OneMegabyte);
		}

		[Test]
		public void TestCount()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			_logFile.Setup(x => x.Count).Returns(42);
			proxy.Count.Should().Be(42);
			_logFile.Setup(x => x.Count).Returns(9001);
			proxy.Count.Should().Be(9001);
		}

		[Test]
		public void TestStartTimestamp()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			_logFile.Setup(x => x.StartTimestamp).Returns(new DateTime(2016, 10, 7, 14, 46, 00));
			proxy.StartTimestamp.Should().Be(new DateTime(2016, 10, 7, 14, 46, 00));
			_logFile.Setup(x => x.StartTimestamp).Returns((DateTime?)null);
			proxy.StartTimestamp.Should().NotHaveValue();
		}

		[Test]
		public void TestMaxCharactersPerLine()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			_logFile.Setup(x => x.MaxCharactersPerLine).Returns(101);
			proxy.MaxCharactersPerLine.Should().Be(101);
			_logFile.Setup(x => x.MaxCharactersPerLine).Returns(42);
			proxy.MaxCharactersPerLine.Should().Be(42);
		}

		[Test]
		public void TestDispose1()
		{
			var proxy = new LogFileProxy(_scheduler);
			new Action(proxy.Dispose).ShouldNotThrow();
		}

		[Test]
		public void TestDispose2()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			proxy.Dispose();
			_logFile.Verify(l => l.Dispose(), Times.Once);
		}

		[Test]
		public void TestListen1()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

			_listeners.OnRead(500);
			_listeners.OnRead(600);
			_modifications.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 500),
					new LogFileSection(500, 100)
				});
		}

		[Test]
		public void TestListen2()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

			_listeners.OnRead(500);
			_listeners.Reset();
			_listeners.OnRead(600);
			_modifications.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 500),
					LogFileSection.Reset,
					new LogFileSection(0, 600)
				});
		}

		[Test]
		public void TestListen3()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

			_listeners.OnRead(500);
			_listeners.Invalidate(400, 100);
			_listeners.OnRead(550);
			_modifications.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 500),
					new LogFileSection(400, 100, true),
					new LogFileSection(400, 150)
				});
		}

		[Test]
		[Description("Verifies that OnLogFileModified calls from log files that aren't the current inner one are ignored")]
		public void TestListen4()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

			new Action(() => proxy.OnLogFileModified(new Mock<ILogFile>().Object, new LogFileSection(0, 1))).ShouldNotThrow();
			_modifications.Should().Equal(new[] {LogFileSection.Reset}, "because the OnLogFileModified shouldn't have been forwarded since it's from the wrong source");

			new Action(() => proxy.OnLogFileModified(null, new LogFileSection(0, 1))).ShouldNotThrow();
			_modifications.Should().Equal(new[] {LogFileSection.Reset}, "because the OnLogFileModified shouldn't have been forwarded since it's from the wrong source");
		}

		[Test]
		[Description("Verifies that OnlogFileModified is eventually called when a non-zero maximum wait time is used (and the max limit is not reached)")]
		public void TestListen5()
		{
			var proxy = new LogFileProxy(_scheduler, _logFile.Object);
			proxy.AddListener(_listener.Object, TimeSpan.FromSeconds(1), 1000);
			proxy.OnLogFileModified(_logFile.Object, new LogFileSection(0, 1));

			WaitUntil(() => _modifications.Count == 2, TimeSpan.FromSeconds(50))
				.Should().BeTrue("because the changes should've eventually been forwarded to the listener");
			_modifications.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 1)
				});
		}
	}
}