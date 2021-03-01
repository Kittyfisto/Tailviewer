using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Parsers;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogLevelParserTest
	{
		[Test]
		public void TestDetermineLevelsFromLine1()
		{
			var parser = new LogLevelParser();
			new Action(() => parser.DetermineLevelFromLine(null)).Should().NotThrow();
			parser.DetermineLevelFromLine(null).Should().Be(LevelFlags.Other);
		}

		[Test]
		public void TestDetermineLevelsFromLine2()
		{
			var parser = new LogLevelParser();
			parser.DetermineLevelFromLine("TRACE").Should().Be(LevelFlags.Trace);
			parser.DetermineLevelFromLine("DEBUG").Should().Be(LevelFlags.Debug);
			parser.DetermineLevelFromLine("INFO").Should().Be(LevelFlags.Info);
			parser.DetermineLevelFromLine("WARN").Should().Be(LevelFlags.Warning);
			parser.DetermineLevelFromLine("ERROR").Should().Be(LevelFlags.Error);
			parser.DetermineLevelFromLine("FATAL").Should().Be(LevelFlags.Fatal);
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/326")]
		public void TestDetermineLevelsFromLine3()
		{
			var parser = new LogLevelParser();
			parser.DetermineLevelFromLine("trace").Should().Be(LevelFlags.Other);
			parser.DetermineLevelFromLine("debug").Should().Be(LevelFlags.Other);
			parser.DetermineLevelFromLine("info").Should().Be(LevelFlags.Other);
			parser.DetermineLevelFromLine("warn").Should().Be(LevelFlags.Other);
			parser.DetermineLevelFromLine("error").Should().Be(LevelFlags.Other);
			parser.DetermineLevelFromLine("fatal").Should().Be(LevelFlags.Other);
		}

		[Test]
		[Description("Verifies that DetermineLevelFromLine returns the FIRST occurrence of a log level")]
		public void TestDetermineLevelsFromLine4()
		{
			var parser = new LogLevelParser();
			parser.DetermineLevelFromLine("TRACE DEBUG INFO WARNING ERROR FATAL").Should().Be(LevelFlags.Trace);
			parser.DetermineLevelFromLine("DEBUG TRACE INFO WARNING ERROR FATAL").Should().Be(LevelFlags.Debug);
			parser.DetermineLevelFromLine("INFO WARNING ERROR FATAL DEBUG TRACE").Should().Be(LevelFlags.Info);
			parser.DetermineLevelFromLine("WARN INFO ERROR FATAL TRACE DEBUG").Should().Be(LevelFlags.Warning);
			parser.DetermineLevelFromLine("ERROR INFO WARNING TRACE DEBUG FATAL").Should().Be(LevelFlags.Error);
			parser.DetermineLevelFromLine("FATAL ERROR WARNING INFO DEBUG TRACE").Should().Be(LevelFlags.Fatal);
		}
	}
}