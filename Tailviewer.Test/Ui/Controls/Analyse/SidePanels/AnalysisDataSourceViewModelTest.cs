using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;
using Tailviewer.Ui.Controls.MainPanel.Analyse;
using Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels;

namespace Tailviewer.Test.Ui.Controls.Analyse.SidePanels
{
	[TestFixture]
	public sealed class AnalysisDataSourceViewModelTest
	{
		private Mock<IDataSource> _dataSource;
		private Mock<IAnalysisViewModel> _analysis;
		private AnalysisId _analysisId;

		[SetUp]
		public void Setup()
		{
			_dataSource = new Mock<IDataSource>();
			_dataSource.Setup(x => x.FullFileName).Returns(@"X:\foo\bar\mega log file.txt");

			_analysis = new Mock<IAnalysisViewModel>();
			_analysisId = AnalysisId.CreateNew();
			_analysis.Setup(x => x.Id).Returns(_analysisId);
		}

		[Test]
		public void TestCtor()
		{
			var viewModel = new AnalysisDataSourceViewModel(_dataSource.Object);
			viewModel.DisplayName.Should().Be("mega log file.txt");
			viewModel.Folder.Should().Be(@"X:\foo\bar");
		}

		[Test]
		public void TestChangeAnalysis()
		{
			var viewModel = new AnalysisDataSourceViewModel(_dataSource.Object);
			_dataSource.Setup(x => x.IsAnalysisActive(It.Is<AnalysisId>(y => y == _analysisId)))
				.Returns(true);

			viewModel.IsSelected.Should().BeFalse();

			viewModel.CurrentAnalysis = _analysis.Object;
			viewModel.IsSelected.Should().BeTrue();

			viewModel.CurrentAnalysis = null;
			viewModel.IsSelected.Should().BeFalse();
		}

		[Test]
		public void TestIsSelected()
		{
			var viewModel = new AnalysisDataSourceViewModel(_dataSource.Object) {CurrentAnalysis = _analysis.Object};
			viewModel.IsSelected.Should().BeFalse();

			viewModel.IsSelected = true;
			_dataSource.Verify(x => x.EnableAnalysis(It.Is<AnalysisId>(y => y == _analysisId)), Times.Once);

			viewModel.IsSelected = false;
			_dataSource.Verify(x => x.DisableAnalysis(It.Is<AnalysisId>(y => y == _analysisId)), Times.Once);
		}
	}
}