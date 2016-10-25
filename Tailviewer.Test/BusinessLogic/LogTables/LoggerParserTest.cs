using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class LoggerParserTest
	{
		private ColumnParser _parser;

		[SetUp]
		public void Setup()
		{
			_parser = new LoggerParser();
		}

		[Test]
		public void TestParse1()
		{
			int length;
			_parser.Parse("SomeSpecialLogger", 0, out length)
				   .Should().Be("SomeSpecialLogger");
			length.Should().Be(17);
		}

		[Test]
		public void TestParse2()
		{
			int length;
			_parser.Parse("Tailviewer.Test.BusinessLogic.LogTables", 0, out length)
				   .Should().Be("Tailviewer.Test.BusinessLogic.LogTables");
			length.Should().Be(39);
		}
	}
}