using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.QuickInfo.BusinessLogic;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Test.Settings.Analysis
{
	[TestFixture]
	public sealed class WidgetTemplateTest
	{
		[Test]
		public void TestClone1()
		{
			var widget = new WidgetTemplate
			{
				Title = "Foobar"
			};
			var clone = widget.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(widget);
			clone.Title.Should().Be("Foobar");
			clone.ViewConfiguration.Should().BeNull();
			clone.AnalysisConfiguration.Should().BeNull();
		}

		[Test]
		public void TestClone2()
		{
			var viewConfiguration = new Mock<IWidgetConfiguration>();
			viewConfiguration.Setup(x => x.Clone()).Returns(() => new Mock<IWidgetConfiguration>().Object);
			var widget = new WidgetTemplate
			{
				ViewConfiguration = viewConfiguration.Object
			};
			viewConfiguration.Verify(x => x.Clone(), Times.Never);

			var clone = widget.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(widget);
			clone.Title.Should().BeNull();
			clone.ViewConfiguration.Should().NotBeNull();
			clone.ViewConfiguration.Should().NotBeSameAs(viewConfiguration.Object);
			clone.AnalysisConfiguration.Should().BeNull();
			viewConfiguration.Verify(x => x.Clone(), Times.Once);
		}

		[Test]
		public void TestClone3()
		{
			var analysisConfiguration = new Mock<ILogAnalyserConfiguration>();
			analysisConfiguration.Setup(x => x.Clone()).Returns(new Mock<ILogAnalyserConfiguration>().Object);
			var widget = new WidgetTemplate
			{
				AnalysisConfiguration = analysisConfiguration.Object
			};

			analysisConfiguration.Verify(x => x.Clone(), Times.Never);
			var clone = widget.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(widget);
			clone.Title.Should().BeNull();
			clone.ViewConfiguration.Should().BeNull();
			clone.AnalysisConfiguration.Should().NotBeNull();
			clone.AnalysisConfiguration.Should().NotBeSameAs(analysisConfiguration.Object);
			analysisConfiguration.Verify(x => x.Clone(), Times.Once);
		}

		[Test]
		public void TestClone4()
		{
			var analysisConfiguration = new QuickInfoAnalyserConfiguration();
			var widget = new WidgetTemplate
			{
				AnalysisConfiguration = analysisConfiguration
			};
			var clone = widget.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(widget);
			clone.Title.Should().BeNull();
			clone.ViewConfiguration.Should().BeNull();
			clone.AnalysisConfiguration.Should().NotBeNull();
			clone.AnalysisConfiguration.Should().NotBeSameAs(analysisConfiguration);
		}
	}
}
