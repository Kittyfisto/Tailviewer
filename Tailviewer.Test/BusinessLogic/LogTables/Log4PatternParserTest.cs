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
			parser.Columns.ElementAt(1).Type.Should().Be(ColumnType.ThreadName);
			parser.Columns.ElementAt(2).Type.Should().Be(ColumnType.Level);
			parser.Columns.ElementAt(3).Type.Should().Be(ColumnType.Logger);
			parser.Columns.ElementAt(4).Type.Should().Be(ColumnType.Message);
		}
	}
}