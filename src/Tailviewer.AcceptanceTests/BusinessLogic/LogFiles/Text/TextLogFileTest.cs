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
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Plugins;
using Tailviewer.Test;
using Tailviewer.Test.BusinessLogic.LogFiles;
using Certainty = Tailviewer.Certainty;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles.Text
{
	[TestFixture]
	public sealed class TextLogFileTest
		: AbstractLogFileTest
	{
		private ManualTaskScheduler _taskScheduler;
		private string _fname;
		private FileStream _stream;
		private StreamWriter _streamWriter;
		private TextLogSource _source;
		private Mock<ILogSourceListener> _listener;
		private List<LogFileSection> _changes;
		private BinaryWriter _binaryWriter;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();
			_fname = PathEx.GetTempFileName();
			if (File.Exists(_fname))
				File.Delete(_fname);

			_stream = File.Open(_fname, FileMode.Create, FileAccess.Write, FileShare.Read);
			_streamWriter = new StreamWriter(_stream);
			_binaryWriter = new BinaryWriter(_stream);

			_source = Create(_fname);

			_listener = new Mock<ILogSourceListener>();
			_changes = new List<LogFileSection>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogFileSection>()))
				.Callback((ILogSource unused, LogFileSection section) => _changes.Add(section));

			_source.AddListener(_listener.Object, TimeSpan.Zero, 10);
		}

		[TearDown]
		public void TearDown()
		{
			_binaryWriter?.Dispose();
			//_streamWriter?.Dispose();
			_stream?.Dispose();
		}

		private TextLogSource Create(string fileName,
		                           Encoding encoding = null,
		                           ILogLineTranslator translator = null,
		                           ILogFileFormatMatcher matcher = null,
		                           ITextLogFileParserPlugin parser = null)
		{
			var serviceContainer = new ServiceContainer();
			serviceContainer.RegisterInstance<ITaskScheduler>(_taskScheduler);
			if (encoding != null)
				serviceContainer.RegisterInstance<Encoding>(encoding);
			if (translator != null)
				serviceContainer.RegisterInstance<ILogLineTranslator>(translator);
			if (matcher == null)
				matcher = new SimpleLogFileFormatMatcher(LogFileFormats.GenericText);
			serviceContainer.RegisterInstance<ILogFileFormatMatcher>(matcher);
			if (parser == null)
				parser = new SimpleTextLogFileParserPlugin();
			serviceContainer.RegisterInstance<ITextLogFileParserPlugin>(parser);
			return new TextLogSource(serviceContainer, fileName);
		}

		[Test]
		public void TestCtor()
		{
			_source.Columns.Should().Equal(LogColumns.Minimum);

			_streamWriter.Write("Foo");
			_streamWriter.Flush();

			_source.GetProperty(GeneralProperties.Format).Should().Be(null);
			_source.GetProperty(GeneralProperties.FormatDetectionCertainty).Should().Be(Certainty.None);

			var info = new FileInfo(_fname);
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LastModified).Should().Be(info.LastWriteTime);
			_source.GetProperty(GeneralProperties.Created).Should().Be(info.CreationTime);
			_source.GetProperty(GeneralProperties.Format).Should().Be(LogFileFormats.GenericText);
			_source.GetProperty(GeneralProperties.FormatDetectionCertainty).Should().Be(Certainty.Uncertain);
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

			_source = Create(_fname, encoding: Encoding.GetEncoding(1252));
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			var entry = _source.GetEntry(0);
			entry.RawContent.Should().Be("65°");
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

			_source = Create(_fname);
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			var line = _source.GetEntry(0);
			line.RawContent.Should().Be("65°");
		}

		[Test]
		public void TestTranslator1()
		{
			var translator = new Mock<ILogLineTranslator>();
			_source = Create(_fname, translator: translator.Object);
			_source.AddListener(_listener.Object, TimeSpan.Zero, 10);

			_streamWriter.Write("Foo");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			translator.Verify(x => x.Translate(It.Is<ILogSource>(y => y == _source), It.IsAny<LogLine>()),
				Times.Once);
		}

		[Test]
		public void TestDoesNotExist()
		{
			_source = Create(_fname);
			_taskScheduler.RunOnce();
			_source.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.None);
			_source.GetProperty(GeneralProperties.Created).Should().NotBe(DateTime.MinValue);
			_source.GetProperty(GeneralProperties.Format).Should().Be(LogFileFormats.GenericText);
			_source.GetProperty(GeneralProperties.FormatDetectionCertainty).Should().NotBe(Certainty.None);

			_streamWriter?.Dispose();
			_stream?.Dispose();
			File.Delete(_fname);
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist);
			_source.GetProperty(GeneralProperties.Created).Should().BeNull();
			_source.GetProperty(GeneralProperties.Format).Should().BeNull();
			_source.GetProperty(GeneralProperties.FormatDetectionCertainty).Should().Be(Certainty.None);
		}

		[Test]
		public void TestExclusiveAccess()
		{
			_streamWriter?.Dispose();
			_stream?.Dispose();

			using (var stream = new FileStream(_fname, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				_source = Create(_fname);
				_taskScheduler.RunOnce();

				_source.GetProperty(GeneralProperties.EmptyReason).Should().Be(ErrorFlags.SourceCannotBeAccessed);
				_source.GetProperty(GeneralProperties.Created).Should().NotBe(DateTime.MinValue);
				_source.GetProperty(GeneralProperties.Created).Should().Be(new FileInfo(_fname).CreationTime);
			}
		}

		[Test]
		public void TestReadOneLine1()
		{
			_streamWriter.Write("Foo");
			_streamWriter.Flush();

			_taskScheduler.RunOnce();
			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			var entry = _source.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("Foo");
			entry.LogLevel.Should().Be(LevelFlags.Other);
		}

		[Test]
		[Description("Verifies that a line written in two separate flushes is correctly assembly to a single log line")]
		public void TestReadOneLine2()
		{
			_streamWriter.Write("Hello ");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_streamWriter.Write("World!");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			var entry = _source.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("Hello World!");
			entry.LogLevel.Should().Be(LevelFlags.Other);
		}

		[Test]
		[Description("Verifies that a line written in three separate flushes is correctly assembly to a single log line")]
		public void TestReadOneLine3()
		{
			_streamWriter.Write("A");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_streamWriter.Write("B");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_streamWriter.Write("C");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			var entry = _source.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("ABC");
			entry.LogLevel.Should().Be(LevelFlags.Other);
		}

		[Test]
		[Description("Verifies that the correct sequence of modification is fired when a log line is assembled from many reads")]
		public void TestReadOneLine4()
		{
			_streamWriter.Write("A");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_streamWriter.Write("B");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_streamWriter.Write("C");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_changes.Should().Equal(new object[]
			{
				LogFileSection.Reset,
				new LogFileSection(0, 1),
				LogFileSection.Invalidate(0, 1),
				new LogFileSection(0, 1),
				LogFileSection.Invalidate(0, 1),
				new LogFileSection(0, 1)
			}, "because the log file should've sent invalidations for the 2nd and 3rd read (because the same line was modified)");

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);
			var entry = _source.GetEntry(0);
			entry.Index.Should().Be(0);
			entry.LogEntryIndex.Should().Be(0);
			entry.RawContent.Should().Be("ABC");
			entry.LogLevel.Should().Be(LevelFlags.Other);
		}

		[Test]
		public void TestReadTwoLines1()
		{
			_source.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.Zero);

			_streamWriter.Write("Hello\r\n");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();
			_source.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because the log file should have processed the entire file");

			_streamWriter.Write("World!\r\n");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();
			_source.GetProperty(GeneralProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because the log file should have processed the entire file");

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(2);
			var entries = _source.GetEntries(new LogFileSection(0, 2));
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("Hello");
			entries[0].LogLevel.Should().Be(LevelFlags.Other);
			
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].RawContent.Should().Be("World!");
			entries[1].LogLevel.Should().Be(LevelFlags.Other);
		}

		[Test]
		public void TestGetColumnTimestamp1()
		{
			_streamWriter.Write("2017-12-03 11:59:30 INFO Foo\r\n");
			_streamWriter.Write("2017-12-03 12:00:00 INFO Bar\r\n");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(2);
			_source.GetColumn(new LogFileSection(0, 2), LogColumns.Timestamp).Should().Equal(new object[]
			{
				new DateTime(2017, 12, 3, 11, 59, 30),
				new DateTime(2017, 12, 3, 12, 00, 00)
			});

			_source.GetColumn(new LogFileSection(1, 1), LogColumns.Timestamp)
			     .Should().Equal(new DateTime(2017, 12, 3, 12, 00, 00));
		}

		[Test]
		[Description("Verifies that columns can be retrieved by indices")]
		public void TestGetColumnTimestamp2()
		{
			_streamWriter.Write("2017-12-03 11:59:30 INFO Foo\r\n");
			_streamWriter.Write("2017-12-03 12:00:00 INFO Bar\r\n");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(2);
			_source.GetColumn(new LogLineIndex[] {0,  1}, LogColumns.Timestamp).Should().Equal(new object[]
			{
				new DateTime(2017, 12, 3, 11, 59, 30),
				new DateTime(2017, 12, 3, 12, 00, 00)
			});
			_source.GetColumn(new LogLineIndex[] { 1, 0 }, LogColumns.Timestamp).Should().Equal(new object[]
			{
				new DateTime(2017, 12, 3, 12, 00, 00),
				new DateTime(2017, 12, 3, 11, 59, 30)
			});
			_source.GetColumn(new LogLineIndex[] { 1 }, LogColumns.Timestamp).Should().Equal(new object[]
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
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(2);
			_source.GetColumn(new LogLineIndex[] { -1 }, LogColumns.Timestamp).Should().Equal(new object[]
			{
				null
			});
			_source.GetColumn(new LogLineIndex[] { 1, 2 }, LogColumns.Timestamp).Should().Equal(new object[]
			{
				new DateTime(2017, 12, 3, 12, 00, 00),
				null
			});
			_source.GetColumn(new LogLineIndex[] { 1, 2, 0 }, LogColumns.Timestamp).Should().Equal(new object[]
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
			_taskScheduler.RunOnce();

			_source.GetProperty(GeneralProperties.LogEntryCount).Should().Be(2);
			_source.GetColumn(new LogFileSection(0, 2), LogColumns.DeltaTime).Should().Equal(new object[]
			{
				null,
				TimeSpan.FromSeconds(30)
			});

			_source.GetColumn(new LogFileSection(1, 1), LogColumns.DeltaTime)
			     .Should().Equal(new object[] {TimeSpan.FromSeconds(30)});
		}

		[Test]
		[Description("Verifies that out-of-bounds access is tolerated")]
		public void TestGetColumnDeltaTime2()
		{
			_streamWriter.Write("2017-12-03 11:59:30 INFO Foo\r\n");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();

			_source.GetColumn(new LogFileSection(0, 2), LogColumns.DeltaTime).Should().Equal(new object[]
			{
				null,
				null
			});
			_source.GetColumn(new LogFileSection(1, 5), LogColumns.DeltaTime).Should().Equal(new object[]
			{
				null,
				null,
				null,
				null,
				null
			});
		}

		[Test]
		public void TestMaxCharactersInLine()
		{
			_source = Create(_fname, encoding: Encoding.GetEncoding(1252));
			_source.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(0, "because the file didn't do any processing and thus should default to 0");

			_taskScheduler.RunOnce();
			_source.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(0, "because the file is empty");

			_streamWriter.WriteLine("Hello");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();
			_source.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(5);

			_streamWriter.WriteLine("you");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();
			_source.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(5, "because we're interested in the maximum amount of characters of the greatest line which is still 5");

			_streamWriter.WriteLine("Good morning");
			_streamWriter.Flush();
			_taskScheduler.RunOnce();
			_source.GetProperty(TextProperties.MaxCharactersInLine).Should().Be(12, "because we've now written a larger line");
		}

		[Test]
		public void Test()
		{
			_source.GetProperty(GeneralProperties.Name).Should().Be(_fname);
		}

		sealed class TestMatcher
		: ILogFileFormatMatcher
		{
			public ILogFileFormat Format;
			public bool Success;
			public int NumTries;

			#region Implementation of ILogFileFormatMatcher

			public bool TryMatchFormat(string fileName, byte[] initialContent, out ILogFileFormat format)
			{
				++NumTries;
				format = Format;
				return Success;
			}

			#endregion
		}

		[Test]
		[Description("Verifies that the log file will create new parsers when the format detection changes its mind")]
		public void TestFormatDetection()
		{
			var matcher = new TestMatcher
			{
				Format = LogFileFormats.GenericText,
				Success = true
			};

			var parser = new Mock<ITextLogFileParserPlugin>();
			parser.Setup(x => x.CreateParser(It.IsAny<IServiceContainer>(),
			                                 It.IsAny<ILogFileFormat>())).Returns(() => new Mock<ITextLogFileParser>().Object);
			_source = Create(_fname, matcher: matcher, parser: parser.Object);

			_streamWriter.WriteLine("Hello, World!");
			_taskScheduler.RunOnce();
			_source.GetProperty(GeneralProperties.Format).Should().Be(matcher.Format);
			_source.GetProperty(GeneralProperties.FormatDetectionCertainty).Should().Be(Certainty.Uncertain);
			matcher.NumTries.Should().Be(1);
			parser.Verify(x => x.CreateParser(It.IsAny<IServiceContainer>(),
			                                  LogFileFormats.GenericText),
			              Times.Once);
			parser.ResetCalls();


			// Now let's write a second line: Our matcher will still claim its a generic text file
			// and we expect the log file to still invoke the matcher, but NOT create a new parser
			// since the format itself hasn't changed, even if its still uncertain.
			_streamWriter.WriteLine("Second line");
			_taskScheduler.RunOnce();
			_source.GetProperty(GeneralProperties.Format).Should().Be(matcher.Format);
			_source.GetProperty(GeneralProperties.FormatDetectionCertainty).Should().Be(Certainty.Uncertain);
			matcher.NumTries.Should().Be(2, "because the log file should perform another match since the first one wasn't certain");
			parser.Verify(x => x.CreateParser(It.IsAny<IServiceContainer>(),
			                                  LogFileFormats.GenericText),
			              Times.Never);
			parser.ResetCalls();


			// And now things become interesting. Now our matcher claims the format is a different one.
			// We expect the log file to create a new parser to accomodate this!
			matcher.Format = LogFileFormats.Csv;
			_streamWriter.WriteLine("This is interesting, it's a CSV now!");
			_taskScheduler.RunOnce();
			_source.GetProperty(GeneralProperties.Format).Should().Be(matcher.Format);
			_source.GetProperty(GeneralProperties.FormatDetectionCertainty).Should().Be(Certainty.Uncertain);
			matcher.NumTries.Should().Be(3, "because the log file should perform another match since the file is still too small to be certain");
			parser.Verify(x => x.CreateParser(It.IsAny<IServiceContainer>(),
			                                  LogFileFormats.GenericText),
			              Times.Never);
			parser.Verify(x => x.CreateParser(It.IsAny<IServiceContainer>(),
			                                  LogFileFormats.Csv),
			              Times.Once);
		}

		protected override ILogSource CreateEmpty()
		{
			var logFile = Create("fawwaaw");
			_taskScheduler.RunOnce();
			return logFile;
		}

		protected override ILogSource CreateFromContent(IReadOnlyLogBuffer content)
		{
			var fname = PathEx.GetTempFileName();
			using (var stream = File.OpenWrite(fname))
			using(var writer = new StreamWriter(stream))
			{
				foreach (var logEntry in content)
				{
					if(logEntry.TryGetValue(LogColumns.Timestamp, out var timestamp) && timestamp != null)
					{
						// Let's write the timestamp in a format everybody recognizes
						writer.Write("{0:yyyy-MM-dd HH:mm:ss.fffffff}", timestamp);
					}
					writer.Write(logEntry.ToString());
					writer.WriteLine();
				}
			}

			var logFile = Create(fname);
			_taskScheduler.RunOnce();
			logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(content.Count);
			return logFile;
		}
	}
}