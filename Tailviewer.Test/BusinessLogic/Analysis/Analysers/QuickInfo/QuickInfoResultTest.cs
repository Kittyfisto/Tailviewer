using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo;

namespace Tailviewer.Test.BusinessLogic.Analysis.Analysers.QuickInfo
{
	[TestFixture]
	public sealed class QuickInfoResultTest
	{
		[Test]
		public void TestCtor()
		{
			var result = new QuickInfoResult();
			result.QuickInfos.Should().NotBeNull();
			result.QuickInfos.Should().BeEmpty();
		}
	}
}