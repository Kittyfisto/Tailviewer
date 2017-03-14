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