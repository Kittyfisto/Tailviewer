using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class QuickFilterTest
	{
		[Test]
		public void TestSubstringFilter1()
		{
			var quickFilter = new Tailviewer.BusinessLogic.Filters.QuickFilter(new QuickFilter())
				{
					Value = "foobar"
				};
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "hello foobar!", LevelFlags.None)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "FOOBAR", LevelFlags.None)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "FOOBA", LevelFlags.None)).Should().BeFalse();
		}

		[Test]
		public void TestWildcardFilter()
		{
			var quickFilter = new Tailviewer.BusinessLogic.Filters.QuickFilter(new QuickFilter())
				{
					Value = "he*rld",
					IgnoreCase = true,
					MatchType = QuickFilterMatchType.WildcardFilter
				};
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "Hello World!", LevelFlags.None)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "hELlo wORld!", LevelFlags.None)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "Hello Wold!", LevelFlags.None)).Should().BeFalse();
		}
	}
}