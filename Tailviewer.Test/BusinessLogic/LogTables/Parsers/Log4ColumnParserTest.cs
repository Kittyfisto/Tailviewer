using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogTables.Parsers;

namespace Tailviewer.Test.BusinessLogic.LogTables.Parsers
{
	[TestFixture]
	public sealed class Log4ColumnParserTest
	{
		[Test]
		public void TestCreate1()
		{
			int length;
			Log4ColumnParser.Create("%timestamp", 0, out length).Should().NotBeNull();
			length.Should().Be(10);
		}

		[Test]
		public void TestCreate2()
		{
			int length;
			var parser = Log4ColumnParser.Create("[%timestamp", 0, out length);
			parser.Should().NotBeNull();
			parser.Pattern.Should().Be("[%timestamp");
			length.Should().Be(11);
		}

		[Test]
		public void TestCreate3()
		{
			int length;
			Log4ColumnParser.Create("     [%timestamp", 5, out length).Should().NotBeNull();
			length.Should().Be(11);
		}
	}
}