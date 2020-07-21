using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Test.BusinessLogic.LogFiles;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class TextLogFileTest
		: AbstractLogFileTest
	{
		private ManualTaskScheduler _scheduler;
		private string _fname;
		private FileStream _stream;
		private StreamWriter _streamWriter;
		private TextLogFile _file;
		private Mock<ILogFileListener> _listener;
		private List<LogFileSection> _changes;
		private BinaryWriter _binaryWriter;

		[SetUp]
		public void Setup()
		{
			_scheduler = new ManualTaskScheduler();
			_fname = Path.GetTempFileName();
			if (File.Exists(_fname))
				File.Delete(_fname);

			_stream = File.Open(_fname, FileMode.Create, FileAccess.Write, FileShare.Read);
			_streamWriter = new StreamWriter(_stream);
			_binaryWriter = new BinaryWriter(_stream);

			_file = new TextLogFile(_scheduler, _fname);

			_listener = new Mock<ILogFileListener>();
			_changes = new List<LogFileSection>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
				.Callback((ILogFile unused, LogFileSection section) => _changes.Add(section));

			_file.AddListener(_listener.Object, TimeSpan.Zero, 10);
		}

		[TearDown]
		public void TearDown()
		{
			_binaryWriter?.Dispose();
			//_streamWriter?.Dispose();
			_stream?.Dispose();
		}

		[Test]
		public void TestCtor()
		{
			_streamWriter.Write("Foo");
			_streamWriter.Flush();

			var info = new FileInfo(_fname);
			_scheduler.RunOnce();

			_file.GetValue(LogFileProperties.LastModified).Should().Be(info.LastWriteTime);
			_file.GetValue(LogFileProperties.Created).Should().Be(info.CreationTime);
		}

		[Test]
		public void TestEncodingLatin1()
		{
			_binaryWriter.Write((byte) 0x36);
			_binaryWriter.Write((byte) 0x35);
			_binaryWriter.Write((byte) 0xB0);
			_binaryWriter.Write((byte) '\r');
			_binaryWriter.Write((byte) '\n');
			_binaryWriter.Flush();

			_file = new TextLogFile(_scheduler, _fname, encoding: Encoding.GetEncoding(1252));
			_scheduler.RunOnce();

			_file.Count.Should().Be(1);
			var line = _file.GetLine(0);
			line.Message.Should().Be("65°");
		}

		[Test]
		public void TestEncodingUtf8()
		{
			_binaryWriter.Write((byte)0x36);
			_binaryWriter.Write((byte)0x35);
			_binaryWriter.Write((byte)0xC2);
			_binaryWriter.Write((byte)0xB0);
			_binaryWriter.Write((byte)'\r');
			_binaryWriter.Write((byte)'\n');
			_binaryWriter.Flush();

			_file = new TextLogFile(_scheduler, _fname);
			_scheduler.RunOnce();

			_file.Count.Should().Be(1);
			var line = _file.GetLine(0);
			line.Message.Should().Be("65°");
		}

		[Test]
		public void TestTranslator1()
		{
			var translator = new Mock<ILogLineTranslator>();
			_file = new TextLogFile(_scheduler, _fname, translator: translator.Object);
			_file.AddListener(_listener.Object, TimeSpan.Zero, 10);

			_streamWriter.Write("Foo");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			translator.Verify(x => x.Translate(It.Is<ILogFile>(y => y == _file), It.IsAny<LogLine>()),
				Times.Once);
		}

		[Test]
		public void TestDoesNotExist()
		{
			_file = new TextLogFile(_scheduler, _fname);
			_scheduler.RunOnce();
			_file.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.None);
			_file.GetValue(LogFileProperties.Created).Should().NotBe(DateTime.MinValue);

			_streamWriter?.Dispose();
			_stream?.Dispose();
			File.Delete(_fname);
			_scheduler.RunOnce();

			_file.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);
			_file.GetValue(LogFileProperties.Created).Should().BeNull();
		}

		[Test]
		public void TestExclusiveAccess()
		{
			_streamWriter?.Dispose();
			_stream?.Dispose();

			using (var stream = new FileStream(_fname, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				_file = new TextLogFile(_scheduler, _fname);
				_scheduler.RunOnce();

				_file.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceCannotBeAccessed);
				_file.GetValue(LogFileProperties.Created).Should().NotBe(DateTime.MinValue);
				_file.GetValue(LogFileProperties.Created).Should().Be(new FileInfo(_fname).CreationTime);
			}
		}

		[Test]
		public void TestReadOneLine1()
		{
			_streamWriter.Write("Foo");
			_streamWriter.Flush();

			_scheduler.RunOnce();
			_file.Count.Should().Be(1);
			_file.GetLine(0).Should().Be(new LogLine(0, 0, "Foo", LevelFlags.Other));
		}

		[Test]
		[Description("Verifies that a line written in two separate flushes is correctly assembly to a single log line")]
		public void TestReadOneLine2()
		{
			_streamWriter.Write("Hello ");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_streamWriter.Write("World!");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_file.Count.Should().Be(1);
			_file.GetLine(0).Should().Be(new LogLine(0, 0, "Hello World!", LevelFlags.Other));
		}

		[Test]
		[Description("Verifies that a line written in three separate flushes is correctly assembly to a single log line")]
		public void TestReadOneLine3()
		{
			_streamWriter.Write("A");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_streamWriter.Write("B");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_streamWriter.Write("C");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_file.Count.Should().Be(1);
			_file.GetLine(0).Should().Be(new LogLine(0, 0, "ABC", LevelFlags.Other));
		}

		[Test]
		[Description("Verifies that the correct sequence of modification is fired when a log line is assembled from many reads")]
		public void TestReadOneLine4()
		{
			_streamWriter.Write("A");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_streamWriter.Write("B");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_streamWriter.Write("C");
			_streamWriter.Flush();
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
			_file.GetLine(0).Should().Be(new LogLine(0, 0, "ABC", LevelFlags.Other));
		}

		[Test]
		public void TestReadTwoLines1()
		{
			_streamWriter.Write("Hello\r\n");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_streamWriter.Write("World!\r\n");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_file.Count.Should().Be(2);
			_file.GetLine(0).Should().Be(new LogLine(0, 0, "Hello", LevelFlags.Other));
			_file.GetLine(1).Should().Be(new LogLine(1, 1, "World!", LevelFlags.Other));
		}

		[Test]
		public void TestGetColumnTimestamp1()
		{
			_streamWriter.Write("2017-12-03 11:59:30 INFO Foo\r\n");
			_streamWriter.Write("2017-12-03 12:00:00 INFO Bar\r\n");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_file.Count.Should().Be(2);
			_file.GetColumn(new LogFileSection(0, 2), LogFileColumns.Timestamp).Should().Equal(new object[]
			{
				new DateTime(2017, 12, 3, 11, 59, 30),
				new DateTime(2017, 12, 3, 12, 00, 00)
			});

			_file.GetColumn(new LogFileSection(1, 1), LogFileColumns.Timestamp)
			     .Should().Equal(new DateTime(2017, 12, 3, 12, 00, 00));
		}

		[Test]
		[Description("Verifies that columns can be retrieved by indices")]
		public void TestGetColumnTimestamp2()
		{
			_streamWriter.Write("2017-12-03 11:59:30 INFO Foo\r\n");
			_streamWriter.Write("2017-12-03 12:00:00 INFO Bar\r\n");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_file.Count.Should().Be(2);
			_file.GetColumn(new LogLineIndex[] {0,  1}, LogFileColumns.Timestamp).Should().Equal(new object[]
			{
				new DateTime(2017, 12, 3, 11, 59, 30),
				new DateTime(2017, 12, 3, 12, 00, 00)
			});
			_file.GetColumn(new LogLineIndex[] { 1, 0 }, LogFileColumns.Timestamp).Should().Equal(new object[]
			{
				new DateTime(2017, 12, 3, 12, 00, 00),
				new DateTime(2017, 12, 3, 11, 59, 30)
			});
			_file.GetColumn(new LogLineIndex[] { 1 }, LogFileColumns.Timestamp).Should().Equal(new object[]
			{
				new DateTime(2017, 12, 3, 12, 00, 00),
			});
		}

		[Test]
		[Description("Verifies that indexing invalid rows is allowed and returns the default value for that particular index")]
		public void TestGetColumnTimestamp3()
		{
			_streamWriter.Write("2017-12-03 11:59:30 INFO Foo\r\n");
			_streamWriter.Write("2017-12-03 12:00:00 INFO Bar\r\n");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_file.Count.Should().Be(2);
			_file.GetColumn(new LogLineIndex[] { -1 }, LogFileColumns.Timestamp).Should().Equal(new object[]
			{
				null
			});
			_file.GetColumn(new LogLineIndex[] { 1, 2 }, LogFileColumns.Timestamp).Should().Equal(new object[]
			{
				new DateTime(2017, 12, 3, 12, 00, 00),
				null
			});
			_file.GetColumn(new LogLineIndex[] { 1, 2, 0 }, LogFileColumns.Timestamp).Should().Equal(new object[]
			{
				new DateTime(2017, 12, 3, 12, 00, 00),
				null,
				new DateTime(2017, 12, 3, 11, 59, 30)
			});
		}

		[Test]
		public void TestGetColumnDeltaTime1()
		{
			_streamWriter.Write("2017-12-03 11:59:30 INFO Foo\r\n");
			_streamWriter.Write("2017-12-03 12:00:00 INFO Bar\r\n");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_file.Count.Should().Be(2);
			_file.GetColumn(new LogFileSection(0, 2), LogFileColumns.DeltaTime).Should().Equal(new object[]
			{
				null,
				TimeSpan.FromSeconds(30)
			});

			_file.GetColumn(new LogFileSection(1, 1), LogFileColumns.DeltaTime)
			     .Should().Equal(new object[] {TimeSpan.FromSeconds(30)});
		}

		[Test]
		[Description("Verifies that out-of-bounds access is tolerated")]
		public void TestGetColumnDeltaTime2()
		{
			_streamWriter.Write("2017-12-03 11:59:30 INFO Foo\r\n");
			_streamWriter.Flush();
			_scheduler.RunOnce();

			_file.GetColumn(new LogFileSection(0, 2), LogFileColumns.DeltaTime).Should().Equal(new object[]
			{
				null,
				null
			});
			_file.GetColumn(new LogFileSection(1, 5), LogFileColumns.DeltaTime).Should().Equal(new object[]
			{
				null,
				null,
				null,
				null,
				null
			});
		}

		[Test]
		public void Test()
		{
			_file.GetValue(LogFileProperties.Name).Should().Be(_fname);
		}

		protected override ILogFile CreateEmpty()
		{
			var logFile = new TextLogFile(_scheduler, "fawwaaw");
			_scheduler.RunOnce();
			return logFile;
		}

		protected override ILogFile CreateFromContent(IReadOnlyLogEntries content)
		{
			var fname = Path.GetTempFileName();
			using (var stream = File.OpenWrite(fname))
			using(var writer = new StreamWriter(stream))
			{
				foreach (var logEntry in content)
				{
					writer.Write(logEntry.ToString());
					writer.WriteLine();
				}
			}

			var logFile = new TextLogFile(_scheduler, fname);
			_scheduler.RunOnce();
			return logFile;
		}
	}
}