﻿using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Proxy
{
	[TestFixture]
	public sealed class LogFileProxyTest
		: AbstractLogFileTest
	{
		private Mock<ILogSource> _logFile;
		private LogSourceListenerCollection _listeners;
		private Mock<ILogSourceListener> _listener;
		private List<LogFileSection> _modifications;
		private ManualTaskScheduler _taskScheduler;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();

			_logFile = new Mock<ILogSource>();
			_listeners = new LogSourceListenerCollection(_logFile.Object);
			_logFile.Setup(x => x.AddListener(It.IsAny<ILogSourceListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			        .Callback((ILogSourceListener listener, TimeSpan maximumWaitTime, int maximumLineCount) => _listeners.AddListener(listener, maximumWaitTime, maximumLineCount));
			_logFile.Setup(x => x.RemoveListener(It.IsAny<ILogSourceListener>()))
			        .Callback((ILogSourceListener listener) => _listeners.RemoveListener(listener));

			_listener = new Mock<ILogSourceListener>();
			_modifications = new List<LogFileSection>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogFileSection>()))
			         .Callback((ILogSource logFile, LogFileSection section) => _modifications.Add(section));
		}

		[Test]
		public void TestEmptyConstruction()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero))
			{
				proxy.InnerLogSource.Should().BeNull();
				proxy.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(0);
				proxy.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);
				proxy.GetProperty(GeneralProperties.Size).Should().BeNull();
				proxy.GetProperty(GeneralProperties.StartTimestamp).Should().NotHaveValue();
				proxy.GetProperty(GeneralProperties.EndTimestamp).Should().NotHaveValue();
				proxy.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0);
				proxy.Columns.Should().Equal(LogColumns.Minimum);

				proxy.GetEntry(0).Index.Should().Be(LogLineIndex.Invalid);
			}
		}

		[Test]
		public void TestConstruction()
		{
			_logFile.Setup(x => x.Columns).Returns(new[] { LogColumns.RawContent });
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.Columns.Should().Equal(LogColumns.RawContent);
			}
		}

		[Test]
		[Description("Verifies that the proxy registers a listener on the inner log file")]
		public void TestCtor2()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Verify(x => x.AddListener(It.IsAny<ILogSourceListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()), Times.Once);
			}
		}

		[Test]
		[Description("Verifies that changing the inner log file causes the proxy to unregister the previously registered listener from the old file")]
		public void TestInnerLogFile1()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.InnerLogSource = null;
				_logFile.Verify(x => x.RemoveListener(It.IsAny<ILogSourceListener>()), Times.Once);
			}
		}

		[Test]
		public void TestGetEntry()
		{
			var source = new InMemoryLogSource();
			source.AddEntry("I'm an english man in new york");
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, source))
			{
				var entry = proxy.GetEntry(0);
				entry.RawContent.Should().Be("I'm an english man in new york");
			}
		}

		[Test]
		public void TestGetLine_NoSource()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, null))
			{
				var entry = proxy.GetEntry(0);
				entry.Index.Should().Be(LogLineIndex.Invalid);
			}
		}

		[Test]
		public void TestGetSection()
		{
			var source = new InMemoryLogSource();
			source.AddEntry("Sting");
			source.AddEntry("I'm an english man in new york");
			source.AddEntry("1987");
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, source))
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
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
				        .Callback((IPropertiesBuffer destination) =>
				        {
					        destination.SetValue(GeneralProperties.EmptyReason, ErrorFlags.None);
				        });
				_taskScheduler.RunOnce();
				proxy.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.None);

				_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
				        .Callback((IPropertiesBuffer destination) =>
				        {
					        destination.SetValue(GeneralProperties.EmptyReason, ErrorFlags.SourceCannotBeAccessed);
				        });
				_taskScheduler.RunOnce();
				proxy.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.SourceCannotBeAccessed);
			}
		}

		[Test]
		public void TestFileSize()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
				        .Callback((IPropertiesBuffer destination) =>
				        {
					        destination.SetValue(GeneralProperties.Size, Size.FromBytes(12));
				        });
				_taskScheduler.RunOnce();
				proxy.GetProperty(GeneralProperties.Size).Should().Be(Size.FromBytes(12));

				_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
				        .Callback((IPropertiesBuffer destination) =>
				        {
					        destination.SetValue(GeneralProperties.Size, Size.OneMegabyte);
				        });
				_taskScheduler.RunOnce();
				proxy.GetProperty(GeneralProperties.Size).Should().Be(Size.OneMegabyte);
			}
		}

		[Test]
		public void TestCount()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
				        .Callback((IPropertiesBuffer destination) =>
					                  destination.SetValue(GeneralProperties.LogEntryCount, 42));
				_taskScheduler.RunOnce();
				proxy.GetProperty(GeneralProperties.LogEntryCount).Should().Be(42);

				_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
				        .Callback((IPropertiesBuffer destination) =>
					                  destination.SetValue(GeneralProperties.LogEntryCount, 9001));
				_taskScheduler.RunOnce();
				proxy.GetProperty(GeneralProperties.LogEntryCount).Should().Be(9001);
			}
		}

		[Test]
		public void TestStartTimestamp()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
				        .Callback((IPropertiesBuffer destination) =>
				        {
					        destination.SetValue(GeneralProperties.StartTimestamp, new DateTime(2016, 10, 7, 14, 46, 00));
				        });
				_taskScheduler.RunOnce();
				proxy.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2016, 10, 7, 14, 46, 00));
				
				_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
				        .Callback((IPropertiesBuffer destination) =>
				        {
					        destination.SetValue(GeneralProperties.StartTimestamp, null);
				        });
				_taskScheduler.RunOnce();
				proxy.GetProperty(GeneralProperties.StartTimestamp).Should().NotHaveValue();
			}
		}

		[Test]
		public void TestMaxCharactersPerLine()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
				        .Callback((IPropertiesBuffer destination) =>
				        {
					        destination.SetValue(TextProperties.MaxCharactersInLine, 101);
				        });
				_taskScheduler.RunOnce();
				proxy.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(101);
				
				_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
				        .Callback((IPropertiesBuffer destination) =>
				        {
					        destination.SetValue(TextProperties.MaxCharactersInLine, 0);
				        });
				_taskScheduler.RunOnce();
				proxy.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(0);
			}
		}

		[Test]
		public void TestDispose1()
		{
			var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero);
			_taskScheduler.PeriodicTaskCount.Should().Be(1);

			proxy.IsDisposed.Should().BeFalse();
			new Action(proxy.Dispose).Should().NotThrow();
			proxy.IsDisposed.Should().BeTrue();
			_taskScheduler.PeriodicTaskCount.Should().Be(0);
		}

		[Test]
		public void TestDispose2()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.Dispose();
				_logFile.Verify(l => l.Dispose(), Times.Once);
			}
		}

		[Test]
		public void TestListen1()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object, 1000))
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
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
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
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
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
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				proxy.AddListener(_listener.Object, TimeSpan.Zero, 1000);

				new Action(() => proxy.OnLogFileModified(new Mock<ILogSource>().Object, new LogFileSection(0, 1))).Should().NotThrow();
				_modifications.Should().Equal(new[] { LogFileSection.Reset }, "because the OnLogFileModified shouldn't have been forwarded since it's from the wrong source");

				new Action(() => proxy.OnLogFileModified(null, new LogFileSection(0, 1))).Should().NotThrow();
				_modifications.Should().Equal(new[] { LogFileSection.Reset }, "because the OnLogFileModified shouldn't have been forwarded since it's from the wrong source");
			}
		}

		[Test]
		public void TestGetLogLineIndexOfOriginalLineIndex1()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
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
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero))
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
			var logFile = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object);
			var destinationIndex = 42;
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCacheOnly);
			logFile.GetColumn(section, LogColumns.RawContent, buffer, destinationIndex, queryOptions);
			_logFile.Verify(x => x.GetColumn(It.Is<LogFileSection>(y => y == section),
			                                 LogColumns.RawContent,
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
			var logFile = new LogSourceProxy(_taskScheduler, TimeSpan.Zero);
			logFile.GetColumn(section, LogColumns.RawContent, buffer);
			buffer.Should().OnlyContain(x => ReferenceEquals(x, null));
		}

		[Test]
		public void TestGetColumn3()
		{
			var indices = new LogLineIndex[] {1, 2};
			var buffer = new string[2];
			var logFile = new LogSourceProxy(_taskScheduler, TimeSpan.Zero);
			logFile.GetColumn(indices, LogColumns.RawContent, buffer);
			buffer.Should().OnlyContain(x => ReferenceEquals(x, null));
		}

		[Test]
		public void TestProgress1()
		{
			var logFile = new LogSourceProxy(_taskScheduler, TimeSpan.Zero);
			logFile.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.Zero);
		}

		[Test]
		public void TestProgress2()
		{
			var logFile = new LogSourceProxy(_taskScheduler, TimeSpan.Zero);

			_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
			        .Callback((IPropertiesBuffer destination) =>
			        {
				        destination.SetValue(GeneralProperties.PercentageProcessed, Percentage.FiftyPercent);
			        });

			logFile.InnerLogSource = _logFile.Object;
			logFile.GetProperty(GeneralProperties.PercentageProcessed).Should()
			       .Be(Percentage.Zero, "because the proxy didn't process anything just yet");

			_taskScheduler.RunOnce();
			logFile.GetProperty(GeneralProperties.PercentageProcessed).Should()
			       .Be(Percentage.FiftyPercent, "because while the proxy is done, its source is only half finished");
		}

		#region Well Known Columns

		[Test]
		public void TestGetOriginalIndexFrom2()
		{
			using (var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, _logFile.Object))
			{
				var buffer = new LogLineIndex[100];
				var destinationIndex = 47;
				var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCacheOnly);

				proxy.GetColumn(new LogFileSection(1, 42),
				                LogColumns.OriginalIndex,
				                buffer,
				                destinationIndex,
				                queryOptions);

				_logFile.Verify(x => x.GetColumn(It.Is<LogFileSection>(y => y == new LogFileSection(1, 42)),
				                                 LogColumns.OriginalIndex,
				                                 buffer,
				                                 destinationIndex,
				                                 queryOptions),
				                Times.Once, "because the proxy should simply forward those calls to its source");
			}
		}

		#endregion

		protected override ILogSource CreateEmpty()
		{
			return new LogSourceProxy(_taskScheduler, TimeSpan.Zero);
		}

		protected override ILogSource CreateFromContent(IReadOnlyLogBuffer content)
		{
			var source = new InMemoryLogSource(content);
			var proxy = new LogSourceProxy(_taskScheduler, TimeSpan.Zero, source);
			_taskScheduler.RunOnce();
			return proxy;
		}
	}
}
