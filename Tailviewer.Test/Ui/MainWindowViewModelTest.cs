using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.Core.LogFiles;
using Tailviewer.Ui.Controls.DataSourceTree;
using Tailviewer.Ui.Controls.MainPanel;
using Tailviewer.Ui.ViewModels;
using ApplicationSettings = Tailviewer.Settings.ApplicationSettings;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class MainWindowViewModelTest
	{
		[SetUp]
		public void SetUp()
		{
			_settings = new ApplicationSettings("adwad");
			_dispatcher = new ManualDispatcher();
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new PluginLogFileFactory(_scheduler);
			_dataSources = new DataSources(_logFileFactory, _scheduler, _settings.DataSources);
			_quickFilters = new QuickFilters(_settings.QuickFilters);
			_actionCenter = new ActionCenter();
			_updater = new Mock<IAutoUpdater>();
			_mainWindow = new MainWindowViewModel(_settings,
			                                      _dataSources,
			                                      _quickFilters,
			                                      _actionCenter,
			                                      _updater.Object,
			                                      _dispatcher,
			                                      Enumerable.Empty<IPluginDescription>());
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

		[Test]
		[Defect("https://github.com/Kittyfisto/Tailviewer/issues/76")]
		public void TestCtor()
		{
			var dataSource = _dataSources.AddDataSource(@"F:\logs\foo.log");
			_settings.DataSources.SelectedItem = dataSource.Id;

			_mainWindow = new MainWindowViewModel(_settings,
				_dataSources,
				_quickFilters,
				_actionCenter,
				_updater.Object,
				_dispatcher,
				Enumerable.Empty<IPluginDescription>());

			_mainWindow.WindowTitle.Should().Be(string.Format(@"{0} - foo.log", Constants.MainWindowTitle));
			_mainWindow.WindowTitleSuffix.Should().Be(@"F:\logs\foo.log");
		}

		[Test]
		[Enhancement("https://github.com/Kittyfisto/Tailviewer/issues/75")]
		public void TestShowLog()
		{
			_dataSources.Should().BeEmpty();
			_mainWindow.ShowLogCommand.Should().NotBeNull();
			_mainWindow.ShowLogCommand.CanExecute(null).Should().BeTrue();
			_mainWindow.ShowLogCommand.Execute(null);

			_dataSources.Should().HaveCount(1);
			var dataSource = _dataSources.First();
			dataSource.FullFileName.Should().Be(Constants.ApplicationLogFile);
		}

		[Test]
		public void TestChangeDataSource1()
		{
			_mainWindow.LogViewPanel.CurrentDataSource.Should().BeNull();
			_mainWindow.SelectedEntry = _mainWindow.Entries.FirstOrDefault(x => x.Id == "raw");

			QuickFilterViewModel filter = _mainWindow.LogViewPanel.AddQuickFilter();
			filter.Value = "test";

			IDataSourceViewModel dataSource = _mainWindow.OpenFile("Foobar");
			_mainWindow.LogViewPanel.CurrentDataSource.Should().BeSameAs(dataSource);
			filter.CurrentDataSource.Should()
			      .BeSameAs(dataSource.DataSource,
			                "Because now that said data source is visible, the filter should be applied to it");

			var panel = (LogViewMainPanelViewModel)_mainWindow.SelectedMainPanel;
			panel.CurrentDataSourceLogView.Should().NotBeNull();
			panel.CurrentDataSourceLogView.QuickFilterChain.Should()
				.BeNull("Because no quick filters have been added / nor activated");
		}

		[Test]
		public void TestChangeDataSource2()
		{
			_mainWindow.LogViewPanel.CurrentDataSource.Should().BeNull();
			_mainWindow.SelectedEntry = _mainWindow.Entries.FirstOrDefault(x => x.Id == "raw");

			QuickFilterViewModel filter = _mainWindow.LogViewPanel.AddQuickFilter();
			filter.Value = "test";

			IDataSourceViewModel dataSource1 = _mainWindow.OpenFile("foo");
			IDataSourceViewModel dataSource2 = _mainWindow.OpenFile("bar");
			_mainWindow.LogViewPanel.CurrentDataSource.Should().NotBeNull();
			_mainWindow.LogViewPanel.CurrentDataSource.Should().BeSameAs(dataSource2);

			dataSource1.DataSource.ActivateQuickFilter(filter.Id);
			dataSource2.DataSource.ActivateQuickFilter(filter.Id);

			_mainWindow.LogViewPanel.CurrentDataSource = dataSource1;
			var panel = (LogViewMainPanelViewModel)_mainWindow.SelectedMainPanel;
			panel.CurrentDataSourceLogView.Should().NotBeNull();
			panel.CurrentDataSourceLogView.QuickFilterChain.Should().NotBeNull();
		}

		[Test]
		[Description(
			"Verifies that the mainwindow synchronizes the currently selected item correctly after having performed a d&d")]
		public void TestGroup1()
		{
			_mainWindow.SelectedEntry = _mainWindow.Entries.FirstOrDefault(x => x.Id == "raw");

			IDataSourceViewModel dataSource1 = _mainWindow.OpenFile("foo");
			IDataSourceViewModel dataSource2 = _mainWindow.OpenFile("bar");
			var mainWindowChanges = new List<string>();
			var logViewChanges = new List<string>();
			_mainWindow.PropertyChanged += (unused, args) => mainWindowChanges.Add(args.PropertyName);
			_mainWindow.LogViewPanel.PropertyChanged += (sender, args) => logViewChanges.Add(args.PropertyName);
			_mainWindow.OnDropped(dataSource1, dataSource2, DataSourceDropType.Group);
			_mainWindow.LogViewPanel.RecentFiles.Count().Should().Be(1);
			IDataSourceViewModel group = _mainWindow.LogViewPanel.RecentFiles.First();
			group.Should().NotBeNull();
			_mainWindow.LogViewPanel.CurrentDataSource.Should().BeSameAs(group);

			var panel = (LogViewMainPanelViewModel)_mainWindow.SelectedMainPanel;
			panel.CurrentDataSourceLogView.DataSource.Should().BeSameAs(group);
			mainWindowChanges.Should().Equal("WindowTitle", "WindowTitleSuffix");
			logViewChanges.Should().Equal("CurrentDataSourceLogView", "CurrentDataSourceLogView", "CurrentDataSource");
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