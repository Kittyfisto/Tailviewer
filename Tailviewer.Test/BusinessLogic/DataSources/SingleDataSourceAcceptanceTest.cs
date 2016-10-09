using System;
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
		: AbstractTest
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

			WaitUntil(() => _dataSource.FilteredLogFile.Count >= 165342, TimeSpan.FromSeconds(5))
				.Should().BeTrue();

			_dataSource.UnfilteredLogFile.Count.Should().Be(165342);
			_dataSource.FilteredLogFile.Count.Should().Be(165342);
		}

		[Test]
		public void TestLevleFilter1()
		{
			_dataSource.UnfilteredLogFile.Property(x => x.EndOfSourceReached)
			           .ShouldEventually().BeTrue();

			_dataSource.LevelFilter = LevelFlags.Info;
			_dataSource.FilteredLogFile.Should().NotBeNull();

			WaitUntil(() => _dataSource.FilteredLogFile.Count >= 5, TimeSpan.FromSeconds(5))
				.Should().BeTrue();

			_dataSource.FilteredLogFile.Count.Should().Be(5);
		}

		[Test]
		public void TestStringFilter1()
		{
			_dataSource.UnfilteredLogFile.Property(x => x.EndOfSourceReached)
					   .ShouldEventually().BeTrue();

			_dataSource.QuickFilterChain = new[] {new SubstringFilter("info", true)};
			_dataSource.FilteredLogFile.Should().NotBeNull();

			WaitUntil(() => _dataSource.FilteredLogFile.Count >= 5, TimeSpan.FromSeconds(5))
				.Should().BeTrue();

			_dataSource.FilteredLogFile.Count.Should().Be(5);
		}
	}
}