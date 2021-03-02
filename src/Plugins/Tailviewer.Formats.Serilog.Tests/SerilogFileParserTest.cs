using System;
using System.Diagnostics.Contracts;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Formats.Serilog;

namespace Tailviewer.Serilog.Test
{
	[TestFixture]
	public class SerilogFileParserTest
	{
		[Pure]
		private static IReadOnlyLogEntry Parse(SerilogEntryParser parser, string rawContent)
		{
			var logEntry = new LogEntry(GeneralColumns.Index, GeneralColumns.RawContent)
			{
				Index = 0,
				RawContent = rawContent
			};
			return parser.Parse(logEntry);
		}

		[Test]
		public void TestParse_Timestamp_Custom_1()
		{
			var parser = new SerilogEntryParser("[{Timestamp:dd-MM-yyyy HH:mm:ss}]");
			Parse(parser, "[15-09-2020 22:58:10]").Timestamp.Should().Be(new DateTime(2020, 9, 15, 22, 58, 10));
			Parse(parser, "[15-09-1999 03:58:10]").Timestamp.Should().Be(new DateTime(1999, 9, 15, 3, 58, 10));
		}

		[Test]
		public void TestParse_Timestamp_Custom_2()
		{
			var parser = new SerilogEntryParser("[{Timestamp:dd.MM.yyyy HH:mm:ss.f}]");
			Parse(parser, "[16.09.2020 02:29:36.7]").Timestamp.Should().Be(new DateTime(2020, 9, 16, 02, 29, 36, 700));
			Parse(parser, "[16.09.2021 12:29:36.7]").Timestamp.Should().Be(new DateTime(2021, 9, 16, 12, 29, 36, 700));
		}

		[Test]
		public void TestParse_Timestamp_Custom_3()
		{
			var parser = new SerilogEntryParser("[{Timestamp:yyyy-MM-dd HH:mm:ss.ff}]");
			Parse(parser, "[2020-09-16 01:30:51.21]").Timestamp.Should().Be(new DateTime(2020, 9, 16, 1, 30, 51, 210));
			Parse(parser, "[2019-09-16 13:30:51.21]").Timestamp.Should().Be(new DateTime(2019, 9, 16, 13, 30, 51, 210));
		}

		[Test]
		public void TestParse_Timestamp_Custom_4()
		{
			var parser = new SerilogEntryParser("[{Timestamp:yyyy-MM-dd HH:mm:ss.ff}]");
			Parse(parser, "[2020-09-16 01:30:51.21]").Timestamp.Should().Be(new DateTime(2020, 9, 16, 1, 30, 51, 210));
		}

		[Test]
		public void TestParse_Timestamp_Custom_5()
		{
			var parser = new SerilogEntryParser("[{Timestamp:MM.dd.yyyy HH mm ss fff}]");
			Parse(parser, "[09.16.2020 01 32 34 342]").Timestamp.Should().Be(new DateTime(2020, 9, 16, 1, 32, 34, 342));
		}

		[Test]
		public void TestParse_Timestamp_Custom_6()
		{
			var parser = new SerilogEntryParser("[{Timestamp:dd/MM/yyyy HH:mm:ss K}]");
			// TODO: Is this correct? What do we actually want here?
			Parse(parser, "[16/09/2020 00:45:39 +02:00]").Timestamp.Should().Be(new DateTime(2020, 9, 16, 0, 45, 39));
			Parse(parser, "[15/09/2020 20:26:52 -07:00]").Timestamp.Should().Be(new DateTime(2020, 9, 15, 20, 26, 52));
		}

		[Test]
		public void TestParse_LogLevel_u1()
		{
			var parser = new SerilogEntryParser("[{Level:u1}]");
			Parse(parser, "[V]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[D]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[I]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[W]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[E]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[F]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_LogLevel_u2()
		{
			var parser = new SerilogEntryParser("[{Level:u2}]");
			Parse(parser, "[VB]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[DE]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[IN]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[WN]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[ER]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[FA]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_LogLevel_u3()
		{
			var parser = new SerilogEntryParser("[{Level:u3}]");
			Parse(parser, "[VRB]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[DBG]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[INF]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[WRN]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[ERR]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[FTL]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_LogLevel_u4()
		{
			var parser = new SerilogEntryParser("[{Level:u4}]");
			Parse(parser, "[VERB]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[DBUG]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[INFO]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[WARN]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[EROR]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[FATL]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_LogLevel_u5()
		{
			var parser = new SerilogEntryParser("[{Level:u5}]");
			Parse(parser, "[VERBO]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[DEBUG]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[INFOR]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[WARNI]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[ERROR]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[FATAL]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_LogLevel_u6()
		{
			var parser = new SerilogEntryParser("[{Level:u6}]");
			Parse(parser, "[VERBOS]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[DEBUG]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[INFORM]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[WARNIN]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[ERROR]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[FATAL]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_LogLevel_u7()
		{
			var parser = new SerilogEntryParser("[{Level:u7}]");
			Parse(parser, "[VERBOSE]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[DEBUG]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[INFORMA]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[WARNING]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[ERROR]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[FATAL]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_LogLevel_u8()
		{
			var parser = new SerilogEntryParser("[{Level:u8}]");
			Parse(parser, "[VERBOSE]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[DEBUG]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[INFORMAT]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[WARNING]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[ERROR]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[FATAL]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_LogLevel_u9()
		{
			var parser = new SerilogEntryParser("[{Level:u9}]");
			Parse(parser, "[VERBOSE]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[DEBUG]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[INFORMATI]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[WARNING]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[ERROR]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[FATAL]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_LogLevel_u10()
		{
			var parser = new SerilogEntryParser("[{Level:u10}]");
			Parse(parser, "[VERBOSE]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[DEBUG]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[INFORMATIO]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[WARNING]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[ERROR]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[FATAL]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_LogLevel_u11()
		{
			var parser = new SerilogEntryParser("[{Level:u11}]");
			Parse(parser, "[VERBOSE]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[DEBUG]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[INFORMATION]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[WARNING]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[ERROR]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[FATAL]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_LogLevel()
		{
			var parser = new SerilogEntryParser("[{Level}]");
			Parse(parser, "[Verbose]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[Debug]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[Information]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[Warning]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[Error]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[Fatal]").LogLevel.Should().Be(LevelFlags.Fatal);
		}

		[Test]
		public void TestParse_Message()
		{
			var parser = new SerilogEntryParser("{Message}");
			Parse(parser, "This is an error").GetValue(GeneralColumns.Message).Should().Be("This is an error");
		}

		[Test]
		public void TestParse_Timestamp_LogLevel()
		{
			var parser = new SerilogEntryParser("{Timestamp:dd/MM/yyyy HH:mm:ss K} [{Level}]");
			var logEntry = Parse(parser, "16/09/2020 00:45:39 +02:00 [Verbose]");
			logEntry.Timestamp.Should().Be(new DateTime(2020, 09, 16, 00, 45, 39));
			logEntry.LogLevel.Should().Be(LevelFlags.Trace);
			logEntry.RawContent.Should().Be("16/09/2020 00:45:39 +02:00 [Verbose]");
		}

		[Test]
		public void TestParse_Timestamp_LogLevel_Message()
		{
			var parser = new SerilogEntryParser("{Timestamp:dd/MM/yyyy HH:mm:ss K} [{Level}] {Message}");
			var logEntry = Parse(parser, "16/09/2020 01:21:59 +02:00 [Fatal] This is a fatal message!");
			logEntry.Timestamp.Should().Be(new DateTime(2020, 09, 16, 01, 21, 59));
			logEntry.LogLevel.Should().Be(LevelFlags.Fatal);
			logEntry.GetValue(GeneralColumns.Message).Should().Be("This is a fatal message!");
		}

		[Test]
		public void TestParse_Timestamp2_LogLevel_Message()
		{
			var parser = new SerilogEntryParser("{Timestamp:yyyy-MM-dd HH:mm:ss.fff K} [{Level:u3}] {Message}");
			var logEntry = Parse(parser, "2020-09-13 00:00:12.207 +04:30 [DBG] Fetch modification job triggered at 9/13/2020 12:00:12 AM!!!");
			logEntry.Timestamp.Should().Be(new DateTime(2020, 09, 13, 00, 00, 12, 207));
			logEntry.LogLevel.Should().Be(LevelFlags.Debug);
			logEntry.GetValue(GeneralColumns.Message).Should().Be("Fetch modification job triggered at 9/13/2020 12:00:12 AM!!!");
		}

		[Test]
		public void TestParse_NullMessage()
		{
			var parser = new SerilogEntryParser("{Timestamp:dd/MM/yyyy HH:mm:ss K} [{Level}] {Message}");
			var logEntry = Parse(parser, null);
			logEntry.RawContent.Should().BeNullOrEmpty();
		}
	}
}