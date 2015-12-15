using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;
using DataSources = Tailviewer.BusinessLogic.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.QuickFilters;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class MainWindowViewModelTest
	{
		private MainWindowViewModel _mainWindow;
		private ManualDispatcher _dispatcher;
		private DataSources _dataSources;
		private QuickFilters _quickFilters;
		private ApplicationSettings _settings;

		[SetUp]
		public void SetUp()
		{
			_settings = new ApplicationSettings("adwad");
			_dispatcher = new ManualDispatcher();
			_dataSources = new DataSources(_settings.DataSources);
			_quickFilters = new QuickFilters(_settings.QuickFilters);
			_mainWindow = new MainWindowViewModel(_settings, _dataSources, _quickFilters, _dispatcher);
		}

		[TearDown]
		public void TearDown()
		{
			_dataSources.Dispose();
		}

		[Test]
		public void TestChangeDataSource1()
		{
			_mainWindow.CurrentDataSource.Should().BeNull();

			var filter = _mainWindow.AddQuickFilter();
			filter.Value = "test";

			var dataSource = _mainWindow.OpenFile("Foobar");
			_mainWindow.CurrentDataSource.Should().BeSameAs(dataSource);
			filter.CurrentDataSource.Should().BeSameAs(dataSource.DataSource, "Because now that said data source is visible, the filter should be applied to it");
			_mainWindow.CurrentDataSourceLogView.Should().NotBeNull();
			_mainWindow.CurrentDataSourceLogView.QuickFilterChain.Should()
			           .BeNull("Because no quick filters have been added / nor activated");
		}

		[Test]
		public void TestChangeDataSource2()
		{
			_mainWindow.CurrentDataSource.Should().BeNull();

			var filter = _mainWindow.AddQuickFilter();
			filter.Value = "test";

			var dataSource1 = _mainWindow.OpenFile("foo");
			var dataSource2 = _mainWindow.OpenFile("bar");
			_mainWindow.CurrentDataSource.Should().NotBeNull();
			_mainWindow.CurrentDataSource.Should().BeSameAs(dataSource2);

			dataSource1.DataSource.ActivateQuickFilter(filter.Id);
			dataSource2.DataSource.ActivateQuickFilter(filter.Id);

			_mainWindow.CurrentDataSource = dataSource1;
			_mainWindow.CurrentDataSourceLogView.Should().NotBeNull();
			_mainWindow.CurrentDataSourceLogView.QuickFilterChain.Should().NotBeNull();
		}

		[Test]
		public void TestNextDataSource()
		{
			var dataSource1 = _mainWindow.OpenFile("foo");
			var dataSource2 = _mainWindow.OpenFile("bar");
			_mainWindow.CurrentDataSource = null;
			new Action(() => _mainWindow.SelectNextDataSourceCommand.Execute(null)).ShouldNotThrow();
			_mainWindow.CurrentDataSource.Should().BeSameAs(dataSource1, "Because when no data source is selected, the first should be when navigating forward");
			new Action(() => _mainWindow.SelectNextDataSourceCommand.Execute(null)).ShouldNotThrow();
			_mainWindow.CurrentDataSource.Should().BeSameAs(dataSource2, "Because obvious");
			new Action(() => _mainWindow.SelectNextDataSourceCommand.Execute(null)).ShouldNotThrow();
			_mainWindow.CurrentDataSource.Should().BeSameAs(dataSource1, "Because selecting the next data source when the last data source is, should simply roundtrip to the first datasource again");
		}

		[Test]
		public void TestPreviousDataSource()
		{
			var dataSource1 = _mainWindow.OpenFile("foo");
			var dataSource2 = _mainWindow.OpenFile("bar");
			_mainWindow.CurrentDataSource = null;
			new Action(() => _mainWindow.SelectPreviousDataSourceCommand.Execute(null)).ShouldNotThrow();
			_mainWindow.CurrentDataSource.Should().BeSameAs(dataSource2, "Because when no data source is selected, the last should be when navigating backwards");
			new Action(() => _mainWindow.SelectPreviousDataSourceCommand.Execute(null)).ShouldNotThrow();
			_mainWindow.CurrentDataSource.Should().BeSameAs(dataSource1);
			new Action(() => _mainWindow.SelectPreviousDataSourceCommand.Execute(null)).ShouldNotThrow();
			_mainWindow.CurrentDataSource.Should().BeSameAs(dataSource2, "Because selecting the previous data source when the first data source is, should simply roundtrip to the last data source again");
		}
	}
}