using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogLineTest
	{
		[Test]
		public void TestConstruction1()
		{
			var line = new LogLine(1, 2, "Foobar", LevelFlags.Info);
			line.LineIndex.Should().Be(1);
			line.OriginalLineIndex.Should().Be(1);
			line.LogEntryIndex.Should().Be(2);
			line.Message.Should().Be("Foobar");
			line.Level.Should().Be(LevelFlags.Info);
			line.Timestamp.Should().NotHaveValue();
			line.MatchedFilters.Should().Be(0);
		}

		[Test]
		public void TestConstruction2()
		{
			var line = new LogLine(1, 500, 2, "Foobar", LevelFlags.Error);
			line.LineIndex.Should().Be(1);
			line.OriginalLineIndex.Should().Be(500);
			line.LogEntryIndex.Should().Be(2);
			line.Message.Should().Be("Foobar");
			line.Level.Should().Be(LevelFlags.Error);
			line.MatchedFilters.Should().Be(0);
		}

		[Test]
		public void TestConstruction3()
		{
			var line = new LogLine(0, 0, null, LevelFlags.All, null);
			line.LineIndex.Should().Be(0);
			line.OriginalLineIndex.Should().Be(0);
			line.LogEntryIndex.Should().Be(0);
			line.Message.Should().BeNull();
			line.Level.Should().Be(LevelFlags.All);
			line.Timestamp.Should().BeNull();
			line.MatchedFilters.Should().Be(0);
		}

		[Test]
		public void TestConstruction4()
		{
			var line = new LogLine(0, 0, 0, null, LevelFlags.All, null, 42);
			line.LineIndex.Should().Be(0);
			line.OriginalLineIndex.Should().Be(0);
			line.LogEntryIndex.Should().Be(0);
			line.Message.Should().BeNull();
			line.Level.Should().Be(LevelFlags.All);
			line.Timestamp.Should().BeNull();
			line.MatchedFilters.Should().Be(42);
		}

		[Test]
		public void TestDetermineLevelsFromLine1()
		{
			new Action(() => LogLine.DetermineLevelFromLine(null)).ShouldNotThrow();
			LogLine.DetermineLevelFromLine(null).Should().Be(LevelFlags.None);
		}

		[Test]
		public void TestDetermineLevelsFromLine2()
		{
			LogLine.DetermineLevelFromLine("TRACE").Should().Be(LevelFlags.Trace);
			LogLine.DetermineLevelFromLine("DEBUG").Should().Be(LevelFlags.Debug);
			LogLine.DetermineLevelFromLine("INFO").Should().Be(LevelFlags.Info);
			LogLine.DetermineLevelFromLine("WARN").Should().Be(LevelFlags.Warning);
			LogLine.DetermineLevelFromLine("ERROR").Should().Be(LevelFlags.Error);
			LogLine.DetermineLevelFromLine("FATAL").Should().Be(LevelFlags.Fatal);
		}

		[Test]
		[Description("Verifies that DetermineLevelFromLine returns the FIRST occurence of alog level")]
		public void TestDetermineLevelsFromLine3()
		{
			LogLine.DetermineLevelFromLine("TRACE DEBUG INFO WARNING ERROR FATAL").Should().Be(LevelFlags.Trace);
			LogLine.DetermineLevelFromLine("DEBUG TRACE INFO WARNING ERROR FATAL").Should().Be(LevelFlags.Debug);
			LogLine.DetermineLevelFromLine("INFO WARNING ERROR FATAL DEBUG TRACE").Should().Be(LevelFlags.Info);
			LogLine.DetermineLevelFromLine("WARN INFO ERROR FATAL TRACE DEBUG").Should().Be(LevelFlags.Warning);
			LogLine.DetermineLevelFromLine("ERROR INFO WARNING TRACE DEBUG FATAL").Should().Be(LevelFlags.Error);
			LogLine.DetermineLevelFromLine("FATAL ERROR WARNING INFO DEBUG TRACE").Should().Be(LevelFlags.Fatal);
		}
	}
}