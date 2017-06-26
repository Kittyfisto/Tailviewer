using System;
using System.Collections.Generic;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.MainPanel;
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
			_dataSources.Setup(x => x.GetEnumerator()).Returns(new List<IDataSource>().GetEnumerator());

			_quickFilters = new Mock<IQuickFilters>();
			_quickFilters.Setup(x => x.Add()).Returns(new QuickFilter(new Tailviewer.Settings.QuickFilter()));

			_settings = new Mock<IApplicationSettings>();
			_settings.Setup(x => x.DataSources).Returns(new Mock<IDataSourcesSettings>().Object);
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
			logFile.FileSize = Size.OneByte;
			model.Update();
			model.CurrentDataSourceLogView.NoEntriesExplanation.Should().Be("Not a single log entry matches the level selection");
		}

		[Test]
		[Description("Verifies that updates are forwarded to the currently selected data source")]
		public void TestUpdate3()
		{
			var model = new LogViewMainPanelViewModel(_actionCenter.Object, _dataSources.Object, _quickFilters.Object, _settings.Object);
			new Action(() => model.Update()).ShouldNotThrow("because we haven't set a data source, yet");

			var dataSource = new Mock<IDataSourceViewModel>();
			dataSource.Setup(x => x.DataSource).Returns(CreateDataSource().Object);
			model.CurrentDataSource = dataSource.Object;
			model.Update();

			dataSource.Verify(x => x.Update(), Times.Once, "because Update() should forward the update to the current data source, if there is any");
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

		private Mock<IDataSource> CreateDataSource()
		{
			var dataSource = new Mock<IDataSource>();
			var activeFilters = new HashSet<Guid>();
			dataSource.Setup(x => x.ActivateQuickFilter(It.IsAny<Guid>()))
				.Callback(
					(Guid id) => { activeFilters.Add(id); });
			dataSource.Setup(x => x.DeactivateQuickFilter(It.IsAny<Guid>()))
				.Returns((Guid id) => activeFilters.Remove(id));
			dataSource.Setup(x => x.IsQuickFilterActive(It.IsAny<Guid>()))
				.Returns((Guid id) => activeFilters.Contains(id));

			var logFile = new InMemoryLogFile();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile);
			dataSource.Setup(x => x.FilteredLogFile).Returns(logFile);
			return dataSource;
		}
	}
}