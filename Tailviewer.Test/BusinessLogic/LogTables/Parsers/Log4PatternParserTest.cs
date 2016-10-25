using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.LogTables;
using Tailviewer.BusinessLogic.LogTables.Parsers;

namespace Tailviewer.Test.BusinessLogic.LogTables.Parsers
{
	[TestFixture]
	public sealed class Log4PatternParserTest
	{
		[Test]
		public void TestCtor1()
		{
			var parser = new Log4PatternParser("%timestamp [%thread] %level %logger %line - %message%newline");
			parser.Pattern.Should().Be("%timestamp [%thread] %level %logger %line - %message%newline");
		}

		[Test]
		public void TestCtor2()
		{
			var parser = new Log4PatternParser("%timestamp [%thread] %level %logger - %message");
			parser.Columns.Count().Should().Be(5);
			parser.Columns.ElementAt(0).Type.Should().Be(ColumnType.Timestamp);
			parser.Columns.ElementAt(0).Pattern.Should().Be("%timestamp [");
			parser.Columns.ElementAt(0).Name.Should().Be("timestamp");

			parser.Columns.ElementAt(1).Type.Should().Be(ColumnType.Thread);
			parser.Columns.ElementAt(1).Pattern.Should().Be("%thread] ");
			parser.Columns.ElementAt(1).Name.Should().Be("thread");

			parser.Columns.ElementAt(2).Type.Should().Be(ColumnType.Level);
			parser.Columns.ElementAt(2).Pattern.Should().Be("%level ");
			parser.Columns.ElementAt(2).Name.Should().Be("level");

			parser.Columns.ElementAt(3).Type.Should().Be(ColumnType.Logger);
			parser.Columns.ElementAt(3).Pattern.Should().Be("%logger - ");
			parser.Columns.ElementAt(3).Name.Should().Be("logger");

			parser.Columns.ElementAt(4).Type.Should().Be(ColumnType.Message);
			parser.Columns.ElementAt(4).Pattern.Should().Be("%message");
			parser.Columns.ElementAt(4).Name.Should().Be("message");
		}

		[Test]
		[Description("Verifies that the parser is tollerates pattern that don't start with a pttern right away")]
		public void TestCtor3()
		{
			var parser = new Log4PatternParser(" %logger");
			parser.Columns.Count().Should().Be(1);
			parser.Columns.ElementAt(0).Type.Should().Be(ColumnType.Logger);
			parser.Columns.ElementAt(0).Name.Should().Be("logger");
		}

		[Test]
		[Description("Verifies that the parser is tollerates pattern that don't start with a pttern right away")]
		public void TestCtor4()
		{
			var parser = new Log4PatternParser("	%logger");
			parser.Columns.Count().Should().Be(1);
			parser.Columns.ElementAt(0).Type.Should().Be(ColumnType.Logger);
			parser.Columns.ElementAt(0).Name.Should().Be("logger");
		}

		[Test]
		[Description("Verifies that the parser is tollerates pattern that don't start with a pttern right away")]
		public void TestCtor5()
		{
			var parser = new Log4PatternParser("[%logger]");
			parser.Columns.Count().Should().Be(1);
			parser.Columns.ElementAt(0).Type.Should().Be(ColumnType.Logger);
			parser.Columns.ElementAt(0).Name.Should().Be("logger");
		}

		[Test]
		[Description("Verifies that the parser is able to extract the column-name from a format modifier")]
		public void TestCtor6()
		{
			var parser = new Log4PatternParser("%-20.30logger");
			parser.Columns.Count().Should().Be(1);
			parser.Columns.ElementAt(0).Type.Should().Be(ColumnType.Logger);
			parser.Columns.ElementAt(0).Name.Should().Be("logger");
		}

		[Test]
		[Ignore("Not yet implemented")]
		[Description("Verifies that the parser is capable of recognizing an escaped '%' in front of an actual pattern")]
		public void TestCtor7()
		{
			var parser = new Log4PatternParser("%% %logger");
			parser.Columns.Count().Should().Be(1);
			parser.Columns.ElementAt(0).Type.Should().Be(ColumnType.Logger);
			parser.Columns.ElementAt(0).Name.Should().Be("logger");
		}

		[Test]
		public void TestCtor8()
		{
			var parser = new Log4PatternParser("%utcdate{ABSOLUTE}");
			parser.Columns.Count().Should().Be(1);
			parser.Columns.ElementAt(0).Type.Should().Be(ColumnType.UtcDate);
			parser.Columns.ElementAt(0).Format.Should().Be("{ABSOLUTE}");
		}

		[Test]
		public void TestCtor9()
		{
			var parser = new Log4PatternParser("[%level]");
			parser.Columns.Count().Should().Be(1);
			parser.Columns.ElementAt(0).Type.Should().Be(ColumnType.Level);
		}

		[Test]
		public void TestParseLevel1()
		{
			var parser = new Log4PatternParser("%level");
			parser.Parse(new LogLine(0, 0, "DEBUG", LevelFlags.None)).Fields.Should().Equal(new object[] {LevelFlags.Debug});
		}

		[Test]
		public void TestParseLevel2()
		{
			var parser = new Log4PatternParser("[%level]");
			parser.Parse(new LogLine(0, 0, "[DEBUG]", LevelFlags.None)).Fields.Should().Equal(new object[] {LevelFlags.Debug});
		}
	}
}