using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Settings;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class QuickFiltersTest
	{
		public static IEnumerable<SpecialTimeRange?> Ranges
		{
			get
			{
				return new SpecialTimeRange?[]
				{
					null,
					SpecialTimeRange.Today
				};
			}
		}

		public static IEnumerable<DateTime?> DateTimes
		{
			get
			{
				return new DateTime?[]
				{
					null,
					new DateTime(2017, 1, 1, 0, 0, 0),
				};
			}
		}

		[SetUp]
		public void SetUp()
		{
			_settings = new QuickFilters();
		}

		private QuickFilters _settings;

		[Test]
		public void TestCtor()
		{
			_settings.Add(new QuickFilter {Value = "foo"});
			_settings.Add(new QuickFilter {Value = "bar"});
			var quickFilters = new Tailviewer.BusinessLogic.Filters.QuickFilters(_settings);
			List<Tailviewer.BusinessLogic.Filters.QuickFilter> filters = quickFilters.Filters.ToList();
			filters.Count.Should().Be(2);
			filters[0].Id.Should().Be(_settings[0].Id);
			filters[0].Value.Should().Be("foo");
			filters[1].Id.Should().Be(_settings[1].Id);
			filters[1].Value.Should().Be("bar");
		}

		[Test]
		public void TestAddQuickFilter()
		{
			_settings.Should().BeEmpty();

			var quickFilters = new Tailviewer.BusinessLogic.Filters.QuickFilters(_settings);
			var quickFilter = quickFilters.AddQuickFilter();
			_settings.Should().HaveCount(1);

			quickFilter.Value = "foobar";
			_settings[0].Value.Should().Be("foobar");
		}

		[Test]
		public void TestChangeTimeFilter([ValueSource(nameof(Ranges))] SpecialTimeRange? range,
		                                 [ValueSource(nameof(DateTimes))] DateTime? start,
		                                 [ValueSource(nameof(DateTimes))] DateTime? end)
		{
			_settings.TimeFilter.Range.Should().BeNull();

			var quickFilters = new Tailviewer.BusinessLogic.Filters.QuickFilters(_settings);
			quickFilters.TimeFilter.Range = range;
			quickFilters.TimeFilter.Start = start;
			quickFilters.TimeFilter.End = end;

			_settings.TimeFilter.Range.Should().Be(range);
		}
	}
}