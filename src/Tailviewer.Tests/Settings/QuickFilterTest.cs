using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;

namespace Tailviewer.Tests.Settings
{
	[TestFixture]
	public sealed class QuickFilterTest
	{
		[Test]
		public void TestClone()
		{
			var filter = new QuickFilterSettings
			{
				IgnoreCase = true,
				IsInverted = true,
				MatchType = FilterMatchType.TimeFilter,
				Value = "hello"
			};
			var clone = filter.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(filter);
			clone.IgnoreCase.Should().BeTrue();
			clone.IsInverted.Should().BeTrue();
			clone.MatchType.Should().Be(FilterMatchType.TimeFilter);
			clone.Value.Should().Be("hello");
		}

		[Test]
		public void TestRoundtrip([Values(true, false)] bool ignoreCase,
								  [Values(true, false)] bool isInverted,
								  [Values(FilterMatchType.RegexpFilter, FilterMatchType.SubstringFilter, FilterMatchType.WildcardFilter)] FilterMatchType matchType,
								  [Values("", "foo", "bar")] string value)
		{
			var id = QuickFilterId.CreateNew();
			var config = new QuickFilterSettings
			{
				Id = id,
				IgnoreCase = ignoreCase,
				IsInverted = isInverted,
				MatchType = matchType,
				Value = value
			};

			var actualQuickFilter = Roundtrip(config);
			actualQuickFilter.Should().NotBeNull();
			actualQuickFilter.Id.Should().Be(id);
			actualQuickFilter.IgnoreCase.Should().Be(ignoreCase);
			actualQuickFilter.IsInverted.Should().Be(isInverted);
			actualQuickFilter.MatchType.Should().Be(matchType);
			actualQuickFilter.Value.Should().Be(value);
		}

		private QuickFilterSettings Roundtrip(QuickFilterSettings quickFilter)
		{
			return quickFilter.Roundtrip(typeof(QuickFilterId));
		}
	}
}