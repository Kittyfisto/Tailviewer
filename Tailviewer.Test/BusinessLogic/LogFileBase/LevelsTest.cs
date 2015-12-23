using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Test.BusinessLogic.LogFileBase
{
	[TestFixture]
	public sealed class LevelsTest
	{
		[Test]
		[Description("Verifies that the levels are counted correctly")]
		public void TestLevelCount()
		{
			using (var logFile = new LogFile(@"TestData\LevelCounts.txt"))
			{
				logFile.Start();
				logFile.Wait();

				logFile.Count.Should().Be(21);
				logFile.DebugCount.Should().Be(1);
				logFile.InfoCount.Should().Be(2);
				logFile.WarningCount.Should().Be(3);
				logFile.ErrorCount.Should().Be(4);
				logFile.FatalCount.Should().Be(5);
				logFile.OtherCount.Should().Be(6);
			}
		}

		[Test]
		[Description("Verifies that the level of a log line is unambigously defined")]
		public void TestLevelPrecedence()
		{
			using (var logFile = new LogFile(@"TestData\DifferentLevels.txt"))
			{
				logFile.Start();
				logFile.Wait();

				logFile.Count.Should().Be(6);
				var lines = logFile.GetSection(new LogFileSection(0, 6));
				lines[0].Message.Should().Be("DEBUG ERROR WARN FATAL INFO");
				lines[0].Level.Should().Be(LevelFlags.Debug, "Because DEBUG is the first level to appear in the line");

				lines[1].Message.Should().Be("INFO DEBUG ERROR WARN FATAL");
				lines[1].Level.Should().Be(LevelFlags.Info, "Because INFO is the first level to appear in the line");

				lines[2].Message.Should().Be("WARN ERROR FATAL INFO DEBUG");
				lines[2].Level.Should().Be(LevelFlags.Warning, "Because WARN is the first level to appear in the line");

				lines[3].Message.Should().Be("ERROR INFO DEBUG FATAL WARN");
				lines[3].Level.Should().Be(LevelFlags.Error, "Because ERROR is the first level to appear in the line");

				lines[4].Message.Should().Be("FATAL ERROR INFO WARN DEBUG");
				lines[4].Level.Should().Be(LevelFlags.Fatal, "Because FATAL is the first level to appear in the line");

				lines[5].Message.Should().Be("fatal error info warn debug");
				lines[5].Level.Should().Be(LevelFlags.None, "Because no level is mentioned in the line");
				lines[5].LogEntryIndex.Should().Be(lines[4].LogEntryIndex);

				logFile.DebugCount.Should().Be(1);
				logFile.InfoCount.Should().Be(1);
				logFile.WarningCount.Should().Be(1);
				logFile.ErrorCount.Should().Be(1);
				logFile.FatalCount.Should().Be(1);
				logFile.OtherCount.Should().Be(1);
			}
		}
	}
}