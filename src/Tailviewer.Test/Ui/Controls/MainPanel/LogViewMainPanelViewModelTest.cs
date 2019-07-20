using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.Highlighters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.MainPanel;
using Tailviewer.Ui.Controls.SidePanel.DataSources;
using Tailviewer.Ui.Controls.SidePanel.QuickFilters;
using Tailviewer.Ui.ViewModels;
using QuickFilter = Tailviewer.BusinessLogic.Filters.QuickFilter;

namespace Tailviewer.Test.Ui.Controls.MainPanel
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class LogViewMainPanelViewModelTest
	{
		private ServiceContainer _services;
		private Mock<IActionCenter> _actionCenter;
		private Mock<IApplicationSettings> _settings;
		private Mock<IDataSources> _dataSources;
		private Mock<IQuickFilters> _quickFilters;
		private Mock<IHighlighters> _highlighters;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();
			_services.RegisterInstance<IPluginLoader>(new PluginRegistry());

			_actionCenter = new Mock<IActionCenter>();
			_dataSources = new Mock<IDataSources>();
			_dataSources.Setup(x => x.Sources).Returns(new List<IDataSource>());

			_quickFilters = new Mock<IQuickFilters>();
			_quickFilters.Setup(x => x.AddQuickFilter()).Returns(new QuickFilter(new Core.Settings.QuickFilter()));
			_quickFilters.Setup(x => x.TimeFilter).Returns(new TimeFilter(new Core.Settings.TimeFilter()));

			_settings = new Mock<IApplicationSettings>();
			_settings.Setup(x => x.DataSources).Returns(new Mock<IDataSourcesSettings>().Object);
			_settings.Setup(x => x.MainWindow).Returns(new Mock<IMainWindowSettings>().Object);
			_settings.Setup(x => x.LogViewer).Returns(new Mock<ILogViewerSettings>().Object);

			_highlighters = new Mock<IHighlighters>();
		}

		[Test]
		public void TestConstruction()
		{
			var model = new LogViewMainPanelViewModel(_services, _actionCenter.Object, _dataSources.Object, _quickFilters.Object, _highlighters.Object, _settings.Object);
			model.Settings.Should().BeSameAs(_settings.Object.LogViewer);
		}

		[Test]
		public void TestUpdate1()
		{
			var model = new LogViewMainPanelViewModel(_services, _actionCenter.Object, _dataSources.Object, _quickFilters.Object, _highlighters.Object, _settings.Object);
			new Action(() => model.Update()).Should().NotThrow();
		}

		[Test]
		public void TestUpdate2()
		{
			var model = new LogViewMainPanelViewModel(_services, _actionCenter.Object, _dataSources.Object, _quickFilters.Object, _highlighters.Object, _settings.Object);
			var dataSourceViewModel = new Mock<IDataSourceViewModel>();
			var dataSource = new Mock<IDataSource>();
			var logFile = new InMemoryLogFile();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile);
			var filteredLogFile = new InMemoryLogFile();
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile);
			dataSourceViewModel.Setup(x => x.DataSource).Returns(dataSource.Object);
			model.CurrentDataSource = dataSourceViewModel.Object;

			logFile.AddEntry("", LevelFlags.All);
			logFile.SetValue(LogFileProperties.Size, Size.OneByte);
			model.Update();
			model.CurrentDataSourceLogView.NoEntriesExplanation.Should().Be("Not a single log entry matches the level selection");
		}

		[Test]
		[NUnit.Framework.Description("Verifies that update retrieves certain changed values from ALL data sources, even if they aren't the selected one")]
		public void TestUpdate3()
		{
			var dataSource1 = new Mock<ISingleDataSource>();
			dataSource1.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			dataSource1.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);
			var dataSource2 = new Mock<ISingleDataSource>();
			dataSource2.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			dataSource2.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);

			_dataSources.Setup(x => x.Sources).Returns(new List<IDataSource> {dataSource1.Object, dataSource2.Object});
			var model = new LogViewMainPanelViewModel(_services, _actionCenter.Object, _dataSources.Object, _quickFilters.Object, _highlighters.Object, _settings.Object);
			model.RecentFiles.Should().HaveCount(2);

			dataSource1.Setup(x => x.NoTimestampCount).Returns(42);
			dataSource2.Setup(x => x.NoTimestampCount).Returns(9001);

			model.Update();
			var viewModel1 = model.RecentFiles.First();
			viewModel1.NoTimestampCount.Should().Be(42);
			var viewModel2 = model.RecentFiles.Last();
			viewModel2.NoTimestampCount.Should().Be(9001);
		}

		[Test]
		[NUnit.Framework.Description("Verifies that changing an active filter is automatically applied to the currently selected data source")]
		public void TestChangeFilter1()
		{
			var model = new LogViewMainPanelViewModel(_services, _actionCenter.Object, _dataSources.Object, _quickFilters.Object, _highlighters.Object, _settings.Object);
			var dataSourceViewModel = new Mock<IDataSourceViewModel>();
			dataSourceViewModel.SetupProperty(x => x.QuickFilterChain);

			var dataSource = CreateDataSource();

			var logFile = new InMemoryLogFile();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile);
			var filteredLogFile = new InMemoryLogFile();
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile);
			dataSourceViewModel.Setup(x => x.DataSource).Returns(dataSource.Object);
			model.CurrentDataSource = dataSourceViewModel.Object;

			dataSourceViewModel.Object.QuickFilterChain.Should().BeNull("because no filter should be set right now");

			var filter = model.AddQuickFilter();
			filter.Value = "Foobar";
			filter.IsActive = true;

			dataSourceViewModel.Object.QuickFilterChain.Should()
				.NotBeNull("because a filter chain should've been created for the 'Foobar' filter");
		}

		[Test]
		public void TestShowQuickFilters()
		{
			var model = new LogViewMainPanelViewModel(_services, _actionCenter.Object, _dataSources.Object, _quickFilters.Object, _highlighters.Object, _settings.Object);
			var quickFilterSidePanel = model.SidePanels.OfType<QuickFiltersSidePanelViewModel>().First();
			model.SelectedSidePanel.Should().NotBe(quickFilterSidePanel);

			model.ShowQuickFilters();
			model.SelectedSidePanel.Should().Be(quickFilterSidePanel);
		}

		[Test]
		public void TestRenameMergedDataSource()
		{
			var model = new LogViewMainPanelViewModel(_services, _actionCenter.Object, _dataSources.Object, _quickFilters.Object, _highlighters.Object, _settings.Object);

			var dataSourceViewModel = new Mock<IDataSourceViewModel>();
			var dataSource = new Mock<IDataSource>();
			dataSource.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);
			dataSourceViewModel.Setup(x => x.DataSource).Returns(dataSource.Object);
			dataSourceViewModel.Setup(x => x.DisplayName).Returns("Merged Data Source");
			dataSourceViewModel.Setup(x => x.DataSourceOrigin).Returns("Merged Data Source");

			model.CurrentDataSource = dataSourceViewModel.Object;
			model.WindowTitle.Should().Be(string.Format("{0} - Merged Data Source", Constants.MainWindowTitle));
			model.WindowTitleSuffix.Should().Be("Merged Data Source");

			dataSourceViewModel.Setup(x => x.DisplayName).Returns("My custom name");
			dataSourceViewModel.Raise(x => x.PropertyChanged += null, new PropertyChangedEventArgs("DisplayName"));
			dataSourceViewModel.Setup(x => x.DataSourceOrigin).Returns("My custom name");
			dataSourceViewModel.Raise(x => x.PropertyChanged += null, new PropertyChangedEventArgs("DataSourceOrigin"));

			model.WindowTitle.Should().Be(string.Format("{0} - My custom name", Constants.MainWindowTitle));
			model.WindowTitleSuffix.Should().Be("My custom name");
		}

		[Test]
		public void TestGoToNextDataSource1()
		{
			var model = new LogViewMainPanelViewModel(_services, _actionCenter.Object, _dataSources.Object, _quickFilters.Object, _highlighters.Object, _settings.Object);
			model.CurrentDataSource.Should().BeNull();
			new Action(() => model.GoToNextDataSource()).Should().NotThrow();
			model.CurrentDataSource.Should().BeNull();
		}

		[Test]
		public void TestGoToPreviousDataSource1()
		{
			var model = new LogViewMainPanelViewModel(_services, _actionCenter.Object, _dataSources.Object, _quickFilters.Object, _highlighters.Object, _settings.Object);
			model.CurrentDataSource.Should().BeNull();
			new Action(() => model.GoToPreviousDataSource()).Should().NotThrow();
			model.CurrentDataSource.Should().BeNull();
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/127")]
		public void TestSelectDataSourcePartOfMergedDataSource()
		{
			var mergedDataSource = new Mock<IMergedDataSource>();
			mergedDataSource.Setup(x => x.Id).Returns(DataSourceId.CreateNew());
			mergedDataSource.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			mergedDataSource.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);
			mergedDataSource.Setup(x => x.Settings).Returns(new DataSource());
			mergedDataSource.Setup(x => x.DisplayName).Returns("My custom merged data source");

			var dataSource1 = new Mock<ISingleDataSource>();
			dataSource1.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			dataSource1.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);
			dataSource1.Setup(x => x.Id).Returns(DataSourceId.CreateNew());
			dataSource1.Setup(x => x.ParentId).Returns(mergedDataSource.Object.Id);
			dataSource1.Setup(x => x.Settings).Returns(new DataSource());
			dataSource1.Setup(x => x.FullFileName).Returns("log1.txt");
			dataSource1.SetupProperty(x => x.CharacterCode);

			var dataSource2 = new Mock<ISingleDataSource>();
			dataSource2.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			dataSource2.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);
			dataSource2.Setup(x => x.Id).Returns(DataSourceId.CreateNew());
			dataSource2.Setup(x => x.Settings).Returns(new DataSource());
			dataSource2.Setup(x => x.FullFileName).Returns("log2.csv");
			dataSource2.SetupProperty(x => x.CharacterCode);

			_dataSources.Setup(x => x.Sources).Returns(new List<IDataSource> {mergedDataSource.Object, dataSource1.Object, dataSource2.Object});
			var model = new LogViewMainPanelViewModel(_services, _actionCenter.Object, _dataSources.Object, _quickFilters.Object, _highlighters.Object, _settings.Object);
			var dataSources = model.SidePanels.OfType<DataSourcesViewModel>().First();

			var dataSource1ViewModel = dataSources.DataSources.First(x => x.DataSource == dataSource1.Object);
			model.CurrentDataSource = dataSource1ViewModel;
			model.WindowTitle.Should().Be($"{Constants.MainWindowTitle} - log1.txt");
			model.WindowTitleSuffix.Should().Be("My custom merged data source -> [A] log1.txt",
			                                    "because the titlebar shall mention the parent's name, if available as well as the character code of the selected data source");

			var dataSource2ViewModel = dataSources.DataSources.First(x => x.DataSource == dataSource2.Object);
			model.CurrentDataSource = dataSource2ViewModel;
			model.WindowTitle.Should().Be($"{Constants.MainWindowTitle} - log2.csv");
			model.WindowTitleSuffix.Should().Be("log2.csv");

			var mergedViewModel = dataSources.DataSources.First(x => x.DataSource == mergedDataSource.Object);
			model.CurrentDataSource = mergedViewModel;
			model.WindowTitle.Should().Be($"{Constants.MainWindowTitle} - My custom merged data source");
			model.WindowTitleSuffix.Should().Be("My custom merged data source");
		}

		[Test]
		public void TestAddFolder()
		{
			var path = Path.GetTempPath();
			var source = new Mock<IFolderDataSource>();
			source.Setup(x => x.OriginalSources).Returns(new IDataSource[0]);
			source.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			source.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);
			_dataSources.Setup(x => x.AddFolder(It.IsAny<string>())).Returns(source.Object);

			var model = new LogViewMainPanelViewModel(_services, _actionCenter.Object, _dataSources.Object, _quickFilters.Object, _highlighters.Object, _settings.Object);
			var dataSource = model.GetOrAddPath(path);
			dataSource.Should().BeOfType<FolderDataSourceViewModel>();
		}

		private Mock<IDataSourceViewModel> CreateDataSourceViewModel()
		{
			var dataSource = CreateDataSource();
			var viewModel = new Mock<IDataSourceViewModel>();
			viewModel.Setup(x => x.DataSource).Returns(dataSource.Object);
			return viewModel;
		}

		private Mock<IDataSource> CreateDataSource()
		{
			var dataSource = new Mock<IDataSource>();
			var activeFilters = new HashSet<QuickFilterId>();
			dataSource.Setup(x => x.ActivateQuickFilter(It.IsAny<QuickFilterId>()))
				.Callback(
					(QuickFilterId id) => { activeFilters.Add(id); });
			dataSource.Setup(x => x.DeactivateQuickFilter(It.IsAny<QuickFilterId>()))
				.Returns((QuickFilterId id) => activeFilters.Remove(id));
			dataSource.Setup(x => x.IsQuickFilterActive(It.IsAny<QuickFilterId>()))
				.Returns((QuickFilterId id) => activeFilters.Contains(id));

			var logFile = new InMemoryLogFile();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile);
			dataSource.Setup(x => x.FilteredLogFile).Returns(logFile);
			return dataSource;
		}
	}
}