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
			LogLine.DetermineLevelFromLine("DEBUG").Should().Be(LevelFlags.Debug);
			LogLine.DetermineLevelFromLine("INFO").Should().Be(LevelFlags.Info);
			LogLine.DetermineLevelFromLine("WARN").Should().Be(LevelFlags.Warning);
			LogLine.DetermineLevelFromLine("ERROR").Should().Be(LevelFlags.Error);
			LogLine.DetermineLevelFromLine("FATAL").Should().Be(LevelFlags.Fatal);
		}
	}
}