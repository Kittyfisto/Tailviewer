using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogTables.Parsers;

namespace Tailviewer.Test.BusinessLogic.LogTables.Parsers
{
	[TestFixture]
	public sealed class LevelParserTest
	{
		[Test]
		public void TestParse1()
		{
			var parser = new LevelParser();
			int length;
			parser.Parse("DEBUG", 0, out length).Should().Be(LevelFlags.Debug);
			length.Should().Be(5);
		}

		[Test]
		public void TestParse2()
		{
			var parser = new LevelParser();
			int length;
			parser.Parse("DEBUG INFO", 6, out length).Should().Be(LevelFlags.Info);
			length.Should().Be(4);
		}
		[Test]
		public void TestParse3()
		{
			var parser = new LevelParser();
			int length;
			parser.Parse("WARNING", 0, out length).Should().Be(LevelFlags.Warning);
			length.Should().Be(7);
		}

		[Test]
		public void TestParse4()
		{
			var parser = new LevelParser();
			int length;
			parser.Parse("ERROR", 0, out length).Should().Be(LevelFlags.Error);
			length.Should().Be(5);
		}

		[Test]
		public void TestParse6()
		{
			var parser = new LevelParser();
			int length;
			parser.Parse("FATAL", 0, out length).Should().Be(LevelFlags.Fatal);
			length.Should().Be(5);
		}
	}
}