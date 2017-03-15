using System;
using System.Collections.Generic;
using System.Linq;
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
	public sealed class MultiLineLogFileTest
	{
		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();

			_lines = new List<LogLine>();
			_source = new Mock<ILogFile>();
			_source.Setup(x => x.Count).Returns(_lines.Count);
			_source.Setup(x => x.GetLine(It.IsAny<int>())).Returns((int index) => _lines[index]);
			_source.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
				.Callback(
					(LogFileSection section, LogLine[] entries) =>
						_lines.CopyTo((int) section.Index, entries, 0, section.Count));
			_source.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
				.Callback((ILogFileListener listener, TimeSpan unused1, int unused2) =>
				{
					listener.OnLogFileModified(_source.Object,
						LogFileSection.Reset);
				});

			_sections = new List<LogFileSection>();
			_listener = new Mock<ILogFileListener>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
					 .Callback((ILogFile l, LogFileSection s) => _sections.Add(s));

		}

		private Mock<ILogFile> _source;
		private List<LogLine> _lines;
		private ManualTaskScheduler _taskScheduler;
		private List<LogFileSection> _sections;
		private Mock<ILogFileListener> _listener;

		[Test]
		public void TestCtor1()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_taskScheduler.PeriodicTaskCount.Should().Be(1);

			logFile.Dispose();
			_taskScheduler.PeriodicTaskCount.Should().Be(0);
		}

		[Test]
		public void TestCtor2()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_source.Verify(
				x => x.AddListener(It.Is<ILogFileListener>(y => Equals(y, logFile)), It.IsAny<TimeSpan>(), It.IsAny<int>()),
				Times.Once,
				"because the single line log file should register itself as a source at the listener");

			new Action(() => logFile.Dispose()).ShouldNotThrow("because Dispose() must always succeed");
			_source.Verify(x => x.RemoveListener(It.Is<ILogFileListener>(y => Equals(y, logFile))), Times.Once,
				"because the single line log file should remove itself as a listener from its source upon being disposed of");
		}

		[Test]
		[Description("Verifies that the file requires the scheduler to run once before it says that it's completely consumed the source")]
		public void TestCtor3()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_source.Setup(x => x.EndOfSourceReached).Returns(true);
			logFile.EndOfSourceReached.Should().BeFalse("because the log file shouldn't even have inspected the source yet");

			_taskScheduler.RunOnce();

			logFile.EndOfSourceReached.Should().BeTrue();
		}

		[Test]
		[Description("Verifies that the EndOfSourceReached flag is reset as soon as a modification is forwarded")]
		public void TestEndOfSourceReached1()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_source.Setup(x => x.EndOfSourceReached).Returns(true);
			_taskScheduler.RunOnce();
			logFile.EndOfSourceReached.Should().BeTrue();

			logFile.OnLogFileModified(_source.Object, LogFileSection.Reset);
			logFile.EndOfSourceReached.Should().BeFalse();
		}

		[Test]
		[Description("Verifies that the EndOfSourceReached flag is only set to true when the source is too")]
		public void TestEndOfSourceReached2()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_source.Setup(x => x.EndOfSourceReached).Returns(false);
			_taskScheduler.RunOnce();
			logFile.EndOfSourceReached.Should().BeFalse("because the source isn't finished yet");

			logFile.OnLogFileModified(_source.Object, LogFileSection.Reset);
			logFile.EndOfSourceReached.Should().BeFalse("because the source isn't finished yet");

			logFile.OnLogFileModified(_source.Object, LogFileSection.Reset);
			_source.Setup(x => x.EndOfSourceReached).Returns(true);
			_taskScheduler.RunOnce();
			logFile.EndOfSourceReached.Should().BeTrue("because the log file has processed all events AND the source is finished");
		}

		[Test]
		[Description("Verifies that MaxCharactersPerLine is changed once a modification is applied")]
		public void TestOneModification2()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			logFile.MaxCharactersPerLine.Should().Be(0);

			_lines.Add(new LogLine());
			_source.Setup(x => x.MaxCharactersPerLine).Returns(42);
			logFile.MaxCharactersPerLine.Should().Be(0, "because the change shouldn't have been applied yet");

			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 1));
			logFile.MaxCharactersPerLine.Should().Be(0, "because the change shouldn't have been applied yet");

			_taskScheduler.RunOnce();
			logFile.MaxCharactersPerLine.Should().Be(42, "because the change should have been applied by now");
		}

		[Test]
		[Description("Verifies that the Exists flag is changed once a modification is applied")]
		public void TestOneModification3()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			logFile.Exists.Should().BeFalse("because the source doesn't exist (yet)");

			_lines.Add(new LogLine());
			_source.Setup(x => x.Exists).Returns(true);
			logFile.Exists.Should().BeFalse("because the change shouldn't have been applied yet");

			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 1));
			logFile.Exists.Should().BeFalse("because the change shouldn't have been applied yet");

			_taskScheduler.RunOnce();
			logFile.Exists.Should().BeTrue("because the change should have been applied by now");
		}

		[Test]
		[Description("Verifies that the StartTimestamp is changed once a modification is applied")]
		public void TestOneModification4()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			logFile.StartTimestamp.Should().NotHaveValue("because the source doesn't exist (yet)");

			var timestamp = new DateTime(2017, 3, 15, 22, 40, 0);
			_lines.Add(new LogLine());
			_source.Setup(x => x.StartTimestamp).Returns(timestamp);
			logFile.StartTimestamp.Should().NotHaveValue("because the change shouldn't have been applied yet");

			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 1));
			logFile.StartTimestamp.Should().NotHaveValue("because the change shouldn't have been applied yet");

			_taskScheduler.RunOnce();
			logFile.StartTimestamp.Should().Be(timestamp, "because the change should have been applied by now");
		}

		[Test]
		[Description("Verifies that the LastModified is changed once a modification is applied")]
		public void TestOneModification5()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			logFile.LastModified.Should().Be(DateTime.MinValue, "because the source doesn't exist (yet)");

			var timestamp = new DateTime(2017, 3, 15, 22, 40, 0);
			_lines.Add(new LogLine());
			_source.Setup(x => x.LastModified).Returns(timestamp);
			logFile.LastModified.Should().Be(DateTime.MinValue, "because the change shouldn't have been applied yet");

			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 1));
			logFile.LastModified.Should().Be(DateTime.MinValue, "because the change shouldn't have been applied yet");

			_taskScheduler.RunOnce();
			logFile.LastModified.Should().Be(timestamp, "because the change should have been applied by now");
		}

		[Test]
		[Description("Verifies that the FileSize is changed once a modification is applied")]
		public void TestOneModification6()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_taskScheduler.RunOnce();
			logFile.FileSize.Should().Be(Size.Zero, "because the source doesn't exist (yet)");

			var size = Size.FromGigabytes(42);
			_lines.Add(new LogLine());
			_source.Setup(x => x.FileSize).Returns(size);
			logFile.FileSize.Should().Be(Size.Zero, "because the change shouldn't have been applied yet");

			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 1));
			logFile.FileSize.Should().Be(Size.Zero, "because the change shouldn't have been applied yet");

			_taskScheduler.RunOnce();
			logFile.FileSize.Should().Be(size, "because the change should have been applied by now");
		}

		[Test]
		[Description("Verifies that receiving a Reset() actually causes the entire content to be reset")]
		public void TestReset1()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_lines.Add(new LogLine(0, 0, "INFO: hello", LevelFlags.Info));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 1));
			_taskScheduler.RunOnce();

			_lines.Clear();
			logFile.OnLogFileModified(_source.Object, LogFileSection.Reset);
			_taskScheduler.RunOnce();

			logFile.Count.Should().Be(0, "because the source is completely empty");
		}

		[Test]
		[Description("Verifies that the log file can represent completely new content after reset")]
		public void TestReset2()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_lines.Add(new LogLine(0, 0, "A", LevelFlags.Info));
			_lines.Add(new LogLine(1, 1, "B", LevelFlags.Warning));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 2));
			_taskScheduler.RunOnce();
			logFile.Count.Should().Be(2);

			_lines.Clear();
			logFile.OnLogFileModified(_source.Object, LogFileSection.Reset);
			_taskScheduler.RunOnce();
			logFile.Count.Should().Be(0);

			_lines.Add(new LogLine(0, 0, "A", LevelFlags.Info));
			_lines.Add(new LogLine(1, 1, "A continued", LevelFlags.None));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 2));
			_taskScheduler.RunOnce();

			logFile.GetSection(new LogFileSection(0, 2)).Should().Equal(new object[]
			{
				new LogLine(0, 0, "A", LevelFlags.Info),
				new LogLine(1, 0, "A continued", LevelFlags.Info)
			}, "because the log file should now represent the new content where both lines belong to the same entry");
		}

		[Test]
		public void TestOneEntry1()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);

			var timestamp = new DateTime(2017, 3, 15, 21, 52, 0);
			_lines.Add(new LogLine(0, 0, "INFO: hello", LevelFlags.Info, timestamp));
			_lines.Add(new LogLine(1, 1, "world!", LevelFlags.None, timestamp));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 2));

			_taskScheduler.RunOnce();

			logFile.Count.Should().Be(2);
			logFile.GetLine(0).Should().Be(new LogLine(0, 0, "INFO: hello", LevelFlags.Info, timestamp));
			logFile.GetLine(1).Should().Be(new LogLine(1, 0, "world!", LevelFlags.Info, timestamp));
		}

		[Test]
		[Description("Verifies that the log file correctly assembles a log event that arrives as two separate lines")]
		public void TestOneEntry2()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);

			var timestamp = new DateTime(2017, 3, 15, 21, 52, 0);
			_lines.Add(new LogLine(0, 0, "hello", LevelFlags.Info, timestamp));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 1));
			_taskScheduler.RunOnce();
			logFile.Count.Should().Be(1);

			_lines.Add(new LogLine(1, 1, "world!", LevelFlags.None));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(1, 1));
			_taskScheduler.RunOnce();
			logFile.Count.Should().Be(2);
			logFile.GetLine(0).Should().Be(new LogLine(0, 0, "hello", LevelFlags.Info, timestamp));
			logFile.GetLine(1).Should().Be(new LogLine(1, 0, "world!", LevelFlags.Info, timestamp));
		}

		[Test]
		[Description("Verifies that the log file correctly fires invalidation events to its listeners when a log entry arrives in multiple parts")]
		public void TestOneEntry3()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			logFile.AddListener(_listener.Object, TimeSpan.Zero, 10);

			_lines.Add(new LogLine(0, 0, "INFO: hello", LevelFlags.Info));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 1));
			_taskScheduler.RunOnce();
			_sections.Should().Equal(new object[] {LogFileSection.Reset, new LogFileSection(0, 1)});

			_sections.Clear();
			_lines.Add(new LogLine(1, 1, "world!", LevelFlags.None));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(1, 1));
			_taskScheduler.RunOnce();
			_sections.Should().Equal(new object[]
			{
				new LogFileSection(0, 1, invalidateSection: true),
				new LogFileSection(0, 2)
			});
		}

		[Test]
		[Description("Verifies that the log file correctly fires invalidation events to its listeners when a log entry arrives in multiple parts")]
		public void TestTwoEntries1()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);

			_lines.Add(new LogLine(0, 0, "DEBUG: Starting...", LevelFlags.Debug));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 1));
			_taskScheduler.RunOnce();

			logFile.AddListener(_listener.Object, TimeSpan.Zero, 10);
			_lines.Add(new LogLine(1, 1, "INFO: hello", LevelFlags.Info));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(1, 1));
			_taskScheduler.RunOnce();
			_sections.Should().Equal(new object[]
			{
				LogFileSection.Reset,
				new LogFileSection(0, 1),
				new LogFileSection(1, 1)
			});

			_sections.Clear();
			_lines.Add(new LogLine(2, 2, "world!", LevelFlags.None));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(2, 1));
			_taskScheduler.RunOnce();
			_sections.Should().Equal(new object[]
			{
				new LogFileSection(1, 1, invalidateSection: true),
				new LogFileSection(1, 2)
			});
		}

		[Test]
		[Description("Verifies that the log file correctly interprets many single line log entries")]
		public void TestManyEntries1()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_lines.Add(new LogLine(0, 0, "A", LevelFlags.Debug));
			_lines.Add(new LogLine(1, 1, "B", LevelFlags.Info));
			_lines.Add(new LogLine(2, 2, "C", LevelFlags.Warning));
			_lines.Add(new LogLine(3, 3, "D", LevelFlags.Error));
			_lines.Add(new LogLine(4, 4, "E", LevelFlags.Fatal));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 5));
			_taskScheduler.RunOnce();

			logFile.Count.Should().Be(5);
			logFile.GetLine(0).Should().Be(new LogLine(0, 0, "A", LevelFlags.Debug));
			logFile.GetLine(1).Should().Be(new LogLine(1, 1, "B", LevelFlags.Info));
			logFile.GetLine(2).Should().Be(new LogLine(2, 2, "C", LevelFlags.Warning));
			logFile.GetLine(3).Should().Be(new LogLine(3, 3, "D", LevelFlags.Error));
			logFile.GetLine(4).Should().Be(new LogLine(4, 4, "E", LevelFlags.Fatal));
		}

		[Test]
		[Description("Verifies that the log file correctly interprets many single line log entries")]
		public void TestManyEntries2()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);

			_lines.Add(new LogLine(0, 0, "A", LevelFlags.Debug));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 1));
			_taskScheduler.RunOnce();

			_lines.Add(new LogLine(1, 1, "B", LevelFlags.Info));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(1, 1));
			_taskScheduler.RunOnce();

			_lines.Add(new LogLine(2, 2, "C", LevelFlags.Warning));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(2, 1));
			_taskScheduler.RunOnce();

			_lines.Add(new LogLine(3, 3, "D", LevelFlags.Error));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(3, 1));
			_taskScheduler.RunOnce();

			_lines.Add(new LogLine(4, 4, "E", LevelFlags.Fatal));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(4, 1));
			_taskScheduler.RunOnce();

			logFile.Count.Should().Be(5);
			logFile.GetLine(0).Should().Be(new LogLine(0, 0, "A", LevelFlags.Debug));
			logFile.GetLine(1).Should().Be(new LogLine(1, 1, "B", LevelFlags.Info));
			logFile.GetLine(2).Should().Be(new LogLine(2, 2, "C", LevelFlags.Warning));
			logFile.GetLine(3).Should().Be(new LogLine(3, 3, "D", LevelFlags.Error));
			logFile.GetLine(4).Should().Be(new LogLine(4, 4, "E", LevelFlags.Fatal));
		}

		[Test]
		[Description("Verifies that the log file correctly interprets many single line log entries")]
		public void TestManyEntries3()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_lines.AddRange(Enumerable.Range(0, 10001).Select(i => new LogLine(i, i, "", LevelFlags.Info)));
			_source.Setup(x => x.EndOfSourceReached).Returns(true);
			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 10001));
			_taskScheduler.RunOnce();

			logFile.Count.Should().Be(10000, "because the log file should process a fixed amount of lines per tick");
			logFile.EndOfSourceReached.Should().BeFalse("because the log file hasn't processed the entire source yet");
			logFile.GetSection(new LogFileSection(0, 10000))
				.Should().Equal(_lines.GetRange(0, 10000));

			_taskScheduler.RunOnce();
			logFile.Count.Should()
				.Be(10001, "because the log file should now have enough ticks elapsed to have processed the entire source");
			logFile.EndOfSourceReached.Should().BeTrue("because the log file should've processed the entire source by now");
			logFile.GetSection(new LogFileSection(0, 10001))
				.Should().Equal(_lines);
		}

		[Test]
		[Description("Verifies that GetSection can return many entries")]
		public void TestGetSection()
		{
			var logFile = new MultiLineLogFile(_taskScheduler, _source.Object, TimeSpan.Zero);
			_lines.Add(new LogLine(0, 0, "A", LevelFlags.Debug));
			_lines.Add(new LogLine(1, 1, "B", LevelFlags.Info));
			_lines.Add(new LogLine(2, 2, "C", LevelFlags.Warning));
			_lines.Add(new LogLine(3, 3, "D", LevelFlags.Error));
			_lines.Add(new LogLine(4, 4, "E", LevelFlags.Fatal));
			logFile.OnLogFileModified(_source.Object, new LogFileSection(0, 5));
			_taskScheduler.RunOnce();

			logFile.GetSection(new LogFileSection(0, 5)).Should().Equal(new object[]
			{
				new LogLine(0, 0, "A", LevelFlags.Debug),
				new LogLine(1, 1, "B", LevelFlags.Info),
				new LogLine(2, 2, "C", LevelFlags.Warning),
				new LogLine(3, 3, "D", LevelFlags.Error),
				new LogLine(4, 4, "E", LevelFlags.Fatal)
			});
		}
	}
}