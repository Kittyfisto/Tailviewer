using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Settings;
using Tailviewer.Test;

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
			_logFileFactory = new SimplePluginLogFileFactory(_scheduler);
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
				dataSource.FilteredLogSource.Property(x =>
				{
					_scheduler.RunOnce();
					return x.GetProperty(GeneralProperties.LogEntryCount) >= 6;
				}).ShouldEventually().BeTrue();
				dataSource.FilteredLogSource.GetProperty(GeneralProperties.LogEntryCount).Should().Be(6, "because the file consists of 6 lines");

				var entries = dataSource.FilteredLogSource.GetEntries(new LogFileSection(0, 6));
				entries[0].RawContent.Should().Be("DEBUG ERROR WARN FATAL INFO");
				entries[0].LogLevel.Should().Be(LevelFlags.Debug, "Because DEBUG is the first level to appear in the line");

				entries[1].RawContent.Should().Be("INFO DEBUG ERROR WARN FATAL");
				entries[1].LogLevel.Should().Be(LevelFlags.Info, "Because INFO is the first level to appear in the line");

				entries[2].RawContent.Should().Be("WARN ERROR FATAL INFO DEBUG");
				entries[2].LogLevel.Should().Be(LevelFlags.Warning, "Because WARN is the first level to appear in the line");

				entries[3].RawContent.Should().Be("ERROR INFO DEBUG FATAL WARN");
				entries[3].LogLevel.Should().Be(LevelFlags.Error, "Because ERROR is the first level to appear in the line");

				entries[4].RawContent.Should().Be("FATAL ERROR INFO WARN DEBUG");
				entries[4].LogLevel.Should().Be(LevelFlags.Fatal, "Because FATAL is the first level to appear in the line");

				entries[5].RawContent.Should().Be("fatal error info warn debug");
				entries[5].LogLevel.Should()
						.Be(LevelFlags.Fatal,
							"Because this line belongs to the previous log entry and thus is marked as fatal as well");
				entries[5].LogEntryIndex.Should().Be(entries[4].LogEntryIndex);

				_scheduler.RunOnce();

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
