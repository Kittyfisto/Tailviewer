using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.AcceptanceTests.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.AcceptanceTests.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class SingleDataSourceTest
	{
		[SetUp]
		public void SetUp()
		{
			_scheduler = new DefaultTaskScheduler();
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
			_scheduler.Dispose();
		}

		private DataSource _settings;
		private SingleDataSource _dataSource;
		private DefaultTaskScheduler _scheduler;

		[Test]
		public void TestCtor()
		{
			_dataSource.FilteredLogFile.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue(TimeSpan.FromSeconds(5));

			_dataSource.UnfilteredLogFile.Should().NotBeNull();
			_dataSource.FilteredLogFile.Should().NotBeNull();

			_dataSource.UnfilteredLogFile.Count.Should().Be(165342);
			_dataSource.FilteredLogFile.Count.Should().Be(165342);
		}

		[Test]
		public void TestLevleFilter1()
		{
			_dataSource.LevelFilter = LevelFlags.Info;
			_dataSource.FilteredLogFile.Should().NotBeNull();
			_dataSource.FilteredLogFile.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue(TimeSpan.FromSeconds(5));

			_dataSource.FilteredLogFile.Count.Should().Be(5);
		}

		[Test]
		public void TestStringFilter1()
		{
			_dataSource.UnfilteredLogFile.Property(x => x.EndOfSourceReached)
					   .ShouldEventually().BeTrue();

			_dataSource.QuickFilterChain = new[] {new SubstringFilter("info", true)};
			_dataSource.FilteredLogFile.Should().NotBeNull();

			_dataSource.FilteredLogFile.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue(TimeSpan.FromSeconds(5));

			_dataSource.FilteredLogFile.Count.Should().Be(5);
		}

		[Test]
		[Description("Verifies that the levels are counted correctly")]
		public void TestLevelCount1()
		{
			_dataSource.FilteredLogFile.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue(TimeSpan.FromSeconds(5));

			_dataSource.TotalCount.Should().Be(165342);
			_dataSource.DebugCount.Should().Be(165337);
			_dataSource.InfoCount.Should().Be(5);
			_dataSource.WarningCount.Should().Be(0);
			_dataSource.ErrorCount.Should().Be(0);
			_dataSource.FatalCount.Should().Be(0);
		}
	}
}