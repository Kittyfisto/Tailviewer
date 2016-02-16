using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class SingleDataSourceAcceptanceTest
	{
		private DataSource _settings;
		private SingleDataSource _dataSource;

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

		[Test]
		public void TestCtor()
		{
			_dataSource.LogFile.Should().NotBeNull();
			_dataSource.FilteredLogFile.Should().NotBeNull();
			_dataSource.FilteredLogFile.Should().BeSameAs(_dataSource.LogFile,
			                                              "Because without a filter being active, the filtered log file should simply be the original log file");

			_dataSource.FilteredLogFile.Wait();
			_dataSource.LogFile.Count.Should().Be(165342);
			_dataSource.FilteredLogFile.Count.Should().Be(165342);
		}

		[Test]
		public void TestStringFilter1()
		{
			_dataSource.LogFile.Wait();

			_dataSource.StringFilter = "info";
			_dataSource.FilteredLogFile.Should().NotBeNull();
			_dataSource.FilteredLogFile.Wait();
			_dataSource.FilteredLogFile.Count.Should().Be(5);
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
	}
}