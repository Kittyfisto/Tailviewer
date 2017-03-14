using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class SingleLineLogFileTest
	{
		private Mock<ILogFile> _logFile;
		private ManualTaskScheduler _taskScheduler;
		private List<LogLine> _entries;
		private List<LogFileSection> _sections;
		private Mock<ILogFileListener> _listener;

		[SetUp]
		public void Setup()
		{
			_entries = new List<LogLine>();
			_logFile = new Mock<ILogFile>();
			_logFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
					.Callback(
						(LogFileSection section, LogLine[] entries) =>
						_entries.CopyTo((int)section.Index, entries, 0, section.Count));
			_logFile.Setup(x => x.GetLine(It.IsAny<int>())).Returns((int index) => _entries[index]);
			_logFile.Setup(x => x.Exists).Returns(true);
			_logFile.Setup(x => x.Count).Returns(() => _entries.Count);
			_logFile.Setup(x => x.EndOfSourceReached).Returns(true);

			_sections = new List<LogFileSection>();
			_listener = new Mock<ILogFileListener>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
					 .Callback((ILogFile l, LogFileSection s) => _sections.Add(s));

			_taskScheduler = new ManualTaskScheduler();
		}

		[Test]
		[Description("Verifies that the log file starts a new periodic task with its dispatcher upon being constructed")]
		public void TestCtor1()
		{
			var single = new SingleLineLogFile(_taskScheduler, _logFile.Object, TimeSpan.Zero);
			_taskScheduler.PeriodicTaskCount.Should()
				.Be(1, "because the log file should've started a new periodic task to perform its work");
		}

		[Test]
		public void TestEmpty()
		{
			var single = new SingleLineLogFile(_taskScheduler, _logFile.Object, TimeSpan.Zero);
			single.OnLogFileModified(_logFile.Object, LogFileSection.Reset);

			_taskScheduler.RunOnce();

			single.EndOfSourceReached.Should().BeTrue();
			single.Count.Should().Be(0, "because the log file represents an empty source");
			single.Exists.Should().BeTrue("because the source exists");
			single.FileSize.Should().Be(Size.Zero, "because the source is empty");
			single.MaxCharactersPerLine.Should().Be(0, "because the source is empty");

			new Action(() => single.GetLine(0)).ShouldThrow<ArgumentOutOfRangeException>("because the source is empty");
			new Action(() => single.GetSection(new LogFileSection(0, 1))).ShouldThrow<ArgumentException>("because the source is empty");
		}

		[Test]
		public void TestOneLine1()
		{
			var single = new SingleLineLogFile(_taskScheduler, _logFile.Object, TimeSpan.Zero);

			var line = new LogLine(0, 0, "INFO: Hello World!", LevelFlags.Info);
			_entries.Add(line);
			single.OnLogFileModified(_logFile.Object, new LogFileSection(0, 1));

			_taskScheduler.RunOnce();

			single.Count.Should().Be(1, "because the log file should represent the one line from its source");
			var actualLine = single.GetLine(0);
			actualLine.Should().Be(line, "because the log file should not modify the one line it represents from the source");
		}

		[Test]
		public void TestOneLine2()
		{
			var single = new SingleLineLogFile(_taskScheduler, _logFile.Object, TimeSpan.Zero);
			single.EndOfSourceReached.Should().BeFalse("because the file hasn't even been updated");
			single.AddListener(_listener.Object, TimeSpan.Zero, 10);

			var line = new LogLine(0, 0, "INFO: Hello World!", LevelFlags.Info);
			_entries.Add(line);
			single.OnLogFileModified(_logFile.Object, new LogFileSection(0, 1));

			_taskScheduler.RunOnce();

			single.EndOfSourceReached.Should().BeTrue("because the file is now up-to-date with respect to its source");

			_sections.Should().Equal(new object[] {LogFileSection.Reset, new LogFileSection(0, 1)},
				"because the log file should've notified its listener about this change");
		}

		[Test]
		[Description("Verifies that the EndOfSourceReached flag is set and reset at the appropriate times")]
		public void TestManyUpdates1()
		{
			var single = new SingleLineLogFile(_taskScheduler, _logFile.Object, TimeSpan.Zero);

			single.OnLogFileModified(_logFile.Object, new LogFileSection(0, 1));
			single.EndOfSourceReached.Should().BeFalse();

			_taskScheduler.RunOnce();
			single.EndOfSourceReached.Should().BeTrue("because the log file fully represents the source");

			single.OnLogFileModified(_logFile.Object, new LogFileSection(1, 1));
			single.EndOfSourceReached.Should().BeFalse("because the source has been modified again and the log file doesn't represent the source just yet");

			_taskScheduler.RunOnce();
			single.EndOfSourceReached.Should().BeTrue("because the log file represents its source once again");
		}

		[Test]
		public void TestOneEntry1()
		{
			var single = new SingleLineLogFile(_taskScheduler, _logFile.Object, TimeSpan.Zero);

			var line1 = new LogLine(0, 0, "INFO: Hello", LevelFlags.Info);
			var line2 = new LogLine(1, 0, "INFO: World!", LevelFlags.Info);
			_entries.Add(line1);
			_entries.Add(line2);
			single.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

			_taskScheduler.RunOnce();

			single.Count.Should().Be(2, "because the source contains two log lines");
			var actualLine1 = single.GetLine(0);
			actualLine1.Should().Be(line1);

			var actualLine2 = single.GetLine(1);
			actualLine2.LineIndex.Should().Be(line2.LineIndex);
			actualLine2.LogEntryIndex.Should().Be(1);
			actualLine2.Message.Should().Be(line2.Message);
			actualLine2.Level.Should().Be(line2.Level);
			actualLine2.Timestamp.Should().Be(line2.Timestamp);

			var section = single.GetSection(new LogFileSection(0, 2));
			section[0].Should().Be(actualLine1);
			section[1].Should().Be(actualLine2);
		}

		[Test]
		public void TestOneEntry2()
		{
			var single = new SingleLineLogFile(_taskScheduler, _logFile.Object, TimeSpan.Zero);

			var line1 = new LogLine(0, 0, "DEBUG: Hello", LevelFlags.Warning);
			var line2 = new LogLine(1, 0, "WARN: World!", LevelFlags.Warning);
			_entries.Add(line1);
			_entries.Add(line2);
			single.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

			_taskScheduler.RunOnce();

			single.Count.Should().Be(2, "because the source contains two log lines");
			var actualLine1 = single.GetLine(0);
			actualLine1.Level.Should().Be(LevelFlags.Debug, "because the log file should re-evaluate the level of a line");

			var actualLine2 = single.GetLine(1);
			actualLine2.Level.Should().Be(LevelFlags.Warning);

			var section = single.GetSection(new LogFileSection(0, 2));
			section[0].Should().Be(actualLine1);
			section[1].Should().Be(actualLine2);
		}

		[Test]
		public void TestDispose1()
		{
			var single = new SingleLineLogFile(_taskScheduler, _logFile.Object, TimeSpan.Zero);
			_taskScheduler.PeriodicTaskCount.Should().Be(1);
			single.Dispose();

			_taskScheduler.PeriodicTaskCount.Should().Be(0, "because Dispose() should've removed its periodic task from the scheduler");
		}

		[Test]
		public void TestDispose2()
		{
			var single = new SingleLineLogFile(_taskScheduler, _logFile.Object, TimeSpan.Zero);
			_logFile.Verify(
				x => x.AddListener(It.Is<ILogFileListener>(y => Equals(y, single)), It.IsAny<TimeSpan>(), It.IsAny<int>()),
				Times.Once,
				"because the single line log file should register itself as a source at the listener");

			new Action(() => single.Dispose()).ShouldNotThrow("because Dispose() must always succeed");
			_logFile.Verify(x => x.RemoveListener(It.Is<ILogFileListener>(y => Equals(y, single))), Times.Once,
				"because the single line log file should remove itself as a listener from its source upon being disposed of");
		}
	}
}