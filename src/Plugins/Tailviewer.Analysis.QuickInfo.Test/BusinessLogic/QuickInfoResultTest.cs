using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Analysis.QuickInfo.BusinessLogic;

namespace Tailviewer.Analysis.QuickInfo.Test.BusinessLogic
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