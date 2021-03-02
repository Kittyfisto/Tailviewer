using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Acceptance.Tests.BusinessLogic.Sources.Text;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;
using Tailviewer.Settings;
using Tailviewer.Tests;

namespace Tailviewer.Acceptance.Tests.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class FileDataSourceRealTest
	{
		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new DefaultTaskScheduler();
			_logSourceFactory = new SimplePluginLogSourceFactory(_taskScheduler);
			_settings = new DataSource(AbstractTextLogSourceAcceptanceTest.File20Mb)
			{
				Id = DataSourceId.CreateNew()
			};
			_dataSource = new FileDataSource(_logSourceFactory, _taskScheduler, _settings, TimeSpan.FromMilliseconds(100));
		}

		[TearDown]
		public void TearDown()
		{
			_dataSource.Dispose();
			_taskScheduler.Dispose();
		}

		private DataSource _settings;
		private FileDataSource _dataSource;
		private DefaultTaskScheduler _taskScheduler;
		private ILogSourceFactory _logSourceFactory;

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