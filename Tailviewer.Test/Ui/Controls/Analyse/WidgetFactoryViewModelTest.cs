using System.Windows.Media;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets;

namespace Tailviewer.Test.Ui.Controls.Analyse
{
	[TestFixture]
	public sealed class WidgetFactoryViewModelTest
	{
		[Test]
		public void TestCtor()
		{
			var geometry = new EllipseGeometry();
			var factory = new WidgetFactoryViewModel(() => null, "foo", "bar", geometry);
			factory.Name.Should().Be("foo");
			factory.Description.Should().Be("bar");
			factory.Icon.Should().Be(geometry);
		}

		[Test]
		public void TestCreate()
		{
			var widget = new Mock<IWidgetViewModel>().Object;
			var factory = new WidgetFactoryViewModel(() => widget, "foo", "bar");
			factory.Create().Should().BeSameAs(widget);
		}
	}
}
