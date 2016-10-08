using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Scheduling;
using Tailviewer.Settings;
using Tailviewer.Test.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class SingleDataSourceAcceptanceTest
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_scheduler = new TaskScheduler();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_scheduler.Dispose();
		}

		[SetUp]
		public void SetUp()
		{
			_settings = new DataSource(LogFileTest.File20Mb)
				{
					Id = Guid.NewGuid()
				};
			_dataSource = new SingleDataSource(_scheduler, _settings, TimeSpan.FromMilliseconds(100));
		}

		[TearDown]
		public void TearDown()
		{
			_dataSource.Dispose();
		}

		private DataSource _settings;
		private SingleDataSource _dataSource;
		private TaskScheduler _scheduler;

		[Test]
		public void TestCtor()
		{
			_dataSource.UnfilteredLogFile.Should().NotBeNull();
			_dataSource.FilteredLogFile.Should().NotBeNull();

			_dataSource.FilteredLogFile.Wait();

			Thread.Sleep(TimeSpan.FromSeconds(1));

			_dataSource.UnfilteredLogFile.Count.Should().Be(165342);
			_dataSource.FilteredLogFile.Count.Should().Be(165342);
		}

		[Test]
		public void TestLevleFilter1()
		{
			_dataSource.UnfilteredLogFile.Wait();

			_dataSource.LevelFilter = LevelFlags.Info;
			_dataSource.FilteredLogFile.Should().NotBeNull();
			_dataSource.FilteredLogFile.Wait();
			_dataSource.FilteredLogFile.Count.Should().Be(5);
		}

		[Test]
		public void TestStringFilter1()
		{
			_dataSource.UnfilteredLogFile.Wait();

			_dataSource.QuickFilterChain = new[] {new SubstringFilter("info", true)};
			_dataSource.FilteredLogFile.Should().NotBeNull();
			_dataSource.FilteredLogFile.Wait();
			_dataSource.FilteredLogFile.Count.Should().Be(5);
		}
	}
}