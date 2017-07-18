using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Settings;
using Tailviewer.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class QuickFiltersTest
	{
		[Test]
		public void TestClone()
		{
			var filters = new QuickFilters
			{
				new QuickFilter
				{
					MatchType = QuickFilterMatchType.WildcardFilter
				}
			};
			var clone = filters.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(filters);
			clone.Count.Should().Be(1);
			clone[0].Should().NotBeNull();
			clone[0].Should().NotBeSameAs(filters[0]);
			clone[0].MatchType.Should().Be(filters[0].MatchType);
		}
	}
}