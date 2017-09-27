using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis.Analysers.QuickInfo;
using Tailviewer.Core.Settings;

namespace Tailviewer.Test.BusinessLogic.Analysis.Analysers.QuickInfo
{
	[TestFixture]
	public sealed class QuickInfoConfigurationTest
	{
		[Test]
		public void TestCtor()
		{
			var config = new QuickInfoConfiguration();
			config.FilterValue.Should().BeNull();
			config.MatchType.Should().Be(FilterMatchType.RegexpFilter);
		}
	}
}