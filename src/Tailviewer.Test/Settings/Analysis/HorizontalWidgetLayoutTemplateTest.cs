using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Analysis.Layouts;

namespace Tailviewer.Test.Settings.Analysis
{
	[TestFixture]
	public sealed class HorizontalWidgetLayoutTemplateTest
	{
		[Test]
		public void TestClone()
		{
			var template = new HorizontalWidgetLayoutTemplate();
			var clone = template.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(template);
		}
	}
}
