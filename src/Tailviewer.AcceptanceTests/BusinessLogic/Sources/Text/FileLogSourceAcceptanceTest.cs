using System;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text.Simple;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Text;
using Tailviewer.Plugins;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text
{
	[TestFixture]
	public sealed class FileLogSourceAcceptanceTest
	{
		private ServiceContainer _services;
		private DefaultTaskScheduler _taskScheduler;
		private Mock<ILogSourceParserPlugin> _parser;
		private SimpleLogFileFormatMatcher _formatMatcher;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();
			_taskScheduler = new DefaultTaskScheduler();
			_parser = new Mock<ILogSourceParserPlugin>();
			_parser.Setup(x => x.CreateParser(It.IsAny<IServiceContainer>(), It.IsAny<ILogSource>()))
			       .Returns((IServiceContainer services, ILogSource source) =>
			       {
				       return new GenericTextLogSource(source, new GenericTextLogEntryParser());
			       });

			_formatMatcher = new SimpleLogFileFormatMatcher(null);

			_services.RegisterInstance<IFileLogSourceFactory>(new FileLogSourceFactory(_taskScheduler));
			_services.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_services.RegisterInstance<ILogSourceParserPlugin>(_parser.Object);
			_services.RegisterInstance<ILogFileFormatMatcher>(_formatMatcher);
		}

		[TearDown]
		public void TearDown()
		{
			_taskScheduler.Dispose();
		}

		private FileLogSource Create(string fileName)
		{
			return new FileLogSource(_services, fileName);
		}

		private string GetUniqueNonExistingFileName()
		{
			var fileName = PathEx.GetTempFileName();
			if (File.Exists(fileName))
				File.Delete(fileName);

			TestContext.WriteLine("FileName: {0}", fileName);
			return fileName;
		}

		private StreamWriter CreateStreamWriter(string fileName)
		{
			var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
			return new StreamWriter(stream);
		}

		[Test]
		public void TestFileDoesNotExist()
		{
			var fileName = @"TestData\Encodings\does_not_exist";
			using (var logSource = Create(fileName))
			{
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
				logSource.GetProperty(GeneralProperties.Format).Should().BeNull("because the file doesn't exist");
				logSource.GetProperty(GeneralProperties.Created).Should().BeNull("because the file doesn't exist");
				logSource.GetProperty(GeneralProperties.LastModified).Should().BeNull("because the file doesn't exist");
				logSource.GetProperty(GeneralProperties.Size).Should().BeNull("because the file doesn't exist");
				logSource.GetProperty(TextProperties.AutoDetectedEncoding).Should().BeNull("because the file doesn't exist");
				logSource.GetProperty(TextProperties.ByteOrderMark).Should().BeNull("because the file doesn't exist");
				logSource.GetProperty(TextProperties.OverwrittenEncoding).Should().BeNull("because the file doesn't exist");
				logSource.GetProperty(TextProperties.Encoding).Should().BeNull("because the file doesn't exist");
			}
		}

		[Test]
		public void TestFileProperties()
		{
			var fileName = @"TestData\1Mb.txt";
			using (var logSource = Create(fileName))
			{
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);

				var info = new FileInfo(fileName);
				logSource.GetProperty(GeneralProperties.Format).Should().Be(LogFileFormats.GenericText, "because we didn't provide the log file with a working detector");
				logSource.GetProperty(GeneralProperties.Created).Should().Be(info.CreationTimeUtc);
				logSource.GetProperty(GeneralProperties.LastModified).Should().Be(info.LastWriteTimeUtc);
				logSource.GetProperty(GeneralProperties.Size).Should().Be(Size.FromBytes(info.Length));

				var entries = logSource.GetEntries();
				//entries.Should().HaveCount(9996);
				entries.Should().HaveCount(9997); //< Streaming and non streaming handle empty newlines differently
				entries[0].RawContent.Should().Be("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");
			}
		}

		#region Encodings

		[Test]
		[Description("Verifies that FileLogSource properly detects files encoded in UTF-8 by its byte order mark")]
		public void TestEncoding_Detect_UTF8_Bom()
		{
			var fileName = @"TestData\Encodings\utf8_w_bom.txt";
			using (var logSource = Create(fileName))
			{
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);

				logSource.GetProperty(TextProperties.AutoDetectedEncoding).Should().Be(Encoding.UTF8, "because the source file has a BOM which should have been detected");
				logSource.GetProperty(TextProperties.ByteOrderMark).Should().Be(true, "because the source contains a byte order mark");
				logSource.GetProperty(TextProperties.Encoding).Should().Be(Encoding.UTF8, "because the source should use the auto detected encoding to read the file");
				logSource.GetProperty(TextProperties.OverwrittenEncoding).Should().BeNull("because we didn't specify any overwritten encoding");

				var entries = logSource.GetEntries();
				entries.Should().HaveCount(1);
				entries[0].RawContent.Should().Be("2015-10-07 19:50:58,997 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
			}
		}

		[Test]
		[Description("Verifies that FileLogSource properly detects files encoded in UTF-16 big endian by its byte order mark")]
		public void TestEncoding_Detect_UTF16_BE_Bom()
		{
			var fileName = @"TestData\Encodings\utf16_be_bom.txt";
			using (var logSource = Create(fileName))
			{
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);

				logSource.GetProperty(TextProperties.AutoDetectedEncoding).Should().Be(Encoding.BigEndianUnicode, "because the source file has a BOM which should have been detected");
				logSource.GetProperty(TextProperties.ByteOrderMark).Should().Be(true, "because the source contains a byte order mark");
				logSource.GetProperty(TextProperties.Encoding).Should().Be(Encoding.BigEndianUnicode, "because the source should use the auto detected encoding to read the file");
				logSource.GetProperty(TextProperties.OverwrittenEncoding).Should().BeNull("because we didn't specify any overwritten encoding");

				var entries = logSource.GetEntries();
				entries.Should().HaveCount(1);
				entries[0].RawContent.Should().Be("2015-10-07 19:50:58,997 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
			}
		}

		[Test]
		[Description("Verifies that FileLogSource properly detects files encoded in UTF-16 little endian by its byte order mark")]
		public void TestEncoding_Detect_UTF16_LE_Bom()
		{
			var fileName = @"TestData\Encodings\utf16_le_bom.txt";
			using (var logSource = Create(fileName))
			{
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);

				logSource.GetProperty(TextProperties.AutoDetectedEncoding).Should().Be(Encoding.Unicode, "because the source file has a BOM which should have been detected");
				logSource.GetProperty(TextProperties.ByteOrderMark).Should().Be(true, "because the source contains a byte order mark");
				logSource.GetProperty(TextProperties.Encoding).Should().Be(Encoding.Unicode, "because the source should use the auto detected encoding to read the file");
				logSource.GetProperty(TextProperties.OverwrittenEncoding).Should().BeNull("because we didn't specify any overwritten encoding");

				var entries = logSource.GetEntries();
				entries.Should().HaveCount(1);
				entries[0].RawContent.Should().Be("2015-10-07 19:50:58,997 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
			}
		}

		[Test]
		[FlakyTest(2)]
		[LocalTest("Why doesn't this work on AppVeyor anymore?")]
		public void TestEncoding_Overwrite_Windows_1252()
		{
			// Let's say the user was sensible and set their default encoding to UTF8.
			// But now we're reading some ancient log file which was encoded using the 1250 code page.
			// Tailviewer can't automatically detect (yet) that this isn't UTF8 and therefore should
			// interpret the values in the log file wrong.
			_services.RegisterInstance<Encoding>(Encoding.UTF8);

			var fileName = @"TestData\Encodings\windows_1252.txt";
			using (var logSource = Create(fileName))
			{
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
				logSource.GetProperty(TextProperties.AutoDetectedEncoding).Should().BeNull("because there's no BOM and thus Tailviewer may not have claimed to auto detect the encoding");
				logSource.GetProperty(TextProperties.OverwrittenEncoding).Should().BeNull("because we didn't overwrite the encoding just yet");
				logSource.GetProperty(TextProperties.Encoding).Should().Be(Encoding.UTF8, "because we specified at the start that UTF8 should be used as a default encoding");

				var entries = logSource.GetEntries();
				entries.Should().HaveCount(1);
				entries[0].RawContent.Should().Be("42� North");

				var overwrittenEncoding = Encoding.GetEncoding(1252);
				logSource.SetProperty(TextProperties.OverwrittenEncoding, overwrittenEncoding);
				logSource.Property(x => x.GetProperty(TextProperties.Encoding)).ShouldEventually().Be(overwrittenEncoding);
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
				logSource.GetProperty(TextProperties.AutoDetectedEncoding).Should().BeNull("because there's still no BOM and thus Tailviewer may not have claimed to auto detect the encoding");
				logSource.GetProperty(TextProperties.Encoding).Should().Be(overwrittenEncoding, "because we've overwritten the encoding for this source");

				entries = logSource.GetEntries();
				entries.Should().HaveCount(1);
				entries[0].RawContent.Should().Be("42° North");
			}
		}

		#endregion

		#region Log Level Count

		[Test]
		[Description("Verifies that the levels are counted correctly")]
		public void TestLevelCount1()
		{
			var fileName = @"TestData\LevelCounts.txt";
			using (var logSource = Create(fileName))
			{
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
				logSource.GetProperty(GeneralProperties.LogEntryCount).Should().Be(27, "because the data source contains that many lines");
				logSource.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(6, "because the data source contains six trace lines");
				logSource.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(1, "because the data source contains one debug line");
				logSource.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(2, "because the data source contains two info lines");
				logSource.GetProperty(GeneralProperties.WarningLogEntryCount).Should().Be(3, "because the data source contains three warnings");
				logSource.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(4, "because the data source contains four errors");
				logSource.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(5, "because the data source contains five fatal lines");
				logSource.GetProperty(GeneralProperties.OtherLogEntryCount).Should().Be(6, "because the file is read in singleline mode and 6 lines don't have a recognizable log level");
			}
		}

		[Test]
		[Ignore("I've broken this but I don't know where and how")]
		[Description("Verifies that the levels are counted correctly")]
		public void TestLevelCount2()
		{
			var fileName = TextLogSourceAcceptanceTest.File20Mb;
			using (var logSource = Create(fileName))
			{
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(15)).Be(Percentage.HundredPercent);

				logSource.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldEventually().Be(165342);
				logSource.GetProperty(GeneralProperties.TraceLogEntryCount).Should().Be(0);
				logSource.GetProperty(GeneralProperties.DebugLogEntryCount).Should().Be(165337);
				logSource.GetProperty(GeneralProperties.InfoLogEntryCount).Should().Be(5);
				logSource.GetProperty(GeneralProperties.ErrorLogEntryCount).Should().Be(0);
				logSource.GetProperty(GeneralProperties.FatalLogEntryCount).Should().Be(0);
			}
		}

		[Test]
		[Description("Verifies that the level of a log line is unambigously defined")]
		public void TestLevelPrecedence()
		{
			var fileName = @"TestData\DifferentLevels.txt";
			using (var logSource = Create(fileName))
			{
				logSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
				
				logSource.GetProperty(GeneralProperties.LogEntryCount).Should().Be(6, "because the file consists of 6 lines");

				var entries = logSource.GetEntries(new LogFileSection(0, 6));
				entries[0].RawContent.Should().Be("DEBUG ERROR WARN FATAL INFO");
				entries[0].LogLevel.Should().Be(LevelFlags.Debug, "Because DEBUG is the first level to appear in the line");

				entries[1].RawContent.Should().Be("INFO DEBUG ERROR WARN FATAL");
				entries[1].LogLevel.Should().Be(LevelFlags.Info, "Because INFO is the first level to appear in the line");

				entries[2].RawContent.Should().Be("WARN ERROR FATAL INFO DEBUG");
				entries[2].LogLevel.Should().Be(LevelFlags.Warning, "Because WARN is the first level to appear in the line");

				entries[3].RawContent.Should().Be("ERROR INFO DEBUG FATAL WARN");
				entries[3].LogLevel.Should().Be(LevelFlags.Error, "Because ERROR is the first level to appear in the line");

				entries[4].RawContent.Should().Be("FATAL ERROR INFO WARN DEBUG");
				entries[4].LogLevel.Should().Be(LevelFlags.Fatal, "Because FATAL is the first level to appear in the line");

				// TODO: Multiline
				//entries[5].RawContent.Should().Be("fatal error info warn debug");
				//entries[5].LogLevel.Should()
				//          .Be(LevelFlags.Fatal,
				//              "Because this line belongs to the previous log entry and thus is marked as fatal as well");
				//entries[5].LogEntryIndex.Should().Be(entries[4].LogEntryIndex);
			}
		}

		#endregion

		#region Timestamps

		[Test]
		public void TestTimestampFormat1()
		{
			using (var file = Create(@"TestData\Timestamps\yyyy-MM-dd HH_mm_ss_fff.txt"))
			{
				file.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);

				var entry = file.GetEntry(0);
				entry.Timestamp.Should().Be(new DateTime(2017, 5, 10, 20, 40, 3, 143, DateTimeKind.Unspecified));
			}
		}

		[Test]
		public void TestTimestampFormat2()
		{
			using (var file = Create(@"TestData\Timestamps\yyyy-MM-dd HH_mm_ss.txt"))
			{
				file.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);

				var entry = file.GetEntry(0);
				entry.Timestamp.Should().Be(new DateTime(2017, 5, 10, 20, 40, 3, DateTimeKind.Unspecified));
			}
		}

		[Test]
		public void TestTimestampFormat3()
		{
			using (var file = Create( @"TestData\Timestamps\HH_mm_ss.txt"))
			{
				file.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);

				var entry = file.GetEntry(0);
				var today = DateTime.Today;
				entry.Timestamp.Should().Be(new DateTime(today.Year, today.Month, today.Day, 21, 04, 33, DateTimeKind.Unspecified));
			}
		}

		[Test]
		public void TestTimestampFormat4()
		{
			using (var file = Create(@"TestData\Timestamps\ddd MMM dd HH_mm_ss.fff yyyy.txt"))
			{
				file.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);

				var entry = file.GetEntry(0);
				entry.Timestamp.Should().Be(new DateTime(2017, 5, 5, 8, 46, 44, 257, DateTimeKind.Unspecified));
			}
		}

		[Test]
		public void TestTimestampFormat5()
		{
			using (var file = Create(@"TestData\Timestamps\yyyy MMM dd HH_mm_ss.fff.txt"))
			{
				file.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetProperty(GeneralProperties.LogEntryCount).Should().Be(2);

				var entry = file.GetEntry(1);
				entry.Timestamp.Should().Be(new DateTime(2017, 5, 9, 6, 51, 57, 583, DateTimeKind.Unspecified));
			}
		}

		[Test]
		public void TestTimestampFormat6()
		{
			using (var file = Create(@"TestData\Timestamps\HH_mm_ss;s.txt"))
			{
				file.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetProperty(GeneralProperties.LogEntryCount).Should().Be(2);

				var today = DateTime.Today;
				var entry = file.GetEntry(0);
				entry.Timestamp.Should().Be(new DateTime(today.Year, today.Month, today.Day, 6, 51, 57, 135, DateTimeKind.Unspecified));
				entry = file.GetEntry(1);
				entry.Timestamp.Should().Be(new DateTime(today.Year, today.Month, today.Day, 6, 53, 06, 341, DateTimeKind.Unspecified));
			}
		}

		[Test]
		public void TestGetColumnTimestamp1()
		{
			var fileName = GetUniqueNonExistingFileName();
			using (var writer = CreateStreamWriter(fileName))
			using (var source = Create(fileName))
			{
				writer.Write("2017-12-03 11:59:30 INFO Foo\r\n");
				writer.Write("2017-12-03 12:00:00 INFO Bar\r\n");
				writer.Flush();

				source.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldEventually().BeGreaterOrEqualTo(2);
				source.GetColumn(new LogFileSection(0, 2), GeneralColumns.Timestamp).Should().Equal(new object[]
				{
					new DateTime(2017, 12, 3, 11, 59, 30),
					new DateTime(2017, 12, 3, 12, 00, 00)
				});

				source.GetColumn(new LogFileSection(1, 1), GeneralColumns.Timestamp)
				      .Should().Equal(new DateTime(2017, 12, 3, 12, 00, 00));
			}
		}

		[Test]
		[Description("Verifies that columns can be retrieved by indices")]
		public void TestGetColumnTimestamp2()
		{
			var fileName = GetUniqueNonExistingFileName();
			using (var writer = CreateStreamWriter(fileName))
			using (var source = Create(fileName))
			{
				writer.Write("2017-12-03 11:59:30 INFO Foo\r\n");
				writer.Write("2017-12-03 12:00:00 INFO Bar\r\n");
				writer.Flush();
				
				source.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldEventually().BeGreaterOrEqualTo(2);
				source.GetColumn(new LogLineIndex[] {0,  1}, GeneralColumns.Timestamp).Should().Equal(new object[]
				{
					new DateTime(2017, 12, 3, 11, 59, 30),
					new DateTime(2017, 12, 3, 12, 00, 00)
				});
				source.GetColumn(new LogLineIndex[] { 1, 0 }, GeneralColumns.Timestamp).Should().Equal(new object[]
				{
					new DateTime(2017, 12, 3, 12, 00, 00),
					new DateTime(2017, 12, 3, 11, 59, 30)
				});
				source.GetColumn(new LogLineIndex[] { 1 }, GeneralColumns.Timestamp).Should().Equal(new object[]
				{
					new DateTime(2017, 12, 3, 12, 00, 00),
				});
			}
		}

		[Test]
		[Ignore("Doesn't work just yet")]
		[Description("Verifies that indexing invalid rows is allowed and returns the default value for that particular index")]
		public void TestGetColumnTimestamp3()
		{
			var fileName = GetUniqueNonExistingFileName();
			using (var writer = CreateStreamWriter(fileName))
			using (var source = Create(fileName))
			{
				writer.Write("2017-12-03 11:59:30 INFO Foo\r\n");
				writer.Write("2017-12-03 12:00:00 INFO Bar\r\n");
				writer.Flush();

				source.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldEventually().BeGreaterOrEqualTo(2);
				source.GetColumn(new LogLineIndex[] { -1 }, GeneralColumns.Timestamp).Should().Equal(new object[]
				{
					null
				});
				source.GetColumn(new LogLineIndex[] { 1, 2 }, GeneralColumns.Timestamp).Should().Equal(new object[]
				{
					new DateTime(2017, 12, 3, 12, 00, 00),
					null
				});
				source.GetColumn(new LogLineIndex[] { 1, 2, 0 }, GeneralColumns.Timestamp).Should().Equal(new object[]
				{
					new DateTime(2017, 12, 3, 12, 00, 00),
					null,
					new DateTime(2017, 12, 3, 11, 59, 30)
				});
			}
		}

		[Test]
		public void TestGetColumnDeltaTime1()
		{
			var fileName = GetUniqueNonExistingFileName();
			using (var writer = CreateStreamWriter(fileName))
			using (var source = Create(fileName))
			{
				writer.Write("2017-12-03 11:59:30 INFO Foo\r\n");
				writer.Write("2017-12-03 12:00:00 INFO Bar\r\n");
				writer.Flush();
				
				source.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldEventually().BeGreaterOrEqualTo(2);
				source.GetColumn(new LogFileSection(0, 2), GeneralColumns.DeltaTime).Should().Equal(new object[]
				{
					null,
					TimeSpan.FromSeconds(30)
				});

				source.GetColumn(new LogFileSection(1, 1), GeneralColumns.DeltaTime)
				       .Should().Equal(new object[] {TimeSpan.FromSeconds(30)});
			}
		}

		[Test]
		[Description("Verifies that out-of-bounds access is tolerated")]
		public void TestGetColumnDeltaTime2()
		{
			var fileName = GetUniqueNonExistingFileName();
			using (var writer = CreateStreamWriter(fileName))
			using (var source = Create(fileName))
			{
				writer.Write("2017-12-03 11:59:30 INFO Foo\r\n");
				writer.Flush();

				source.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldEventually().BeGreaterOrEqualTo(1);
				source.GetColumn(new LogFileSection(0, 2), GeneralColumns.DeltaTime).Should().Equal(new object[]
				{
					null,
					null
				});
				source.GetColumn(new LogFileSection(1, 5), GeneralColumns.DeltaTime).Should().Equal(new object[]
				{
					null,
					null,
					null,
					null,
					null
				});
			}
		}

		#endregion

		#region Custom Formats

		[Test]
		public void TestParseCustomFormat()
		{
			
		}

		#endregion

		[Test]
		public void TestGetEntries1()
		{
			using (var file = Create(AbstractTextLogSourceAcceptanceTest.File20Mb))
			{
				file.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(15)).Be(Percentage.HundredPercent, "because we should be able to read the entire file in a few seconds");
				file.GetProperty(GeneralProperties.LogEntryCount).Should().Be(165342);
				file.GetProperty(GeneralProperties.StartTimestamp).Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

				var buffer = file.GetEntries(new LogFileSection(0, 10));


				buffer[0].Index.Should().Be(0);
				buffer[0].RawContent.Should()
				         .Be("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");
				buffer[0].LogLevel.Should().Be(LevelFlags.Info);
				buffer[0].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

				buffer[1].Index.Should().Be(1);
				buffer[1].RawContent.Should()
				         .Be("2015-10-07 19:50:58,998 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
				buffer[1].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[1].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 998));

				buffer[2].Index.Should().Be(2);
				buffer[2].RawContent.Should()
				         .Be("2015-10-07 19:50:59,013 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551613) 'SharpRemote.Heartbeat' implementing 'SharpRemote.IHeartbeat'");
				buffer[2].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[2].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 013));

				buffer[3].Index.Should().Be(3);
				buffer[3].RawContent.Should()
				         .Be("2015-10-07 19:50:59,062 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551614) 'SharpRemote.Latency' implementing 'SharpRemote.ILatency'");
				buffer[3].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[3].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 062));

				buffer[4].Index.Should().Be(4);
				buffer[4].RawContent.Should()
				         .Be("2015-10-07 19:50:59,067 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551615) 'SharpRemote.Hosting.SubjectHost' implementing 'SharpRemote.Hosting.ISubjectHost'");
				buffer[4].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[4].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 067));

				buffer[5].Index.Should().Be(5);
				buffer[5].RawContent.Should()
				         .Be("2015-10-07 19:50:59,081 [8092, 1] INFO  SharpRemote.SocketRemotingEndPointServer (null) - EndPoint '<Unnamed>' listening on 0.0.0.0:49152");
				buffer[5].LogLevel.Should().Be(LevelFlags.Info);
				buffer[5].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 081));

				buffer[6].Index.Should().Be(6);
				buffer[6].RawContent.Should()
				         .Be("2015-10-07 19:50:59,141 [8092, 6] DEBUG SharpRemote.SocketRemotingEndPointServer (null) - Incoming connection from '127.0.0.1:10348', starting handshake...");
				buffer[6].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[6].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 141));

				buffer[7].Index.Should().Be(7);
				buffer[7].RawContent.Should()
				         .Be("2015-10-07 19:50:59,171 [8092, 6] INFO  SharpRemote.AbstractIPSocketRemotingEndPoint (null) - <Unnamed>: Connected to 127.0.0.1:10348");
				buffer[7].LogLevel.Should().Be(LevelFlags.Info);
				buffer[7].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 171));

				buffer[8].Index.Should().Be(8);
				buffer[8].RawContent.Should()
				         .Be("2015-10-07 19:50:59,181 [8092, 10] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - 0.0.0.0:49152 to 127.0.0.1:10348: sending RPC #1 to 18446744073709551611.Beat");
				buffer[8].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[8].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 181));

				buffer[9].Index.Should().Be(9);
				buffer[9].RawContent.Should()
				         .Be("2015-10-07 19:50:59,182 [8092, 11] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - 0.0.0.0:49152 to 127.0.0.1:10348: sending RPC #2 to 18446744073709551612.Roundtrip");
				buffer[9].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[9].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 182));
			}
		}
	}
}
