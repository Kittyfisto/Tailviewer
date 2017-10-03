using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Test.Settings.Analysis
{
	[TestFixture]
	public sealed class PageTemplateTest
	{
		[Test]
		public void TestClone1()
		{
			var template = new PageTemplate();
			var clone = template.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(template);
			clone.Layout.Should().BeNull();
			clone.Widgets.Should().BeEmpty();
		}

		[Test]
		public void TestClone2()
		{
			var template = new PageTemplate();
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
			var template = new PageTemplate();
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

		[Test]
		public void TestSerialize1()
		{
			var template = new PageTemplate();

			var actualTemplate = template.Roundtrip();
			actualTemplate.Should().NotBeNull();
			actualTemplate.Layout.Should().BeNull();
			actualTemplate.Widgets.Should().BeEmpty();
		}

		[Test]
		public void TestSerialize2()
		{
			var template = new PageTemplate
			{
				Layout = new HorizontalWidgetLayoutTemplate()
			};

			var actualTemplate = template.Roundtrip(typeof(HorizontalWidgetLayoutTemplate));
			actualTemplate.Should().NotBeNull();
			actualTemplate.Layout.Should().NotBeNull();
			actualTemplate.Layout.Should().BeOfType<HorizontalWidgetLayoutTemplate>();
			actualTemplate.Layout.Should().NotBeSameAs(template.Layout);
			actualTemplate.Widgets.Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that not being able to restore the layout is NOT a problem")]
		public void TestSerialize3()
		{
			var template = new PageTemplate
			{
				Layout = new HorizontalWidgetLayoutTemplate()
			};

			// We don't specify the layout type here, causing the factory to not be able to restore
			// the layout upon deserialization
			var actualTemplate = template.Roundtrip();
			actualTemplate.Should().NotBeNull();
			actualTemplate.Layout.Should().BeNull();
		}
	}
}