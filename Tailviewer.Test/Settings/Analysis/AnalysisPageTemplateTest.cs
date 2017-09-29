using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Test.Settings.Analysis
{
	[TestFixture]
	public sealed class AnalysisPageTemplateTest
	{
		[Test]
		public void TestClone1()
		{
			var template = new AnalysisPageTemplate();
			var clone = template.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(template);
			clone.Layout.Should().BeNull();
			clone.Widgets.Should().BeEmpty();
		}

		[Test]
		public void TestClone2()
		{
			var template = new AnalysisPageTemplate();
			template.Layout = new HorizontalWidgetLayoutTemplate();

			var clone = template.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(template);
			clone.Layout.Should().NotBeNull();
			clone.Layout.Should().BeOfType<HorizontalWidgetLayoutTemplate>();
			clone.Layout.Should().NotBeSameAs(template.Layout);
			clone.Widgets.Should().BeEmpty();
		}

		[Test]
		public void TestClone3()
		{
			var template = new AnalysisPageTemplate();
			var widget = new WidgetTemplate();
			template.Add(widget);

			var clone = template.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(template);
			clone.Layout.Should().BeNull();
			clone.Widgets.Should().HaveCount(1);
			clone.Widgets.ElementAt(0).Should().NotBeNull();
			clone.Widgets.ElementAt(0).Should().NotBeSameAs(widget);
		}
	}
}