using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Proxy
{
	[TestFixture]
	public sealed class LogFileProxyTest
		: AbstractLogFileTest
	{
		private Mock<ILogFile> _logFile;
		private LogFileListenerCollection _listeners;
		private Mock<ILogFileListener> _listener;
		private List<LogFileSection> _modifications;
		private ManualTaskScheduler _taskScheduler;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();

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
		public void TestEmptyConstruction()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero))
			{
				proxy.InnerLogFile.Should().BeNull();
				proxy.GetValue(TextLogFileProperties.MaxCharactersInLine).Should().Be(0);
				proxy.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);
				proxy.GetValue(LogFileProperties.Size).Should().BeNull();
				proxy.GetValue(LogFileProperties.StartTimestamp).Should().NotHaveValue();
				proxy.GetValue(LogFileProperties.EndTimestamp).Should().NotHaveValue();
				proxy.Count.Should().Be(0);
				proxy.Columns.Should().Equal(LogFileColumns.Minimum);

				proxy.GetEntry(0).Index.Should().Be(LogLineIndex.Invalid);
			}
		}

		[Test]
		public void TestConstruction()
		{
			_logFile.Setup(x => x.Columns).Returns(new[] { LogFileColumns.RawContent });
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.Columns.Should().Equal(LogFileColumns.RawContent);
			}
		}

		[Test]
		[Description("Verifies that the proxy registers a listener on the inner log file")]
		public void TestCtor2()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Verify(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()), Times.Once);
			}
		}

		[Test]
		[Description("Verifies that changing the inner log file causes the proxy to unregister the previously registered listener from the old file")]
		public void TestInnerLogFile1()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.InnerLogFile = null;
				_logFile.Verify(x => x.RemoveListener(It.IsAny<ILogFileListener>()), Times.Once);
			}
		}

		[Test]
		public void TestGetEntry()
		{
			var source = new InMemoryLogFile();
			source.AddEntry("I'm an english man in new york");
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, source))
			{
				var entry = proxy.GetEntry(0);
				entry.RawContent.Should().Be("I'm an english man in new york");
			}
		}

		[Test]
		public void TestGetLine_NoSource()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, null))
			{
				var entry = proxy.GetEntry(0);
				entry.Index.Should().Be(LogLineIndex.Invalid);
			}
		}

		[Test]
		public void TestGetSection()
		{
			var source = new InMemoryLogFile();
			source.AddEntry("Sting");
			source.AddEntry("I'm an english man in new york");
			source.AddEntry("1987");
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, source))
			{
				var entries = proxy.GetEntries(new LogFileSection(1, 2));
				entries[0].Index.Should().Be(1);
				entries[0].RawContent.Should().Be("I'm an english man in new york");
				entries[1].Index.Should().Be(2);
				entries[1].RawContent.Should().Be("1987");
			}
		}

		[Test]
		public void TestExists()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetAllValues(It.IsAny<ILogFileProperties>()))
				        .Callback((ILogFileProperties destination) =>
				        {
					        destination.SetValue(LogFileProperties.EmptyReason, ErrorFlags.None);
				        });
				_taskScheduler.RunOnce();
				proxy.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.None);

				_logFile.Setup(x => x.GetAllValues(It.IsAny<ILogFileProperties>()))
				        .Callback((ILogFileProperties destination) =>
				        {
					        destination.SetValue(LogFileProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
				        });
				_taskScheduler.RunOnce();
				proxy.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceCannotBeAccessed);
			}
		}

		[Test]
		public void TestFileSize()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetAllValues(It.IsAny<ILogFileProperties>()))
				        .Callback((ILogFileProperties destination) =>
				        {
					        destination.SetValue(LogFileProperties.Size, Size.FromBytes(12));
				        });
				_taskScheduler.RunOnce();
				proxy.GetValue(LogFileProperties.Size).Should().Be(Size.FromBytes(12));

				_logFile.Setup(x => x.GetAllValues(It.IsAny<ILogFileProperties>()))
				        .Callback((ILogFileProperties destination) =>
				        {
					        destination.SetValue(LogFileProperties.Size, Size.OneMegabyte);
				        });
				_taskScheduler.RunOnce();
				proxy.GetValue(LogFileProperties.Size).Should().Be(Size.OneMegabyte);
			}
		}

		[Test]
		public void TestCount()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
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
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetAllValues(It.IsAny<ILogFileProperties>()))
				        .Callback((ILogFileProperties destination) =>
				        {
					        destination.SetValue(LogFileProperties.StartTimestamp, new DateTime(2016, 10, 7, 14, 46, 00));
				        });
				_taskScheduler.RunOnce();
				proxy.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2016, 10, 7, 14, 46, 00));
				
				_logFile.Setup(x => x.GetAllValues(It.IsAny<ILogFileProperties>()))
				        .Callback((ILogFileProperties destination) =>
				        {
					        destination.SetValue(LogFileProperties.StartTimestamp, null);
				        });
				_taskScheduler.RunOnce();
				proxy.GetValue(LogFileProperties.StartTimestamp).Should().NotHaveValue();
			}
		}

		[Test]
		public void TestMaxCharactersPerLine()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetAllValues(It.IsAny<ILogFileProperties>()))
				        .Callback((ILogFileProperties destination) =>
				        {
					        destination.SetValue(TextLogFileProperties.MaxCharactersInLine, 101);
				        });
				_taskScheduler.RunOnce();
				proxy.GetValue(TextLogFileProperties.MaxCharactersInLine).Should().Be(101);
				
				_logFile.Setup(x => x.GetAllValues(It.IsAny<ILogFileProperties>()))
				        .Callback((ILogFileProperties destination) =>
				        {
					        destination.SetValue(TextLogFileProperties.MaxCharactersInLine, 0);
				        });
				_taskScheduler.RunOnce();
				proxy.GetValue(TextLogFileProperties.MaxCharactersInLine).Should().Be(0);
			}
		}

		[Test]
		public void TestDispose1()
		{
			var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero);
			_taskScheduler.PeriodicTaskCount.Should().Be(1);

			proxy.IsDisposed.Should().BeFalse();
			new Action(proxy.Dispose).Should().NotThrow();
			proxy.IsDisposed.Should().BeTrue();
			_taskScheduler.PeriodicTaskCount.Should().Be(0);
		}

		[Test]
		public void TestDispose2()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.Dispose();
				_logFile.Verify(l => l.Dispose(), Times.Once);
			}
		}

		[Test]
		public void TestListen1()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

				_listeners.OnRead(500);
				_listeners.OnRead(600);

				_taskScheduler.RunOnce();

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
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

				_listeners.OnRead(500);
				_listeners.Reset();
				_listeners.OnRead(600);

				_taskScheduler.RunOnce();

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
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

				_listeners.OnRead(500);
				_listeners.Invalidate(400, 100);
				_listeners.OnRead(550);

				_taskScheduler.RunOnce();

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
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

				new Action(() => proxy.OnLogFileModified(new Mock<ILogFile>().Object, new LogFileSection(0, 1))).Should().NotThrow();
				_modifications.Should().Equal(new[] { LogFileSection.Reset }, "because the OnLogFileModified shouldn't have been forwarded since it's from the wrong source");

				new Action(() => proxy.OnLogFileModified(null, new LogFileSection(0, 1))).Should().NotThrow();
				_modifications.Should().Equal(new[] { LogFileSection.Reset }, "because the OnLogFileModified shouldn't have been forwarded since it's from the wrong source");
			}
		}

		[Test]
		public void TestGetLogLineIndexOfOriginalLineIndex1()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
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
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero))
			{
				proxy.GetLogLineIndexOfOriginalLineIndex(new LogLineIndex(9001))
					.Should()
					.Be(LogLineIndex.Invalid, "because the proxy should just return an invalid index when no inner log file is present");
			}
		}

		[Test]
		public void TestGetColumn1()
		{
			var section = new LogFileSection(42, 100);
			var buffer = new string[142];
			var logFile = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object);
			var destinationIndex = 42;
			var queryOptions = new LogFileQueryOptions(LogFileQueryMode.FromCacheOnly);
			logFile.GetColumn(section, LogFileColumns.RawContent, buffer, destinationIndex, queryOptions);
			_logFile.Verify(x => x.GetColumn(It.Is<LogFileSection>(y => y == section),
			                                 LogFileColumns.RawContent,
			                                 buffer,
			                                 destinationIndex,
			                                 queryOptions),
			                Times.Once);
		}

		[Test]
		public void TestGetColumn2()
		{
			var section = new LogFileSection(42, 100);
			var buffer = new string[100];
			var logFile = new LogFileProxy(_taskScheduler, TimeSpan.Zero);
			logFile.GetColumn(section, LogFileColumns.RawContent, buffer);
			buffer.Should().OnlyContain(x => ReferenceEquals(x, null));
		}

		[Test]
		public void TestGetColumn3()
		{
			var indices = new LogLineIndex[] {1, 2};
			var buffer = new string[2];
			var logFile = new LogFileProxy(_taskScheduler, TimeSpan.Zero);
			logFile.GetColumn(indices, LogFileColumns.RawContent, buffer);
			buffer.Should().OnlyContain(x => ReferenceEquals(x, null));
		}

		[Test]
		public void TestProgress1()
		{
			var logFile = new LogFileProxy(_taskScheduler, TimeSpan.Zero);
			logFile.GetValue(LogFileProperties.PercentageProcessed).Should().Be(Percentage.Zero);
		}

		[Test]
		public void TestProgress2()
		{
			var logFile = new LogFileProxy(_taskScheduler, TimeSpan.Zero);

			_logFile.Setup(x => x.GetAllValues(It.IsAny<ILogFileProperties>()))
			        .Callback((ILogFileProperties destination) =>
			        {
				        destination.SetValue(LogFileProperties.PercentageProcessed, Percentage.FiftyPercent);
			        });

			logFile.InnerLogFile = _logFile.Object;
			logFile.GetValue(LogFileProperties.PercentageProcessed).Should()
			       .Be(Percentage.Zero, "because the proxy didn't process anything just yet");

			_taskScheduler.RunOnce();
			logFile.GetValue(LogFileProperties.PercentageProcessed).Should()
			       .Be(Percentage.FiftyPercent, "because while the proxy is done, its source is only half finished");
		}

		#region Well Known Columns

		[Test]
		public void TestGetOriginalIndexFrom2()
		{
			using (var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				var buffer = new LogLineIndex[100];
				var destinationIndex = 47;
				var queryOptions = new LogFileQueryOptions(LogFileQueryMode.FromCacheOnly);

				proxy.GetColumn(new LogFileSection(1, 42),
				                LogFileColumns.OriginalIndex,
				                buffer,
				                destinationIndex,
				                queryOptions);

				_logFile.Verify(x => x.GetColumn(It.Is<LogFileSection>(y => y == new LogFileSection(1, 42)),
				                                 LogFileColumns.OriginalIndex,
				                                 buffer,
				                                 destinationIndex,
				                                 queryOptions),
				                Times.Once, "because the proxy should simply forward those calls to its source");
			}
		}

		#endregion

		protected override ILogFile CreateEmpty()
		{
			return new LogFileProxy(_taskScheduler, TimeSpan.Zero);
		}

		protected override ILogFile CreateFromContent(IReadOnlyLogEntries content)
		{
			var source = new InMemoryLogFile(content);
			var proxy = new LogFileProxy(_taskScheduler, TimeSpan.Zero, source);
			return proxy;
		}
	}
}
