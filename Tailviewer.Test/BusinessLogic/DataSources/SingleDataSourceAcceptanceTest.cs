using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Test.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class SingleDataSourceAcceptanceTest
	{
		[SetUp]
		public void SetUp()
		{
			_settings = new DataSource(LogFileTest.File20Mb)
				{
					Id = Guid.NewGuid()
				};
			_dataSource = new SingleDataSource(_settings, TimeSpan.FromMilliseconds(100));
		}

		[TearDown]
		public void TearDown()
		{
			_dataSource.Dispose();
		}

		private DataSource _settings;
		private SingleDataSource _dataSource;

		[Test]
		public void TestCtor()
		{
			_dataSource.LogFile.Should().NotBeNull();
			_dataSource.FilteredLogFile.Should().NotBeNull();

			_dataSource.FilteredLogFile.Wait();

			Thread.Sleep(TimeSpan.FromSeconds(1));

			_dataSource.LogFile.Count.Should().Be(165342);
			_dataSource.FilteredLogFile.Count.Should().Be(165342);
		}

		[Test]
		public void TestLevleFilter1()
		{
			_dataSource.LogFile.Wait();

			_dataSource.LevelFilter = LevelFlags.Info;
			_dataSource.FilteredLogFile.Should().NotBeNull();
			_dataSource.FilteredLogFile.Wait();
			_dataSource.FilteredLogFile.Count.Should().Be(5);
		}

		[Test]
		public void TestStringFilter1()
		{
			_dataSource.LogFile.Wait();

			_dataSource.QuickFilterChain = new[] {new SubstringFilter("info", true)};
			_dataSource.FilteredLogFile.Should().NotBeNull();
			_dataSource.FilteredLogFile.Wait();
			_dataSource.FilteredLogFile.Count.Should().Be(5);
		}
	}
}