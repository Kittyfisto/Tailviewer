using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using QuickFilter = Tailviewer.Settings.QuickFilter;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class QuickFilterTest
	{
		[Test]
		public void TestSubstringFilter1()
		{
			var quickFilter = new Tailviewer.BusinessLogic.QuickFilter(new QuickFilter())
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
			var quickFilter = new Tailviewer.BusinessLogic.QuickFilter(new QuickFilter())
			{
				Value = "he*rld",
				IgnoreCase = true,
				Type = QuickFilterType.WildcardFilter
			};
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "Hello World!", LevelFlags.None)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "hELlo wORld!", LevelFlags.None)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "Hello Wold!", LevelFlags.None)).Should().BeFalse();
		}
	}
}