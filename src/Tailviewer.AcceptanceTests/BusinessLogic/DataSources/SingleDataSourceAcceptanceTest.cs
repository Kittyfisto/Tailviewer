using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.AcceptanceTests.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class SingleDataSourceAcceptanceTest
	{
		private ManualTaskScheduler _scheduler;
		private PluginLogFileFactory _logFileFactory;

		[SetUp]
		public void SetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new PluginLogFileFactory(_scheduler);
		}

		[Test]
		[Repeat(100)]
		[Description("Verifies that the level of a log line is unambigously defined")]
		public void TestLevelPrecedence()
		{
			var settings = new DataSource(@"TestData\DifferentLevels.txt")
			{
				Id = DataSourceId.CreateNew()
			};
			using (var dataSource = new SingleDataSource(_logFileFactory, _scheduler, settings))
			{
				dataSource.FilteredLogFile.Property(x =>
				{
					_scheduler.RunOnce();
					return x.Count >= 6;
				}).ShouldEventually().BeTrue();
				dataSource.FilteredLogFile.Count.Should().Be(6, "because the file consists of 6 lines");

				LogLine[] lines = dataSource.FilteredLogFile.GetSection(new LogFileSection(0, 6));
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
				lines[5].Level.Should()
						.Be(LevelFlags.Fatal,
							"Because this line belongs to the previous log entry and thus is marked as fatal as well");
				lines[5].LogEntryIndex.Should().Be(lines[4].LogEntryIndex);

				dataSource.DebugCount.Should().Be(1);
				dataSource.InfoCount.Should().Be(1);
				dataSource.WarningCount.Should().Be(1);
				dataSource.ErrorCount.Should().Be(1);
				dataSource.FatalCount.Should().Be(1);
				dataSource.NoLevelCount.Should().Be(0);
			}
		}
	}
}
