using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Analysis.DataSources.Ui;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Analysis.DataSources.Test.Ui
{
	[TestFixture]
	public sealed class DataSourcesWidgetViewModelTest
	{
		[Test]
		public void TestConstruction()
		{
			var viewModel = new DataSourcesWidgetViewModel(
				new WidgetTemplate {Configuration = new DataSourcesWidgetConfiguration()},
				new Mock<IDataSourceAnalyser>().Object);
			viewModel.CanBeEdited.Should().BeTrue();
		}
	}
}
