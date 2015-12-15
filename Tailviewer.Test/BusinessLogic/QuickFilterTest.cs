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
			quickFilter.CreateFilter().PassesFilter(new LogEntry("hello foobar!", LevelFlags.None)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogEntry("FOOBAR", LevelFlags.None)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogEntry("FOOBA", LevelFlags.None)).Should().BeFalse();
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
			quickFilter.CreateFilter().PassesFilter(new LogEntry("Hello World!", LevelFlags.None)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogEntry("hELlo wORld!", LevelFlags.None)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogEntry("Hello Wold!", LevelFlags.None)).Should().BeFalse();
		}
	}
}