using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core.Analysis;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.MainPanel.Analyse;
using Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels;

namespace Tailviewer.Test.Ui.Controls.Analyse
{
	[TestFixture]
	public sealed class AnalyseMainPanelViewModelTest
	{
		private Mock<IApplicationSettings> _settings;
		private Mock<IMainWindowSettings> _mainWindow;
		private Mock<IDataSources> _dataSources;
		private ManualDispatcher _dispatcher;
		private ManualTaskScheduler _taskScheduler;
		private Mock<IAnalysisStorage> _analysisStorage;

		[SetUp]
		public void Setup()
		{
			_settings = new Mock<IApplicationSettings>();
			_mainWindow = new Mock<IMainWindowSettings>();
			_settings.Setup(x => x.MainWindow).Returns(_mainWindow.Object);

			_dataSources = new Mock<IDataSources>();
			_dispatcher = new ManualDispatcher();
			_taskScheduler = new ManualTaskScheduler();
			_analysisStorage = new Mock<IAnalysisStorage>();
		}

		[Test]
		[Description("Verifies that the view model doesn't select any panel if none was selected in the settings")]
		public void TestCtor1()
		{
			_mainWindow.Setup(x => x.SelectedSidePanel).Returns((string)null);

			var viewModel = new AnalyseMainPanelViewModel(_settings.Object,
				_dataSources.Object,
				_dispatcher,
				_taskScheduler,
				_analysisStorage.Object);

			const string reason = "because the settings don't have a panel selected and thus the view model should neither";
			viewModel.SelectedSidePanel.Should().BeNull(reason);
		}

		[Test]
		[Description("Verifies that the view model tolerates not finding a matching side panel (and displays none)")]
		public void TestCtor2()
		{
			_mainWindow.Setup(x => x.SelectedSidePanel).Returns("foobar");

			var viewModel = new AnalyseMainPanelViewModel(_settings.Object,
				_dataSources.Object,
				_dispatcher,
				_taskScheduler,
				_analysisStorage.Object);

			const string reason = "because there is no such panel and thus no panel should've been selected";
			viewModel.SelectedSidePanel.Should().BeNull(reason);
		}

		[Test]
		[Description("Verifies that the current side panel is restored from the configuration")]
		public void TestCtor3()
		{
			_mainWindow.Setup(x => x.SelectedSidePanel).Returns(WidgetsSidePanel.PanelId);

			var viewModel = new AnalyseMainPanelViewModel(_settings.Object,
				_dataSources.Object,
				_dispatcher,
				_taskScheduler,
				_analysisStorage.Object);

			const string reason = "because the widgets side panel should've been selected";
			viewModel.SelectedSidePanel.Should().NotBeNull(reason);
			viewModel.SelectedSidePanel.Should().BeOfType<WidgetsSidePanel>(reason);
		}

		[Test]
		[Description("Verifies that the window title changes when the current analyses name does")]
		public void TestChangeName()
		{
			var viewModel = new AnalyseMainPanelViewModel(_settings.Object,
			                                              _dataSources.Object,
			                                              _dispatcher,
			                                              _taskScheduler,
			                                              _analysisStorage.Object);
			var analysis = new AnalysisViewModel(_dispatcher, new AnalysisViewTemplate(), new Mock<IAnalysis>().Object,
			                                     new Mock<IAnalysisStorage>().Object)
			{
				Name = "Foo"
			};
			viewModel.Analysis = analysis;
			viewModel.WindowTitle.Should().EndWith("Foo");

			analysis.Name = "Foobar";
			viewModel.WindowTitle.Should().EndWith("Foobar");
		}
	}
}