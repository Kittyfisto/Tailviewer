using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.Tests.BusinessLogic.Settings
{
	[TestFixture]
	public sealed class QuickFilterTest
	{
		[Test]
		public void TestCtor()
		{
			var quickFilter = new QuickFilterSettings();
			quickFilter.Id.Should().NotBe(QuickFilterId.Empty);
			quickFilter.Value.Should().BeNullOrEmpty();
			quickFilter.IgnoreCase.Should().BeTrue();
			quickFilter.IsInverted.Should().BeFalse();
		}

		[Test]
		public void TestSubstringFilter1()
		{
			var quickFilter = new Tailviewer.BusinessLogic.Filters.QuickFilter(new QuickFilterSettings())
				{
					Value = "foobar"
				};
			quickFilter.CreateFilter().PassesFilter(new LogEntry(Columns.Minimum){Index = 0, RawContent="hello foobar!", LogLevel = LevelFlags.Other}).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogEntry(Columns.Minimum){Index = 0, RawContent="FOOBAR", LogLevel = LevelFlags.Other}).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogEntry(Columns.Minimum){Index = 0, RawContent="FOOBA", LogLevel = LevelFlags.Other}).Should().BeFalse();
		}

		[Test]
		public void TestWildcardFilter()
		{
			var quickFilter = new Tailviewer.BusinessLogic.Filters.QuickFilter(new QuickFilterSettings())
				{
					Value = "he*rld",
					IgnoreCase = true,
					MatchType = FilterMatchType.WildcardFilter
				};
			quickFilter.CreateFilter().PassesFilter(new LogEntry(Columns.Minimum){Index=0, RawContent="Hello World!", LogLevel = LevelFlags.Other}).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogEntry(Columns.Minimum){Index=0, RawContent="hELlo wORld!", LogLevel = LevelFlags.Other }).Should().BeTrue();
			quickFilter.CreateFilter().PassesFilter(new LogEntry(Columns.Minimum){Index=0, RawContent="Hello Wold!", LogLevel = LevelFlags.Other}).Should().BeFalse();
		}

		[Test]
		public void TestInvertFilter()
		{
			var quickFilter = new Tailviewer.BusinessLogic.Filters.QuickFilter(new QuickFilterSettings())
				{
					Value = "foo",
					IsInverted = true
				};
			quickFilter.CreateFilter().PassesFilter(new LogEntry(Columns.Minimum){Index=0, RawContent="foo", LogLevel=LevelFlags.Other}).Should().BeFalse();
			quickFilter.CreateFilter().PassesFilter(new LogEntry(Columns.Minimum){Index=0, RawContent="bar", LogLevel=LevelFlags.Other }).Should().BeTrue();
		}
	}
}