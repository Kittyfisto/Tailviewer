using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
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
		private AnalysesSidePanel _sidePanel;

		[SetUp]
		public void Setup()
		{
			_dispatcher = new ManualDispatcher();
			_taskScheduler = new ManualTaskScheduler();

			_analysisStorage = new Mock<IAnalysisStorage>();
			_analysisStorage.Setup(x => x.CreateAnalysis(It.IsAny<AnalysisTemplate>(), It.IsAny<AnalysisViewTemplate>()))
				.Returns((AnalysisTemplate templates, AnalysisViewTemplate viewTemplate) =>
				{
					var analysis = new Mock<IAnalysis>();
					var id = AnalysisId.CreateNew();
					analysis.Setup(x => x.Id).Returns(id);
					return analysis.Object;
				});

			_sidePanel = new AnalysesSidePanel(_dispatcher, _taskScheduler, _analysisStorage.Object);
		}

		[Test]
		public void TestCreateNew()
		{
			_sidePanel.HasActiveAnalyses.Should().BeFalse();
			_sidePanel.Active.Should().BeEmpty();
			_sidePanel.CreateNew();
			_sidePanel.HasActiveAnalyses.Should().BeTrue();
			_sidePanel.Active.Should().HaveCount(1);
		}

		[Test]
		public void TestRemoveAnalysis1()
		{
			var viewModel = _sidePanel.CreateNew();
			_sidePanel.Active.Should().HaveCount(1);

			viewModel.RemoveCommand.Execute(null);
			_sidePanel.Active.Should().BeEmpty("because we've just removed the only analysis");
		}

		[Test]
		public void TestRemoveAnalysis2()
		{
			var viewModel = _sidePanel.CreateNew();
			_sidePanel.Active.Should().HaveCount(1);

			_analysisStorage.Verify(x => x.Remove(It.IsAny<AnalysisId>()), Times.Never);
			viewModel.RemoveCommand.Execute(null);
			_analysisStorage.Verify(x => x.Remove(It.Is<AnalysisId>(y => y == viewModel.Id)), Times.Once,
				"because the view model is responsible for removing the analysis (so it doesn't continue to run in the background)");
		}
	}
}