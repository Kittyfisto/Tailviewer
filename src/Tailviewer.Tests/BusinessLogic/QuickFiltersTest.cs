using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;

namespace Tailviewer.Tests.BusinessLogic
{
	[TestFixture]
	public sealed class QuickFiltersTest
	{
		public static IEnumerable<SpecialDateTimeInterval> Ranges
		{
			get
			{
				return new[]
				{
					SpecialDateTimeInterval.Today,
					SpecialDateTimeInterval.ThisWeek
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
			_settings = new QuickFiltersSettings();
		}

		private QuickFiltersSettings _settings;

		[Test]
		public void TestCtor()
		{
			_settings.Add(new QuickFilterSettings {Value = "foo"});
			_settings.Add(new QuickFilterSettings {Value = "bar"});
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
		public void TestChangeTimeFilter([ValueSource(nameof(Ranges))] SpecialDateTimeInterval range,
		                                 [ValueSource(nameof(DateTimes))] DateTime? minimum,
		                                 [ValueSource(nameof(DateTimes))] DateTime? maximum)
		{
			var quickFilters = new Tailviewer.BusinessLogic.Filters.QuickFilters(_settings);
			quickFilters.TimeFilter.SpecialInterval = range;
			quickFilters.TimeFilter.Minimum = minimum;
			quickFilters.TimeFilter.Maximum = maximum;

			_settings.TimeFilter.SpecialInterval.Should().Be(range);
			_settings.TimeFilter.Minimum.Should().Be(minimum);
			_settings.TimeFilter.Maximum.Should().Be(maximum);
		}
	}
}