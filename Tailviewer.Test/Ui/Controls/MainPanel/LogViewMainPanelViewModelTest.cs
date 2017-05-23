using System;
using System.Collections.Generic;
using System.IO;
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
using ApplicationSettings = Tailviewer.Settings.ApplicationSettings;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;

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
	}
}