using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Settings;
using Tailviewer.QuickInfo.BusinessLogic;

namespace Tailviewer.QuickInfo.Test.BusinessLogic
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