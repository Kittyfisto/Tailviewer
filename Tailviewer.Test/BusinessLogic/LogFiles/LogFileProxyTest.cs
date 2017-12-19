using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFileProxyTest
		: AbstractLogFileTest
	{
		private Mock<ILogFile> _logFile;
		private LogFileListenerCollection _listeners;
		private Mock<ILogFileListener> _listener;
		private List<LogFileSection> _modifications;
		private ManualTaskScheduler _scheduler;

		[SetUp]
		public void Setup()
		{
			_scheduler = new ManualTaskScheduler();

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
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero))
			{
				proxy.InnerLogFile.Should().BeNull();
				proxy.MaxCharactersPerLine.Should().Be(0);
				proxy.Error.Should().Be(ErrorFlags.SourceDoesNotExist);
				proxy.Size.Should().Be(Size.Zero);
				proxy.StartTimestamp.Should().NotHaveValue();
				proxy.Count.Should().Be(0);

				new Action(() => proxy.GetLine(0)).ShouldThrow<IndexOutOfRangeException>();
				new Action(() => proxy.GetSection(new LogFileSection(0, 1))).ShouldThrow<IndexOutOfRangeException>();
			}
		}

		[Test]
		[Description("Verifies that the proxy registers a listener on the inner log file")]
		public void TestCtor2()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Verify(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()), Times.Once);
			}
		}

		[Test]
		[Description("Verifies that changing the inner log file causes the proxy to unregister the previously registered listener from the old file")]
		public void TestInnerLogFile1()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.InnerLogFile = null;
				_logFile.Verify(x => x.RemoveListener(It.IsAny<ILogFileListener>()), Times.Once);
			}
		}

		[Test]
		public void TestGetLine()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.GetLine(42);
				_logFile.Verify(l => l.GetLine(It.Is<int>(x => x == 42)), Times.Once);
			}
		}

		[Test]
		public void TestGetSection()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.GetSection(new LogFileSection(42, 101), new LogLine[101]);

				var expected = new LogFileSection(42, 101);
				_logFile.Verify(l => l.GetSection(It.Is<LogFileSection>(x => Equals(x, expected)),
												 It.IsAny<LogLine[]>()), Times.Once);
			}
		}

		[Test]
		public void TestExists()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.Error).Returns(ErrorFlags.None);
				proxy.Error.Should().Be(ErrorFlags.None);
				_logFile.Setup(x => x.Error).Returns(ErrorFlags.SourceCannotBeAccessed);
				proxy.Error.Should().Be(ErrorFlags.SourceCannotBeAccessed);
			}
		}

		[Test]
		public void TestFileSize()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.Size).Returns(Size.FromBytes(12));
				proxy.Size.Should().Be(Size.FromBytes(12));
				_logFile.Setup(x => x.Size).Returns(Size.OneMegabyte);
				proxy.Size.Should().Be(Size.OneMegabyte);
			}
		}

		[Test]
		public void TestCount()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.Count).Returns(42);
				proxy.Count.Should().Be(42);
				_logFile.Setup(x => x.Count).Returns(9001);
				proxy.Count.Should().Be(9001);
			}
		}

		[Test]
		public void TestStartTimestamp()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.StartTimestamp).Returns(new DateTime(2016, 10, 7, 14, 46, 00));
				proxy.StartTimestamp.Should().Be(new DateTime(2016, 10, 7, 14, 46, 00));
				_logFile.Setup(x => x.StartTimestamp).Returns((DateTime?)null);
				proxy.StartTimestamp.Should().NotHaveValue();
			}
		}

		[Test]
		public void TestMaxCharactersPerLine()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.MaxCharactersPerLine).Returns(101);
				proxy.MaxCharactersPerLine.Should().Be(101);
				_logFile.Setup(x => x.MaxCharactersPerLine).Returns(42);
				proxy.MaxCharactersPerLine.Should().Be(42);
			}
		}

		[Test]
		public void TestDispose1()
		{
			var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero);
			_scheduler.PeriodicTaskCount.Should().Be(1);

			proxy.IsDisposed.Should().BeFalse();
			new Action(proxy.Dispose).ShouldNotThrow();
			proxy.IsDisposed.Should().BeTrue();
			_scheduler.PeriodicTaskCount.Should().Be(0);
		}

		[Test]
		public void TestDispose2()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.Dispose();
				_logFile.Verify(l => l.Dispose(), Times.Once);
			}
		}

		[Test]
		public void TestListen1()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

				_listeners.OnRead(500);
				_listeners.OnRead(600);

				_scheduler.RunOnce();

				_modifications.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 500),
					new LogFileSection(500, 100)
				});
			}
		}

		[Test]
		public void TestListen2()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

				_listeners.OnRead(500);
				_listeners.Reset();
				_listeners.OnRead(600);

				_scheduler.RunOnce();

				_modifications.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 500),
					LogFileSection.Reset,
					new LogFileSection(0, 600)
				});
			}
		}

		[Test]
		public void TestListen3()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

				_listeners.OnRead(500);
				_listeners.Invalidate(400, 100);
				_listeners.OnRead(550);

				_scheduler.RunOnce();

				_modifications.Should().Equal(new[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 500),
					LogFileSection.Invalidate(400, 100),
					new LogFileSection(400, 150)
				});
			}
		}

		[Test]
		[Description("Verifies that OnLogFileModified calls from log files that aren't the current inner one are ignored")]
		public void TestListen4()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

				new Action(() => proxy.OnLogFileModified(new Mock<ILogFile>().Object, new LogFileSection(0, 1))).ShouldNotThrow();
				_modifications.Should().Equal(new[] { LogFileSection.Reset }, "because the OnLogFileModified shouldn't have been forwarded since it's from the wrong source");

				new Action(() => proxy.OnLogFileModified(null, new LogFileSection(0, 1))).ShouldNotThrow();
				_modifications.Should().Equal(new[] { LogFileSection.Reset }, "because the OnLogFileModified shouldn't have been forwarded since it's from the wrong source");
			}
		}

		[Test]
		public void TestGetLogLineIndexOfOriginalLineIndex1()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetLogLineIndexOfOriginalLineIndex(It.Is<LogLineIndex>(y => y == 9001)))
					.Returns(42);

				proxy.GetLogLineIndexOfOriginalLineIndex(new LogLineIndex(9001))
					.Should()
					.Be(new LogLineIndex(42), "because the proxy should forward all requests to the inner log file, if available");
			}
		}

		[Test]
		public void TestGetLogLineIndexOfOriginalLineIndex2()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero))
			{
				proxy.GetLogLineIndexOfOriginalLineIndex(new LogLineIndex(9001))
					.Should()
					.Be(LogLineIndex.Invalid, "because the proxy should just return an invalid index when no inner log file is present");
			}
		}

		[Test]
		public void TestGetOriginalIndexFrom1()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero))
			{
				proxy.GetOriginalIndexFrom(new LogLineIndex(1))
					.Should().Be(LogLineIndex.Invalid);
			}
		}

		[Test]
		public void TestGetOriginalIndexFrom2()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetOriginalIndexFrom(It.Is<LogLineIndex>(y => y == new LogLineIndex(42))))
					.Returns(new LogLineIndex(9001));

				proxy.GetOriginalIndexFrom(new LogLineIndex(42))
					.Should().Be(new LogLineIndex(9001));
			}
		}

		[Test]
		public void TestGetOriginalIndicesFrom1()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero))
			{
				new Action(() => proxy.GetOriginalIndicesFrom(new LogFileSection(), null))
					.ShouldThrow<ArgumentNullException>();
			}
		}

		[Test]
		public void TestGetOriginalIndicesFrom2()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero))
			{
				new Action(() => proxy.GetOriginalIndicesFrom(new LogFileSection(1, 4), new LogLineIndex[3]))
					.ShouldThrow<ArgumentOutOfRangeException>("because the given array is too small");
			}
		}

		[Test]
		public void TestGetOriginalIndicesFrom3()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero))
			{
				var indices = new LogLineIndex[3];
				proxy.GetOriginalIndicesFrom(new LogFileSection(1, 3), indices);
				indices.Should().Equal(LogLineIndex.Invalid, LogLineIndex.Invalid, LogLineIndex.Invalid);
			}
		}

		[Test]
		public void TestGetOriginalIndicesFrom4()
		{
			using (var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetOriginalIndicesFrom(It.Is<LogFileSection>(
						y => y == new LogFileSection(1, 3)), It.IsAny<LogLineIndex[]>()))
					.Callback((LogFileSection section, LogLineIndex[] ind) =>
					{
						ind[0] = new LogLineIndex(0);
						ind[1] = new LogLineIndex(1);
						ind[2] = new LogLineIndex(2);
					});

				var indices = new LogLineIndex[3];
				proxy.GetOriginalIndicesFrom(new LogFileSection(1, 3), indices);
				indices.Should().Equal(new LogLineIndex(0), new LogLineIndex(1), new LogLineIndex(2));
			}
		}

		[Test]
		public void TestGetOriginalIndicesFrom5()
		{
			var logFile = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object);
			var indices = new LogLineIndex[5];
			var originalIndices = new LogLineIndex[5];
			logFile.GetOriginalIndicesFrom(indices, originalIndices);

			_logFile.Verify(x => x.GetOriginalIndicesFrom(It.Is<IReadOnlyList<LogLineIndex>>(y => y.Count == 5),
				It.Is<LogLineIndex[]>(y => y.Length == 5)), Times.Once);
		}

		[Test]
		public void TestGetOriginalIndicesFrom6()
		{
			var logFile = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object);
			new Action(() => logFile.GetOriginalIndicesFrom(null, new LogLineIndex[0]))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestGetOriginalIndicesFrom7()
		{
			var logFile = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object);
			new Action(() => logFile.GetOriginalIndicesFrom(new LogLineIndex[1], null))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestGetOriginalIndicesFrom8()
		{
			var logFile = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object);
			new Action(() => logFile.GetOriginalIndicesFrom(new LogLineIndex[5], new LogLineIndex[4]))
				.ShouldThrow<ArgumentOutOfRangeException>();
		}

		[Test]
		public void TestGetColumn1()
		{
			var section = new LogFileSection(42, 100);
			var buffer = new string[100];
			var logFile = new LogFileProxy(_scheduler, TimeSpan.Zero, _logFile.Object);
			logFile.GetColumn(section, LogFileColumns.RawContent, buffer, 42);
			_logFile.Verify(x => x.GetColumn(It.Is<LogFileSection>(y => y == section),
			                                 It.Is<ILogFileColumn<string>>(y => Equals(y, LogFileColumns.RawContent)),
			                                 It.Is<string[]>(y => ReferenceEquals(y, buffer)),
			                                 It.Is<int>(y => y == 42)),
			                Times.Once);
		}

		[Test]
		public void TestGetColumn2()
		{
			var section = new LogFileSection(42, 100);
			var buffer = new string[100];
			var logFile = new LogFileProxy(_scheduler, TimeSpan.Zero);
			logFile.GetColumn(section, LogFileColumns.RawContent, buffer);
			buffer.Should().OnlyContain(x => ReferenceEquals(x, null));
		}

		[Test]
		public void TestGetColumn3()
		{
			var indices = new LogLineIndex[] {1, 2};
			var buffer = new string[2];
			var logFile = new LogFileProxy(_scheduler, TimeSpan.Zero);
			logFile.GetColumn(indices, LogFileColumns.RawContent, buffer);
			buffer.Should().OnlyContain(x => ReferenceEquals(x, null));
		}

		[Test]
		public void TestProgress1()
		{
			var logFile = new LogFileProxy(_scheduler, TimeSpan.Zero);
			logFile.Progress.Should().Be(1);
		}

		[Test]
		public void TestProgress2()
		{
			var logFile = new LogFileProxy(_scheduler, TimeSpan.Zero);
			_logFile.Setup(x => x.Progress).Returns(0.5);
			logFile.InnerLogFile = _logFile.Object;
			logFile.Progress.Should().Be(0.5);
		}

		protected override ILogFile CreateEmpty()
		{
			return new LogFileProxy(_scheduler, TimeSpan.Zero);
		}

		protected override ILogFile CreateFromContent(IReadOnlyLogEntries content)
		{
			var source = new InMemoryLogFile(content.First().Columns);
			source.AddRange(content);
			var proxy = new LogFileProxy(_scheduler, TimeSpan.Zero, source);
			return proxy;
		}
	}
}