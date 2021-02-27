using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core;
using Tailviewer.Settings.Bookmarks;
using Tailviewer.Ui;
using Tailviewer.Ui.DataSourceTree;
using Tailviewer.Ui.LogView;
using Tailviewer.Ui.QuickFilter;
using ApplicationSettings = Tailviewer.Settings.ApplicationSettings;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class MainWindowViewModelTest
	{
		[SetUp]
		public void SetUp()
		{
			_settings = new ApplicationSettings("adwad");
			_bookmarks = new Bookmarks("aawdwa");
			_dispatcher = new ManualDispatcher();
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new SimplePluginLogFileFactory(_scheduler);
			_filesystem = new InMemoryFilesystem();
			_dataSources = new DataSources(_logFileFactory, _scheduler, _filesystem, _settings.DataSources, _bookmarks);
			_quickFilters = new QuickFilters(_settings.QuickFilters);
			_actionCenter = new ActionCenter();
			_updater = new Mock<IAutoUpdater>();

			_services = new ServiceContainer();
			_services.RegisterInstance<ITaskScheduler>(_scheduler);
			_services.RegisterInstance<IDispatcher>(_dispatcher);
			_services.RegisterInstance<IPluginLoader>(new PluginRegistry());
			_services.RegisterInstance<IHighlighters>(new HighlighterCollection());
			_services.RegisterInstance<INavigationService>(new NavigationService());

			_mainWindow = new MainWindowViewModel(_services,
			                                      _settings,
			                                      _dataSources,
			                                      _quickFilters,
			                                      _actionCenter,
			                                      _updater.Object);
		}

		[TearDown]
		public void TearDown()
		{
			_dataSources.Dispose();
		}

		private MainWindowViewModel _mainWindow;
		private ManualDispatcher _dispatcher;
		private DataSources _dataSources;
		private QuickFilters _quickFilters;
		private ApplicationSettings _settings;
		private Mock<IAutoUpdater> _updater;
		private ManualTaskScheduler _scheduler;
		private ActionCenter _actionCenter;
		private ILogFileFactory _logFileFactory;
		private Bookmarks _bookmarks;
		private InMemoryFilesystem _filesystem;
		private ServiceContainer _services;

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/76")]
		public void TestCtor()
		{
			var dataSource = _dataSources.AddFile(@"F:\logs\foo.log");
			_settings.DataSources.SelectedItem = dataSource.Id;

			_mainWindow = new MainWindowViewModel(_services,
			                                      _settings,
			                                      _dataSources,
			                                      _quickFilters,
			                                      _actionCenter,
			                                      _updater.Object);

			_mainWindow.WindowTitle.Should().Be(string.Format(@"{0} - foo.log", Constants.MainWindowTitle));
			_mainWindow.WindowTitleSuffix.Should().Be(@"F:\logs\foo.log");
		}

		[Test]
		[Enhancement("https://github.com/Kittyfisto/Tailviewer/issues/75")]
		public void TestShowLog()
		{
			_dataSources.Sources.Should().BeEmpty();
			_mainWindow.ShowLogCommand.Should().NotBeNull();
			_mainWindow.ShowLogCommand.CanExecute(null).Should().BeTrue();
			_mainWindow.ShowLogCommand.Execute(null);

			_dataSources.Sources.Should().HaveCount(1);
			var dataSource = _dataSources.Sources.First();
			dataSource.FullFileName.Should().Be(Constants.ApplicationLogFile);
		}

		[Test]
		public void TestChangeDataSource1()
		{
			_mainWindow.LogViewPanel.CurrentDataSource.Should().BeNull();

			QuickFilterViewModel filter = _mainWindow.LogViewPanel.AddQuickFilter();
			filter.Value = "test";

			IDataSourceViewModel dataSource = _mainWindow.AddFileOrDirectory("Foobar");
			_mainWindow.LogViewPanel.CurrentDataSource.Should().BeSameAs(dataSource);
			filter.CurrentDataSource.Should()
			      .BeSameAs(dataSource.DataSource,
			                "Because now that said data source is visible, the filter should be applied to it");

			var panel = _mainWindow.LogViewPanel;
			panel.CurrentDataSourceLogView.Should().NotBeNull();
			panel.CurrentDataSourceLogView.QuickFilterChain.Should()
				.BeNull("Because no quick filters have been added / nor activated");
		}

		[Test]
		public void TestChangeDataSource2()
		{
			_mainWindow.LogViewPanel.CurrentDataSource.Should().BeNull();

			QuickFilterViewModel filter = _mainWindow.LogViewPanel.AddQuickFilter();
			filter.Value = "test";

			IDataSourceViewModel dataSource1 = _mainWindow.AddFileOrDirectory("foo");
			IDataSourceViewModel dataSource2 = _mainWindow.AddFileOrDirectory("bar");
			_mainWindow.LogViewPanel.CurrentDataSource.Should().NotBeNull();
			_mainWindow.LogViewPanel.CurrentDataSource.Should().BeSameAs(dataSource2);

			dataSource1.DataSource.ActivateQuickFilter(filter.Id);
			dataSource2.DataSource.ActivateQuickFilter(filter.Id);

			_mainWindow.LogViewPanel.CurrentDataSource = dataSource1;
			var panel = _mainWindow.LogViewPanel;
			panel.CurrentDataSourceLogView.Should().NotBeNull();
			panel.CurrentDataSourceLogView.QuickFilterChain.Should().NotBeNull();
		}

		[Test]
		public void TestShowQuickNavigation()
		{
			_mainWindow.LogViewPanel.ShowQuickNavigation.Should().BeFalse();

			_mainWindow.ShowQuickNavigationCommand.Should().NotBeNull();
			_mainWindow.ShowQuickNavigationCommand.CanExecute(null).Should().BeTrue();
			_mainWindow.ShowQuickNavigationCommand.Execute(null);
			_mainWindow.LogViewPanel.ShowQuickNavigation.Should().BeTrue();
		}

		[Test]
		public void TestShowGoToLine()
		{
			_mainWindow.LogViewPanel.GoToLine.Show.Should().BeFalse();

			_mainWindow.ShowGoToLineCommand.Should().NotBeNull();
			_mainWindow.ShowGoToLineCommand.CanExecute(null).Should().BeTrue();
			_mainWindow.ShowGoToLineCommand.Execute(null);
			_mainWindow.LogViewPanel.GoToLine.Show.Should().BeTrue();
		}

		[Test]
		[Description(
			"Verifies that the mainwindow synchronizes the currently selected item correctly after having performed a d&d")]
		public void TestGroup1()
		{
			IDataSourceViewModel dataSource1 = _mainWindow.AddFileOrDirectory("foo");
			IDataSourceViewModel dataSource2 = _mainWindow.AddFileOrDirectory("bar");
			var mainWindowChanges = new List<string>();
			var logViewChanges = new List<string>();
			_mainWindow.PropertyChanged += (unused, args) => mainWindowChanges.Add(args.PropertyName);
			_mainWindow.LogViewPanel.PropertyChanged += (sender, args) => logViewChanges.Add(args.PropertyName);
			_mainWindow.OnDropped(dataSource1, dataSource2, DataSourceDropType.Group);
			_mainWindow.LogViewPanel.RecentFiles.Count().Should().Be(1);
			IDataSourceViewModel group = _mainWindow.LogViewPanel.RecentFiles.First();
			group.Should().NotBeNull();
			_mainWindow.LogViewPanel.CurrentDataSource.Should().BeSameAs(group);

			var panel = _mainWindow.LogViewPanel;
			panel.CurrentDataSourceLogView.DataSource.Should().BeSameAs(group);
			mainWindowChanges.Should().Equal("WindowTitleSuffix", "WindowTitle", "WindowTitleSuffix", "ViewMenuItems");
			logViewChanges.Should().Contain("CurrentDataSourceLogView", "CurrentDataSourceLogView", "CurrentDataSource");
		}
		
		[Test]
		public void TestUpdateAvailable1()
		{
			var changes = new List<string>();
			_mainWindow.AutoUpdater.PropertyChanged += (sender, args) => changes.Add(args.PropertyName);
			_updater.Setup(x => x.AppVersion).Returns(new Version(1, 0, 0));
			_updater.Raise(x => x.LatestVersionChanged += null, new VersionInfo(null, null, new Version(1, 0, 1), null));

			_mainWindow.AutoUpdater.IsUpdateAvailable.Should().BeFalse("Because these changes should be dispatched first");
			_mainWindow.AutoUpdater.ShowUpdateAvailable.Should().BeFalse("Because these changes should be dispatched first");

			_dispatcher.InvokeAll();
			_mainWindow.AutoUpdater.IsUpdateAvailable.Should().BeTrue();
			_mainWindow.AutoUpdater.ShowUpdateAvailable.Should().BeTrue();
			changes.Should().BeEquivalentTo(new object[] { "ShowUpdateAvailable", "IsUpdateAvailable", "LatestVersion" });
		}

		[Test]
		public void TestUpdateAvailable2()
		{
			_updater.Setup(x => x.AppVersion).Returns(new Version(1, 0, 0));
			_updater.Raise(x => x.LatestVersionChanged += null, new VersionInfo(null, null, new Version(1, 0, 1), null));

			_dispatcher.InvokeAll();
			_mainWindow.AutoUpdater.IsUpdateAvailable.Should().BeTrue();
			_mainWindow.AutoUpdater.ShowUpdateAvailable.Should().BeTrue();
			_mainWindow.AutoUpdater.GotItCommand.Execute(null);

			_mainWindow.AutoUpdater.IsUpdateAvailable.Should().BeTrue();
			_mainWindow.AutoUpdater.ShowUpdateAvailable.Should().BeFalse();

			_mainWindow.AutoUpdater.CheckForUpdatesCommand.Execute(null);
			_updater.Raise(x => x.LatestVersionChanged += null, new VersionInfo(null, null, new Version(1, 0, 1), null));
			_dispatcher.InvokeAll();

			_mainWindow.AutoUpdater.IsUpdateAvailable.Should().BeTrue();
			_mainWindow.AutoUpdater.ShowUpdateAvailable.Should().BeTrue();
		}
	}
}