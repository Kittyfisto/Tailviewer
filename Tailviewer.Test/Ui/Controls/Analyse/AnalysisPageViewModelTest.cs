using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets;

namespace Tailviewer.Test.Ui.Controls.Analyse
{
	[TestFixture]
	public sealed class AnalysisPageViewModelTest
	{
		private Mock<IAnalyserGroup> _analyser;

		[SetUp]
		public void Setup()
		{
			_analyser = new Mock<IAnalyserGroup>();
		}

		[Test]
		public void TestCtor()
		{
			var model = new AnalysisPageViewModel(_analyser.Object);
			model.Name.Should().NotBeEmpty();
			model.PageLayout.Should().Be(PageLayout.WrapHorizontal);
			model.Layout.Should().NotBeNull();
			model.Layout.Should().BeOfType<HorizontalWidgetLayoutViewModel>();
		}

		[Test]
		public void TestAddWidget1()
		{
			var model = new AnalysisPageViewModel(_analyser.Object);
			var widget = new Mock<IWidgetViewModel>();
			model.Add(widget.Object);
			((HorizontalWidgetLayoutViewModel) model.Layout).Widgets.Should().Contain(widget.Object);
		}

		[Test]
		public void TestAddWidget2()
		{
			var model = new AnalysisPageViewModel(_analyser.Object);
			model.PageLayout = PageLayout.None;
			var widget = new Mock<IWidgetViewModel>();
			model.Add(widget.Object);
			model.PageLayout = PageLayout.WrapHorizontal;
			((HorizontalWidgetLayoutViewModel)model.Layout).Widgets.Should().Contain(widget.Object);
		}

		[Test]
		[Description("Verifies that if a layout requests that a widget be added, the page does so")]
		public void TestRequestAddWidget()
		{
			var model = new AnalysisPageViewModel(_analyser.Object);
			var layout = (HorizontalWidgetLayoutViewModel) model.Layout;

			var widget = new Mock<IWidgetViewModel>().Object;
			var factory = new Mock<IWidgetFactory>();
			factory.Setup(x => x.Create(It.IsAny<IDataSourceAnalyser>()))
				.Returns(widget);

			layout.Widgets.Should().NotContain(widget);

			layout.RaiseRequestAdd(factory.Object);
			layout.Widgets.Should().Contain(widget);
		}
	}
}