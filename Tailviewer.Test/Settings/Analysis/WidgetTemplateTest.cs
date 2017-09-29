using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Core.Analysis;
using Tailviewer.Ui.Analysis;

namespace Tailviewer.Test.Settings.Analysis
{
	[TestFixture]
	public sealed class WidgetTemplateTest
	{
		[Test]
		public void TestClone1()
		{
			var template = new WidgetTemplate
			{
				Title = "Foobar"
			};
			var clone = template.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(template);
			clone.Title.Should().Be("Foobar");
			clone.ViewConfiguration.Should().BeNull();
		}

		[Test]
		public void TestClone2()
		{
			var viewConfiguration = new Mock<IWidgetConfiguration>();
			viewConfiguration.Setup(x => x.Clone()).Returns(() => new Mock<IWidgetConfiguration>().Object);
			var template = new WidgetTemplate
			{
				ViewConfiguration = viewConfiguration.Object
			};
			viewConfiguration.Verify(x => x.Clone(), Times.Never);

			var clone = template.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(template);
			clone.Title.Should().BeNull();
			clone.ViewConfiguration.Should().NotBeNull();
			clone.ViewConfiguration.Should().NotBeSameAs(viewConfiguration.Object);
			viewConfiguration.Verify(x => x.Clone(), Times.Once);
		}

		[Test]
		public void TestSerialize()
		{
			var template = new WidgetTemplate
			{
				Id = WidgetId.CreateNew(),
				AnalyserId = AnalyserId.CreateNew(),
				Title = "dwankwadjkwad",
				ViewConfiguration = null
			};

			var actualTemplate = template.Roundtrip();
			actualTemplate.Should().NotBeNull();
			actualTemplate.Id.Should().Be(template.Id);
			actualTemplate.AnalyserId.Should().Be(template.AnalyserId);
			actualTemplate.Title.Should().Be(template.Title);
			actualTemplate.ViewConfiguration.Should().BeNull();
		}
	}
}
