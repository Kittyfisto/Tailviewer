using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Settings;
using Tailviewer.Settings;

namespace Tailviewer.Test.Settings
{
	[TestFixture]
	public sealed class QuickFilterTest
	{
		[Test]
		public void TestClone()
		{
			var filter = new QuickFilter
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
	}
}