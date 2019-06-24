using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core;
using Tailviewer.Core.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels.Analyses;

namespace Tailviewer.Test.Ui.Controls.Analyse.SidePanels
{
	[TestFixture]
	public sealed class AnalysesSidePanelTest
	{
		private ManualDispatcher _dispatcher;
		private ManualTaskScheduler _taskScheduler;
		private Mock<IAnalysisStorage> _analysisStorage;
		private List<IAnalysis> _analyses;
		private Dictionary<AnalysisId, ActiveAnalysisConfiguration> _templates;
		private PluginRegistry _pluginRegistry;
		private ServiceContainer _services;

		[SetUp]
		public void Setup()
		{
			_dispatcher = new ManualDispatcher();
			_taskScheduler = new ManualTaskScheduler();

			_analysisStorage = new Mock<IAnalysisStorage>();
			_analyses = new List<IAnalysis>();
			_analysisStorage.Setup(x => x.Analyses).Returns(_analyses);
			_templates = new Dictionary<AnalysisId, ActiveAnalysisConfiguration>();
			_analysisStorage.Setup(x => x.CreateAnalysis(It.IsAny<AnalysisTemplate>(), It.IsAny<AnalysisViewTemplate>()))
				.Returns((AnalysisTemplate templates, AnalysisViewTemplate viewTemplate) => AddAnalysis());
			_pluginRegistry = new PluginRegistry();

			_services = new ServiceContainer();
			_services.RegisterInstance<IDispatcher>(_dispatcher);
			_services.RegisterInstance<ITaskScheduler>(_taskScheduler);
			_services.RegisterInstance<IPluginLoader>(_pluginRegistry);
		}

		private IAnalysis AddAnalysis()
		{
			var analysis = new Mock<IAnalysis>();
			var id = AnalysisId.CreateNew();
			analysis.Setup(x => x.Id).Returns(id);
			_analyses.Add(analysis.Object);
			
			var config = new ActiveAnalysisConfiguration();
			_templates.Add(id, config);

			_analysisStorage.Setup(x => x.TryGetTemplateFor(It.Is<AnalysisId>(y => y == id), out config))
			                .Returns(true);

			return analysis.Object;
		}

		[Test]
		public void TestCreateNew()
		{
			var sidePanel = new AnalysesSidePanel(_services, _analysisStorage.Object);
			sidePanel.HasActiveAnalyses.Should().BeFalse();
			sidePanel.Active.Should().BeEmpty();
			sidePanel.CreateNewAnalysis();
			sidePanel.HasActiveAnalyses.Should().BeTrue();
			sidePanel.Active.Should().HaveCount(1);
		}

		[Test]
		public void TestRemoveAnalysis1()
		{
			var sidePanel = new AnalysesSidePanel(_services, _analysisStorage.Object);
			var viewModel = sidePanel.CreateNewAnalysis();
			sidePanel.Active.Should().HaveCount(1);

			viewModel.RemoveCommand.Execute(null);
			sidePanel.Active.Should().BeEmpty("because we've just removed the only analysis");
		}

		[Test]
		public void TestRemoveAnalysis2()
		{
			var sidePanel = new AnalysesSidePanel(_services, _analysisStorage.Object);
			var viewModel = sidePanel.CreateNewAnalysis();
			sidePanel.Active.Should().HaveCount(1);

			_analysisStorage.Verify(x => x.Remove(It.IsAny<AnalysisId>()), Times.Never);
			viewModel.RemoveCommand.Execute(null);
			_analysisStorage.Verify(x => x.Remove(It.Is<AnalysisId>(y => y == viewModel.Id)), Times.Once,
				"because the view model is responsible for removing the analysis (so it doesn't continue to run in the background)");
		}

		[Test]
		public void TestRemoveAnalysis3()
		{
			AddAnalysis();

			var sidePanel = new AnalysesSidePanel(_services, _analysisStorage.Object);
			sidePanel.Update();

			sidePanel.Active.Should().HaveCount(1);
			var viewModel = sidePanel.Active.First();

			_analysisStorage.Verify(x => x.Remove(It.IsAny<AnalysisId>()), Times.Never);
			viewModel.RemoveCommand.Execute(null);
			_analysisStorage.Verify(x => x.Remove(It.Is<AnalysisId>(y => y == viewModel.Id)), Times.Once,
			                        "because the view model is responsible for removing the analysis (so it doesn't continue to run in the background)");
		}

		[Test]
		public void TestUpdateSelectAnalysis()
		{
			var sidePanel = new AnalysesSidePanel(_services, _analysisStorage.Object);
			sidePanel.SelectedAnalysis.Should().BeNull();

			sidePanel.Update();
			sidePanel.SelectedAnalysis.Should().BeNull();

			AddAnalysis();
			sidePanel.Update();
			sidePanel.SelectedAnalysis.Should().NotBeNull("because now that an analysis has become available, it should be selected as well!");
		}
	}
}