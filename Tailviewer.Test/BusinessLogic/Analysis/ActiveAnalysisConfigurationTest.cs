using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Test.BusinessLogic.Analysis
{
	[TestFixture]
	public sealed class ActiveAnalysisConfigurationTest
	{
		[Test]
		public void TestCtor1()
		{
			var id = AnalysisId.CreateNew();
			var template = new AnalysisTemplate();
			var viewTemplate = new AnalysisViewTemplate();
			var configuration = new ActiveAnalysisConfiguration(id, template, viewTemplate);

			configuration.Id.Should().Be(id);
			configuration.Template.Should().BeSameAs(template);
			configuration.ViewTemplate.Should().BeSameAs(viewTemplate);
		}
	}
}