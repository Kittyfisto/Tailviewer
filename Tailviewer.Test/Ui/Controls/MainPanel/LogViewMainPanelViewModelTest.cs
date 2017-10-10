using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.MainPanel;
using Tailviewer.Ui.Controls.SidePanel;
using Tailviewer.Ui.ViewModels;
using QuickFilter = Tailviewer.BusinessLogic.Filters.QuickFilter;

namespace Tailviewer.Test.Ui.Controls.MainPanel
{
	[TestFixture]
	public sealed class LogViewMainPanelViewModelTest
	{
		private Mock<IActionCenter> _actionCenter;
		private Mock<IApplicationSettings> _settings;
		private Mock<IDataSources> _dataSources;
		private Mock<IQuickFilters> _quickFilters;

		[SetUp]
		public void Setup()
		{
			_actionCenter = new Mock<IActionCenter>();
			_dataSources = new Mock<IDataSources>();
			_dataSources.Setup(x => x.Sources).Returns(new List<IDataSource>());

			_quickFilters = new Mock<IQuickFilters>();
			_quickFilters.Setup(x => x.Add()).Returns(new QuickFilter(new Core.Settings.QuickFilter()));

			_settings = new Mock<IApplicationSettings>();
			_settings.Setup(x => x.DataSources).Returns(new Mock<IDataSourcesSettings>().Object);
			_settings.Setup(x => x.MainWindow).Returns(new Mock<IMainWindowSettings>().Object);
		}

		[Test]
		public void TestUpdate1()
		{
			var model = new LogViewMainPanelViewModel(_actionCenter.Object, _dataSources.Object, _quickFilters.Object, _settings.Object);
			new Action(() => model.Update()).ShouldNotThrow();
		}

		[Test]
		public void TestUpdate2()
		{
			var model = new LogViewMainPanelViewModel(_actionCenter.Object, _dataSources.Object, _quickFilters.Object, _settings.Object);
			var dataSourceViewModel = new Mock<IDataSourceViewModel>();
			var dataSource = new Mock<IDataSource>();
			var logFile = new InMemoryLogFile();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile);
			var filteredLogFile = new InMemoryLogFile();
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile);
			dataSourceViewModel.Setup(x => x.DataSource).Returns(dataSource.Object);
			model.CurrentDataSource = dataSourceViewModel.Object;

			logFile.AddEntry("", LevelFlags.All);
			logFile.Size = Size.OneByte;
			model.Update();
			model.CurrentDataSourceLogView.NoEntriesExplanation.Should().Be("Not a single log entry matches the level selection");
		}

		[Test]
		[Description("Verifies that update retrieves certain changed values from ALL data sources, even if they aren't the selected one")]
		public void TestUpdate3()
		{
			var dataSource1 = new Mock<ISingleDataSource>();
			dataSource1.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			dataSource1.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);
			var dataSource2 = new Mock<ISingleDataSource>();
			dataSource2.Setup(x => x.FilteredLogFile).Returns(new Mock<ILogFile>().Object);
			dataSource2.Setup(x => x.UnfilteredLogFile).Returns(new Mock<ILogFile>().Object);

			_dataSources.Setup(x => x.Sources).Returns(new List<IDataSource> {dataSource1.Object, dataSource2.Object});
			var model = new LogViewMainPanelViewModel(_actionCenter.Object, _dataSources.Object, _quickFilters.Object, _settings.Object);
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
		[Description("Verifies that changing an active filter is automatically applied to the currently selected data source")]
		public void TestChangeFilter1()
		{
			var model = new LogViewMainPanelViewModel(_actionCenter.Object, _dataSources.Object, _quickFilters.Object, _settings.Object);
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
			var model = new LogViewMainPanelViewModel(_actionCenter.Object, _dataSources.Object, _quickFilters.Object, _settings.Object);
			var quickFilterSidePanel = model.SidePanels.OfType<QuickFiltersSidePanelViewModel>().First();
			model.SelectedSidePanel.Should().NotBe(quickFilterSidePanel);

			model.ShowQuickFilters();
			model.SelectedSidePanel.Should().Be(quickFilterSidePanel);
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