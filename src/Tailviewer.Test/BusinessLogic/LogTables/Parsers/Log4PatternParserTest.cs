using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogTables;
using Tailviewer.Core.LogTables.Parsers;

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
			parser.Parsers.Count().Should().Be(5);
			parser.Parsers.ElementAt(0).Type.Should().Be(ColumnType.Timestamp);
			parser.Parsers.ElementAt(0).Pattern.Should().Be("%timestamp [");
			parser.Parsers.ElementAt(0).Name.Should().Be("timestamp");

			parser.Parsers.ElementAt(1).Type.Should().Be(ColumnType.Thread);
			parser.Parsers.ElementAt(1).Pattern.Should().Be("%thread] ");
			parser.Parsers.ElementAt(1).Name.Should().Be("thread");

			parser.Parsers.ElementAt(2).Type.Should().Be(ColumnType.Level);
			parser.Parsers.ElementAt(2).Pattern.Should().Be("%level ");
			parser.Parsers.ElementAt(2).Name.Should().Be("level");

			parser.Parsers.ElementAt(3).Type.Should().Be(ColumnType.Logger);
			parser.Parsers.ElementAt(3).Pattern.Should().Be("%logger - ");
			parser.Parsers.ElementAt(3).Name.Should().Be("logger");

			parser.Parsers.ElementAt(4).Type.Should().Be(ColumnType.Message);
			parser.Parsers.ElementAt(4).Pattern.Should().Be("%message");
			parser.Parsers.ElementAt(4).Name.Should().Be("message");
		}

		[Test]
		[Description("Verifies that the parser is tollerates pattern that don't start with a pttern right away")]
		public void TestCtor3()
		{
			var parser = new Log4PatternParser(" %logger");
			parser.Parsers.Count().Should().Be(1);
			parser.Parsers.ElementAt(0).Type.Should().Be(ColumnType.Logger);
			parser.Parsers.ElementAt(0).Name.Should().Be("logger");
		}

		[Test]
		[Description("Verifies that the parser is tollerates pattern that don't start with a pttern right away")]
		public void TestCtor4()
		{
			var parser = new Log4PatternParser("	%logger");
			parser.Parsers.Count().Should().Be(1);
			parser.Parsers.ElementAt(0).Type.Should().Be(ColumnType.Logger);
			parser.Parsers.ElementAt(0).Name.Should().Be("logger");
		}

		[Test]
		[Description("Verifies that the parser is tollerates pattern that don't start with a pttern right away")]
		public void TestCtor5()
		{
			var parser = new Log4PatternParser("[%logger]");
			parser.Parsers.Count().Should().Be(1);
			parser.Parsers.ElementAt(0).Type.Should().Be(ColumnType.Logger);
			parser.Parsers.ElementAt(0).Name.Should().Be("logger");
		}

		[Test]
		[Description("Verifies that the parser is able to extract the column-name from a format modifier")]
		public void TestCtor6()
		{
			var parser = new Log4PatternParser("%-20.30logger");
			parser.Parsers.Count().Should().Be(1);
			parser.Parsers.ElementAt(0).Type.Should().Be(ColumnType.Logger);
			parser.Parsers.ElementAt(0).Name.Should().Be("logger");
		}

		[Test]
		[Ignore("Not yet implemented")]
		[Description("Verifies that the parser is capable of recognizing an escaped '%' in front of an actual pattern")]
		public void TestCtor7()
		{
			var parser = new Log4PatternParser("%% %logger");
			parser.Parsers.Count().Should().Be(1);
			parser.Parsers.ElementAt(0).Type.Should().Be(ColumnType.Logger);
			parser.Parsers.ElementAt(0).Name.Should().Be("logger");
		}

		[Test]
		public void TestCtor8()
		{
			var parser = new Log4PatternParser("%utcdate{ABSOLUTE}");
			parser.Parsers.Count().Should().Be(1);
			parser.Parsers.ElementAt(0).Type.Should().Be(ColumnType.UtcDate);
			parser.Parsers.ElementAt(0).Format.Should().Be("{ABSOLUTE}");
		}

		[Test]
		public void TestCtor9()
		{
			var parser = new Log4PatternParser("[%level]");
			parser.Parsers.Count().Should().Be(1);
			parser.Parsers.ElementAt(0).Type.Should().Be(ColumnType.Level);
		}

		[Test]
		public void TestParseLevel1()
		{
			var parser = new Log4PatternParser("%level");
			parser.Parse(new LogLine(0, 0, "DEBUG", LevelFlags.Other)).Fields.Should().Equal(new object[] {LevelFlags.Debug});
		}

		[Test]
		public void TestParseLevel2()
		{
			var parser = new Log4PatternParser("[%level]");
			parser.Parse(new LogLine(0, 0, "[DEBUG]", LevelFlags.Other)).Fields.Should().Equal(new object[] {LevelFlags.Debug});
		}

		[Test]
		[Description("Verifies that parsers don't crash when the input doesn't match the pattern")]
		public void TestParseLevel3()
		{
			var parser = new Log4PatternParser("[%level]");
			parser.Parse(new LogLine(0, 0, null, LevelFlags.Other)).Fields.Should().Equal(new object[1]);
			parser.Parse(new LogLine(0, 0, string.Empty, LevelFlags.Other)).Fields.Should().Equal(new object[1]);
			parser.Parse(new LogLine(0, 0, "DEBUG]", LevelFlags.Other)).Fields.Should().Equal(new object[1]);
			parser.Parse(new LogLine(0, 0, "[DEBUG", LevelFlags.Other)).Fields.Should().Equal(new object[1]);
			parser.Parse(new LogLine(0, 0, "[debug]", LevelFlags.Other)).Fields.Should().Equal(new object[1]);
			parser.Parse(new LogLine(0, 0, "[DeBuG]", LevelFlags.Other)).Fields.Should().Equal(new object[1]);
		}

		[Test]
		[Ignore("Not yet implemented")]
		public void TestParseDate1()
		{
			var parser = new Log4PatternParser("%date");
			parser.Parse(new LogLine(0, 0, "2016-10-26 09:06:35,176", LevelFlags.Other)).Fields.Should().Equal(new object[] {DateTime.Parse("2016-10-26 09:06:35,176")});
		}
	}
}