using System;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
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

		private FileLogSource Create(string fileName)
		{
			return new FileLogSource(_services, fileName);
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
				entries.Should().HaveCount(9997);
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
	}
}
