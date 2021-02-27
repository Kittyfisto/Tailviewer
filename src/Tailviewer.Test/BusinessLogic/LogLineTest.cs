using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;

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
			line.SourceId.Should().Be(LogEntrySourceId.Default);
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
			line.SourceId.Should().Be(LogEntrySourceId.Default);
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
			line.SourceId.Should().Be(LogEntrySourceId.Default);
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
			line.SourceId.Should().Be(LogEntrySourceId.Default);
		}

		[Test]
		public void TestConstruction5()
		{
			var line = new LogLine(0, 0, 0, new LogEntrySourceId(42), null, LevelFlags.All, null, 42);
			line.LineIndex.Should().Be(0);
			line.OriginalLineIndex.Should().Be(0);
			line.LogEntryIndex.Should().Be(0);
			line.Message.Should().BeNull();
			line.Level.Should().Be(LevelFlags.All);
			line.Timestamp.Should().BeNull();
			line.MatchedFilters.Should().Be(42);
			line.SourceId.Should().Be(new LogEntrySourceId(42));
		}

		[Test]
		public void TestConstruction6()
		{
			var originalLine = new LogLine(1, 2, 3, "foobar", LevelFlags.Trace, new DateTime(2017, 10, 16, 23, 28, 20));
			var line = new LogLine(new LogEntrySourceId(200), originalLine);
			line.LineIndex.Should().Be(1);
			line.OriginalLineIndex.Should().Be(2);
			line.LogEntryIndex.Should().Be(3);
			line.SourceId.Should().Be(new LogEntrySourceId(200));
			line.Message.Should().Be("foobar");
			line.Level.Should().Be(LevelFlags.Trace);
			line.Timestamp.Should().Be(new DateTime(2017, 10, 16, 23, 28, 20));
			line.MatchedFilters.Should().Be(0);
		}

		[Test]
		public void TestConstruction7()
		{
			var originalLine = new LogLine(1, 2, "foobar", LevelFlags.Trace);
			var line = new LogLine(3, 4, new LogEntrySourceId(128), originalLine);
			line.LineIndex.Should().Be(3);
			line.OriginalLineIndex.Should().Be(3);
			line.LogEntryIndex.Should().Be(4);
			line.SourceId.Should().Be(new LogEntrySourceId(128));
			line.Message.Should().Be("foobar");
			line.Level.Should().Be(LevelFlags.Trace);
			line.Timestamp.Should().BeNull();
			line.MatchedFilters.Should().Be(0);
		}

		[Test]
		public void TestConstruction8()
		{
			var line = new LogLine(1, 2, 3, new LogEntrySourceId(201), "stuff", LevelFlags.Trace, new DateTime(2017, 10, 16, 23, 43, 00));
			line.LineIndex.Should().Be(1);
			line.OriginalLineIndex.Should().Be(2);
			line.LogEntryIndex.Should().Be(3);
			line.SourceId.Should().Be(new LogEntrySourceId(201));
			line.Message.Should().Be("stuff");
			line.Level.Should().Be(LevelFlags.Trace);
			line.Timestamp.Should().Be(new DateTime(2017, 10, 16, 23, 43, 00));
			line.MatchedFilters.Should().Be(0);
		}

		[Test]
		public void TestConstruction9()
		{
			var lineIndex = new LogLineIndex(1);
			var entryIndex = new LogEntryIndex(42);
			var sourceId = new LogEntrySourceId(254);
			var t= new DateTime(2017, 11, 26, 12, 20, 1);
			var line = new LogLine(lineIndex, entryIndex, sourceId, "Hello, World!", LevelFlags.Trace, t);
			line.LineIndex.Should().Be(1);
			line.OriginalLineIndex.Should().Be(1);
			line.LogEntryIndex.Should().Be(42);
			line.SourceId.Should().Be(sourceId);
			line.Message.Should().Be("Hello, World!");
			line.Level.Should().Be(LevelFlags.Trace);
			line.Timestamp.Should().Be(t);
			line.MatchedFilters.Should().Be(0);
		}

		[Test]
		[Description("Verifies that two lines with different data source ids are not equal")]
		public void TestEquality1()
		{
			var line = new LogLine(0, 0, 0, new LogEntrySourceId(42), null, LevelFlags.Trace, null, 0);
			var otherLine = new LogLine(0, 0, 0, new LogEntrySourceId(41), null, LevelFlags.Trace, null, 0);

			line.Equals(otherLine).Should().BeFalse();
			otherLine.Equals(line).Should().BeFalse();
		}

		[Test]
		public void TestToString1()
		{
			var line = new LogLine(1, 2, 3, new LogEntrySourceId(4), "foobar", LevelFlags.Trace, null, 0);
			line.ToString().Should().Be("#1 (Original #3) (Source #4): foobar");
		}

		[Test]
		public void TestDetermineLevelsFromLine1()
		{
			new Action(() => LogLine.DetermineLevelFromLine(null)).Should().NotThrow();
			LogLine.DetermineLevelFromLine(null).Should().Be(LevelFlags.Other);
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