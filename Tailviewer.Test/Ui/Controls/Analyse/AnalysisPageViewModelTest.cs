using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Ui.Controls.MainPanel.Analyse;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets;

namespace Tailviewer.Test.Ui.Controls.Analyse
{
	[TestFixture]
	public sealed class AnalysisPageViewModelTest
	{
		[Test]
		public void TestCtor()
		{
			var model = new AnalysisPageViewModel();
			model.Name.Should().NotBeEmpty();
			model.PageLayout.Should().Be(PageLayout.WrapHorizontal);
			model.Layout.Should().NotBeNull();
			model.Layout.Should().BeOfType<HorizontalWidgetLayoutViewModel>();
		}

		[Test]
		public void TestAddWidget1()
		{
			var model = new AnalysisPageViewModel();
			var widget = new Mock<IWidgetViewModel>();
			model.Add(widget.Object);
			((HorizontalWidgetLayoutViewModel) model.Layout).Widgets.Should().Contain(widget.Object);
		}

		[Test]
		public void TestAddWidget2()
		{
			var model = new AnalysisPageViewModel();
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
			var model = new AnalysisPageViewModel();
			var layout = (HorizontalWidgetLayoutViewModel) model.Layout;

			var widget = new Mock<IWidgetViewModel>().Object;
			layout.Widgets.Should().NotContain(widget);

			layout.RaiseRequestAdd(widget);
			layout.Widgets.Should().Contain(widget);
		}
	}
}