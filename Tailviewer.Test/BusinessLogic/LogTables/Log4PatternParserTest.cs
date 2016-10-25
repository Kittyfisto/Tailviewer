using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class Log4PatternParserTest
	{
		[Test]
		public void TestCtor1()
		{
			var parser = new Log4PatternParser("%timestamp [%thread] %level %logger %ndc - %message%newline");
			parser.Pattern.Should().Be("%timestamp [%thread] %level %logger %ndc - %message%newline");
		}

		[Test]
		public void TestCtor2()
		{
			var parser = new Log4PatternParser("%timestamp [%thread] %level %logger - %message");
			parser.Columns.Count().Should().Be(5);
			parser.Columns.ElementAt(0).Type.Should().Be(ColumnType.Timestamp);
			parser.Columns.ElementAt(0).Name.Should().Be("timestamp");

			parser.Columns.ElementAt(1).Type.Should().Be(ColumnType.Thread);
			parser.Columns.ElementAt(1).Name.Should().Be("thread");

			parser.Columns.ElementAt(2).Type.Should().Be(ColumnType.Level);
			parser.Columns.ElementAt(2).Name.Should().Be("level");

			parser.Columns.ElementAt(3).Type.Should().Be(ColumnType.Logger);
			parser.Columns.ElementAt(3).Name.Should().Be("logger");

			parser.Columns.ElementAt(4).Type.Should().Be(ColumnType.Message);
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
	}
}