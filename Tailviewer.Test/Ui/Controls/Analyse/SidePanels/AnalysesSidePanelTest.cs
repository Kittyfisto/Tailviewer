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
	}
}