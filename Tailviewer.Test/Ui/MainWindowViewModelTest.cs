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
			_settings = new ApplicationSettings();
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
		public void TestChangeDataSource()
		{
			_mainWindow.CurrentDataSource.Should().BeNull();

			var filter = _mainWindow.AddQuickFilter();
			var dataSource = _mainWindow.OpenFile("Foobar");
			_mainWindow.CurrentDataSource.Should().BeSameAs(dataSource);
			filter.CurrentDataSource.Should().BeSameAs(dataSource.DataSource, "Because now that said data source is visible, the filter should be applied to it");
		}
	}
}