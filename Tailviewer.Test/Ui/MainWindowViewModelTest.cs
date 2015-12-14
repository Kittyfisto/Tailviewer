using System.Linq;
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
	}
}