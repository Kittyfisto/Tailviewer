using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class NoAnalyserTest
	{
		[Test]
		public void TestConstruction()
		{
			var analyser = new NoAnalyser();
			analyser.Id.Should().Be(AnalyserId.Empty, "because this placeholder for a missing / unknown analyser should only exist once and thus should have a constant id");
		}
	}
}
