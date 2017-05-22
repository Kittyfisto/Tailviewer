using System;
using System.IO;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.MainPanel;
using Tailviewer.Ui.ViewModels;
using ApplicationSettings = Tailviewer.Settings.ApplicationSettings;

namespace Tailviewer.Test.Ui.Controls.MainPanel
{
	[TestFixture]
	public sealed class LogViewMainPanelViewModelTest
	{
		private Mock<IActionCenter> _actionCenter;
		private Mock<IApplicationSettings> _settings;

		[SetUp]
		public void Setup()
		{
			_actionCenter = new Mock<IActionCenter>();
			_settings = new Mock<IApplicationSettings>();
		}

		[Test]
		public void TestUpdate1()
		{
			var model = new LogViewMainPanelViewModel(_actionCenter.Object, _settings.Object);
			new Action(() => model.Update()).ShouldNotThrow();
		}

		[Test]
		public void TestUpdate2()
		{
			var model = new LogViewMainPanelViewModel(_actionCenter.Object, _settings.Object);
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