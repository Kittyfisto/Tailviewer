using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFileManualTest
	{
		private ManualTaskScheduler _scheduler;
		private string _fname;
		private FileStream _stream;
		private StreamWriter _writer;
		private LogFile _file;
		private Mock<ILogFileListener> _listener;
		private List<LogFileSection> _changes;

		[SetUp]
		public void Setup()
		{
			_scheduler = new ManualTaskScheduler();
			_fname = Path.GetTempFileName();
			if (File.Exists(_fname))
				File.Delete(_fname);

			_stream = File.Open(_fname, FileMode.Create, FileAccess.Write, FileShare.Read);
			_writer = new StreamWriter(_stream);

			_file = new LogFile(_scheduler, _fname);

			_listener = new Mock<ILogFileListener>();
			_changes = new List<LogFileSection>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
				.Callback((ILogFile unused, LogFileSection section) => _changes.Add(section));

			_file.AddListener(_listener.Object, TimeSpan.Zero, 10);
		}

		[TearDown]
		public void TearDown()
		{
			_writer?.Dispose();
			_stream?.Dispose();
		}

		[Test]
		public void TestReadOneLine1()
		{
			_writer.Write("Foo");
			_writer.Flush();

			_scheduler.RunOnce();
			_file.Count.Should().Be(1);
			_file.GetLine(0).Should().Be(new LogLine(0, 0, "Foo", LevelFlags.None));
		}

		[Test]
		[Description("Verifies that a line written in two separate flushes is correctly assembly to a single log line")]
		public void TestReadOneLine2()
		{
			_writer.Write("Hello ");
			_writer.Flush();
			_scheduler.RunOnce();

			_writer.Write("World!");
			_writer.Flush();
			_scheduler.RunOnce();

			_file.Count.Should().Be(1);
			_file.GetLine(0).Should().Be(new LogLine(0, 0, "Hello World!", LevelFlags.None));
		}

		[Test]
		[Description("Verifies that a line written in three separate flushes is correctly assembly to a single log line")]
		public void TestReadOneLine3()
		{
			_writer.Write("A");
			_writer.Flush();
			_scheduler.RunOnce();

			_writer.Write("B");
			_writer.Flush();
			_scheduler.RunOnce();

			_writer.Write("C");
			_writer.Flush();
			_scheduler.RunOnce();

			_file.Count.Should().Be(1);
			_file.GetLine(0).Should().Be(new LogLine(0, 0, "ABC", LevelFlags.None));
		}

		[Test]
		[Description("Verifies that the correct sequence of modification is fired when a log line is assembled from many reads")]
		public void TestReadOneLine4()
		{
			_writer.Write("A");
			_writer.Flush();
			_scheduler.RunOnce();

			_writer.Write("B");
			_writer.Flush();
			_scheduler.RunOnce();

			_writer.Write("C");
			_writer.Flush();
			_scheduler.RunOnce();

			_changes.Should().Equal(new object[]
			{
				LogFileSection.Reset,
				new LogFileSection(0, 1),
				LogFileSection.Invalidate(0, 1),
				new LogFileSection(0, 1),
				LogFileSection.Invalidate(0, 1),
				new LogFileSection(0, 1)
			}, "because the log file should've sent invalidations for the 2nd and 3rd read (because the same line was modified)");

			_file.Count.Should().Be(1);
			_file.GetLine(0).Should().Be(new LogLine(0, 0, "ABC", LevelFlags.None));
		}

		[Test]
		public void TestReadTwoLines1()
		{
			_writer.Write("Hello\r\n");
			_writer.Flush();
			_scheduler.RunOnce();

			_writer.Write("World!\r\n");
			_writer.Flush();
			_scheduler.RunOnce();

			_file.Count.Should().Be(2);
			_file.GetLine(0).Should().Be(new LogLine(0, 0, "Hello", LevelFlags.None));
			_file.GetLine(1).Should().Be(new LogLine(1, 1, "World!", LevelFlags.None));
		}
	}
}