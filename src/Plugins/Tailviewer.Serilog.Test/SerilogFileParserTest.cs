using System.Diagnostics.Contracts;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Serilog.Test
{
	[TestFixture]
	public class SerilogFileParserTest
	{
		[Pure]
		private static IReadOnlyLogEntry Parse(SerilogFileParser parser, string rawContent)
		{
			var logEntry = new LogEntry2(LogFileColumns.RawContent)
			{
				RawContent = rawContent
			};
			return parser.Parse(logEntry);
		}

		[Test]
		public void TestParse_LogLevel_u1()
		{
			var parser = new SerilogFileParser("[{Level:u1}]");
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
			var parser = new SerilogFileParser("[{Level:u2}]");
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
			var parser = new SerilogFileParser("[{Level:u3}]");
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
			var parser = new SerilogFileParser("[{Level:u4}]");
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
			var parser = new SerilogFileParser("[{Level:u5}]");
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
			var parser = new SerilogFileParser("[{Level:u6}]");
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
			var parser = new SerilogFileParser("[{Level:u7}]");
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
			var parser = new SerilogFileParser("[{Level:u8}]");
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
			var parser = new SerilogFileParser("[{Level:u9}]");
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
			var parser = new SerilogFileParser("[{Level:u10}]");
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
			var parser = new SerilogFileParser("[{Level:u11}]");
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
			var parser = new SerilogFileParser("[{Level}]");
			Parse(parser, "[Verbose]").LogLevel.Should().Be(LevelFlags.Trace);
			Parse(parser, "[Debug]").LogLevel.Should().Be(LevelFlags.Debug);
			Parse(parser, "[Information]").LogLevel.Should().Be(LevelFlags.Info);
			Parse(parser, "[Warning]").LogLevel.Should().Be(LevelFlags.Warning);
			Parse(parser, "[Error]").LogLevel.Should().Be(LevelFlags.Error);
			Parse(parser, "[Fatal]").LogLevel.Should().Be(LevelFlags.Fatal);
		}
	}
}