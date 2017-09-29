using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Test.Settings.Analysis
{
	[TestFixture]
	public sealed class AnalyserTemplateTest
	{
		[Test]
		public void TestClone3()
		{
			var analysisConfiguration = new Mock<ILogAnalyserConfiguration>();
			analysisConfiguration.Setup(x => x.Clone()).Returns(new Mock<ILogAnalyserConfiguration>().Object);
			var widget = new AnalyserTemplate
			{
				Configuration = analysisConfiguration.Object
			};

			analysisConfiguration.Verify(x => x.Clone(), Times.Never);
			var clone = widget.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(widget);
			clone.Configuration.Should().NotBeNull();
			clone.Configuration.Should().NotBeSameAs(analysisConfiguration.Object);
			analysisConfiguration.Verify(x => x.Clone(), Times.Once);
		}

	}
}
