using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Settings;
using Tailviewer.Ui.Controls.SidePanel.TimeFilter;

namespace Tailviewer.Test.Ui.Controls.SidePanel.TimeFilter
{
	[TestFixture]
	public sealed class TimeFiltersViewModelTest
	{
		private Tailviewer.BusinessLogic.Filters.TimeFilter _timeFilter;

		public static IEnumerable<SpecialDateTimeInterval> Ranges
		{
			get
			{
				return new SpecialDateTimeInterval[]
				{
					SpecialDateTimeInterval.Today,
					SpecialDateTimeInterval.ThisWeek,
					SpecialDateTimeInterval.ThisMonth,
					SpecialDateTimeInterval.ThisYear
				};
			}
		}

		[SetUp]
		public void Setup()
		{
			_timeFilter = new Tailviewer.BusinessLogic.Filters.TimeFilter(new Core.Settings.TimeFilter());
		}

		[Test]
		public void TestConstruction1()
		{
			var viewModel = new TimeFiltersViewModel(_timeFilter);
			viewModel.SelectedTimeRange.Should().BeNull();
			viewModel.HasNoTimeRange.Should().BeTrue();
			viewModel.SelectedTimeRangeTitle.Should().Be("Select: Everything");
		}

		[Test]
		public void TestConstruction2([ValueSource(nameof(Ranges))] SpecialDateTimeInterval range)
		{
			_timeFilter.Range = range;

			var viewModel = new TimeFiltersViewModel(_timeFilter);
			viewModel.HasNoTimeRange.Should().BeFalse();
			viewModel.SelectedTimeRange.Should().NotBeNull();
			viewModel.SelectedTimeRange.Should().BeOfType<SpecialTimeRangeViewModel>();
			((SpecialTimeRangeViewModel) viewModel.SelectedTimeRange).Range.Should().Be(range);
		}

		[Test]
		public void TestChangeSelectedFilter([ValueSource(nameof(Ranges))] SpecialDateTimeInterval range)
		{
			var viewModel = new TimeFiltersViewModel(_timeFilter);
			var tmp = viewModel.AvailableSpecialRanges.First(x => x.Range == range);

			viewModel.SelectedTimeRange = tmp;
			_timeFilter.Range.Should().Be(range);

			viewModel.SelectedTimeRange = null;
			_timeFilter.Range.Should().BeNull();
		}
	}
}
