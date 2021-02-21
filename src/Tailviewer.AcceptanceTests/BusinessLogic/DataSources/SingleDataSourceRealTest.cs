using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core.Filters;
using Tailviewer.Core.Properties;
using Tailviewer.Settings;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class SingleDataSourceRealTest
	{
		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new DefaultTaskScheduler();
			_logFileFactory = new SimplePluginLogFileFactory(_taskScheduler);
			_settings = new DataSource(AbstractTextLogSourceAcceptanceTest.File20Mb)
			{
				Id = DataSourceId.CreateNew()
			};
			_dataSource = new SingleDataSource(_logFileFactory, _taskScheduler, _settings, TimeSpan.FromMilliseconds(100));
		}

		[TearDown]
		public void TearDown()
		{
			_dataSource.Dispose();
			_taskScheduler.Dispose();
		}

		private DataSource _settings;
		private SingleDataSource _dataSource;
		private DefaultTaskScheduler _taskScheduler;
		private ILogFileFactory _logFileFactory;

		[Test]
		public void TestCtor()
		{
			_dataSource.FilteredLogSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(15)).Be(Percentage.HundredPercent);

			_dataSource.UnfilteredLogSource.Should().NotBeNull();
			_dataSource.FilteredLogSource.Should().NotBeNull();

			_dataSource.Property(x => x.UnfilteredLogSource.GetProperty(GeneralProperties.LogEntryCount)).ShouldEventually().Be(165342);
			_dataSource.Property(x => x.FilteredLogSource.GetProperty(GeneralProperties.LogEntryCount)).ShouldEventually().Be(165342);
		}

		[Test]
		[Ignore("I've slowed down filtering with the new streaming implementation, needs to be fixed eventually")]
		public void TestLevelFilter1()
		{
			_dataSource.LevelFilter = LevelFlags.Info;
			_dataSource.FilteredLogSource.Should().NotBeNull();
			_dataSource.FilteredLogSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(15)).Be(Percentage.HundredPercent);

			// TODO: Find the bug in the EndOfSourceReached implementation!!!!
			Thread.Sleep(1000);

			_dataSource.FilteredLogSource.GetProperty(GeneralProperties.LogEntryCount).Should().Be(5);
		}

		[Test]
		[Ignore("I've slowed down filtering with the new streaming implementation, needs to be fixed eventually")]
		public void TestStringFilter1()
		{
			_dataSource.UnfilteredLogSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed))
					   .ShouldEventually().Be(Percentage.HundredPercent);

			_dataSource.QuickFilterChain = new[] {new SubstringFilter("info", true)};
			_dataSource.FilteredLogSource.Should().NotBeNull();

			_dataSource.FilteredLogSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(15)).Be(Percentage.HundredPercent);

			_dataSource.FilteredLogSource.GetProperty(GeneralProperties.LogEntryCount).Should().Be(5);
		}
	}
}