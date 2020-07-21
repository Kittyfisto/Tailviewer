using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.Settings;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class QuickFilterTest
	{
		[Test]
		public void TestCtor()
		{
			var quickFilter = new QuickFilter();
			quickFilter.Id.Should().NotBe(QuickFilterId.Empty);
			quickFilter.Value.Should().BeNullOrEmpty();
			quickFilter.IgnoreCase.Should().BeTrue();
			quickFilter.IsInverted.Should().BeFalse();
		}

		[Test]
		public void TestSubstringFilter1()
		{
			var quickFilter = new Tailviewer.BusinessLogic.Filters.QuickFilter(new QuickFilter())
				{
					Value = "foobar"
				};
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "hello foobar!", LevelFlags.Other)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "FOOBAR", LevelFlags.Other)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "FOOBA", LevelFlags.Other)).Should().BeFalse();
		}

		[Test]
		public void TestWildcardFilter()
		{
			var quickFilter = new Tailviewer.BusinessLogic.Filters.QuickFilter(new QuickFilter())
				{
					Value = "he*rld",
					IgnoreCase = true,
					MatchType = FilterMatchType.WildcardFilter
				};
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "Hello World!", LevelFlags.Other)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "hELlo wORld!", LevelFlags.Other)).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "Hello Wold!", LevelFlags.Other)).Should().BeFalse();
		}

		[Test]
		public void TestInvertFilter()
		{
			var quickFilter = new Tailviewer.BusinessLogic.Filters.QuickFilter(new QuickFilter())
				{
					Value = "foo",
					IsInverted = true
				};
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "foo", LevelFlags.Other)).Should().BeFalse();
			quickFilter.CreateFilter().PassesFilter(new LogLine(0, "bar", LevelFlags.Other)).Should().BeTrue();
		}
	}
}