using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Test.Settings.Analysis
{
	[TestFixture]
	public sealed class AnalysisViewTemplateTest
	{
		[Test]
		public void TestConstruction()
		{
			var template = new AnalysisViewTemplate();
			template.Name.Should().Be("<Unnamed>");
		}

		[Test]
		public void TestSerialize1()
		{
			var template = new AnalysisViewTemplate
			{
				Name = "Ronny"
			};

			var actualTemplate = template.Roundtrip();
			actualTemplate.Should().NotBeNull();
			actualTemplate.Name.Should().Be("Ronny");
		}
	}
}