using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core;
using Tailviewer.Ui.SidePanel.QuickFilters.TimeFilter;

namespace Tailviewer.Tests.Ui.Controls.SidePanel.TimeFilter
{
	[TestFixture]
	public sealed class TimeFiltersViewModelTest
	{
		private Tailviewer.BusinessLogic.Filters.TimeFilter _timeFilter;

		public static IEnumerable<SpecialDateTimeInterval> Ranges
		{
			get
			{
				return new[]
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
			_timeFilter = new Tailviewer.BusinessLogic.Filters.TimeFilter(new Core.TimeFilterSettings());
		}

		[Test]
		public void TestConstruction1()
		{
			var viewModel = new TimeFiltersViewModel(_timeFilter);
			viewModel.SelectEverything.Should().BeTrue();
			viewModel.SelectBySpecialInterval.Should().BeFalse();
			viewModel.SelectByInterval.Should().BeFalse();
			viewModel.Description.Should().Be("Select: Everything");
		}

		[Test]
		public void TestConstruction2([ValueSource(nameof(Ranges))] SpecialDateTimeInterval range)
		{
			_timeFilter.SpecialInterval = range;
			_timeFilter.Mode = TimeFilterMode.SpecialInterval;

			var viewModel = new TimeFiltersViewModel(_timeFilter);
			viewModel.SelectEverything.Should().BeFalse();
			viewModel.SelectBySpecialInterval.Should().BeTrue();
			viewModel.SelectedSpecialInterval.Should().NotBeNull();
			viewModel.SelectedSpecialInterval.Should().BeOfType<SpecialTimeRangeViewModel>();
			viewModel.SelectedSpecialInterval.Interval.Should().Be(range);
		}

		[Test]
		public void TestChangeSelectedFilter([ValueSource(nameof(Ranges))] SpecialDateTimeInterval interval)
		{
			var viewModel = new TimeFiltersViewModel(_timeFilter);
			var tmp = viewModel.AvailableSpecialRanges.First(x => x.Interval == interval);

			viewModel.SelectBySpecialInterval = true;
			viewModel.SelectedSpecialInterval = tmp;

			_timeFilter.SpecialInterval.Should().Be(interval);
			_timeFilter.Mode.Should().Be(TimeFilterMode.SpecialInterval);
		}

		[Test]
		[Description("Verifies that the configured special interval is remembered when switching between modes")]
		public void TestChangeMode([ValueSource(nameof(Ranges))] SpecialDateTimeInterval interval)
		{
			var viewModel = new TimeFiltersViewModel(_timeFilter);

			viewModel.SelectBySpecialInterval = true;
			viewModel.SelectedSpecialInterval = new SpecialTimeRangeViewModel(interval);

			viewModel.SelectEverything = true;
			viewModel.SelectBySpecialInterval = true;
			viewModel.SelectedSpecialInterval.Should().NotBeNull();
			viewModel.SelectedSpecialInterval.Interval.Should().Be(interval);
		}

		[Test]
		public void TestToggleModes()
		{
			var viewModel = new TimeFiltersViewModel(_timeFilter);
			viewModel.SelectEverything.Should().BeTrue();
			viewModel.SelectBySpecialInterval.Should().BeFalse();
			viewModel.SelectByInterval.Should().BeFalse();
			_timeFilter.Mode.Should().Be(TimeFilterMode.Everything);

			viewModel.SelectBySpecialInterval = true;
			viewModel.SelectBySpecialInterval.Should().BeTrue();
			viewModel.SelectEverything.Should().BeFalse();
			viewModel.SelectByInterval.Should().BeFalse();
			_timeFilter.Mode.Should().Be(TimeFilterMode.SpecialInterval);

			viewModel.SelectByInterval = true;
			viewModel.SelectByInterval.Should().BeTrue();
			viewModel.SelectEverything.Should().BeFalse();
			viewModel.SelectBySpecialInterval.Should().BeFalse();
			_timeFilter.Mode.Should().Be(TimeFilterMode.Interval);

			viewModel.SelectEverything = true;
			viewModel.SelectEverything.Should().BeTrue();
			viewModel.SelectBySpecialInterval.Should().BeFalse();
			viewModel.SelectByInterval.Should().BeFalse();
			_timeFilter.Mode.Should().Be(TimeFilterMode.Everything);
		}

		[Test]
		public void TestDescription()
		{
			var viewModel = new TimeFiltersViewModel(_timeFilter);

			viewModel.SelectBySpecialInterval = true;
			viewModel.SelectedSpecialInterval = new SpecialTimeRangeViewModel(SpecialDateTimeInterval.ThisWeek);

			viewModel.Description.Should().Be("Select: This week");
		}

		[Test]
		[SetCulture("en-US")]
		public void TestChangeMinimum()
		{
			var viewModel = new TimeFiltersViewModel(_timeFilter);
			viewModel.SelectByInterval = true;
			using (var monitor = viewModel.Monitor())
			{
				viewModel.Minimum = new DateTime(2018, 8, 13, 23, 25, 0);
				_timeFilter.Minimum.Should().Be(new DateTime(2018, 8, 13, 23, 25, 0));
				viewModel.Description.Should().Be("Select from 8/13/2018 11:25:00 PM");
				monitor.Should().Raise(nameof(TimeFiltersViewModel.OnFiltersChanged));
			}
		}

		[Test]
		[SetCulture("en-US")]
		public void TestChangeMaximum()
		{
			var viewModel = new TimeFiltersViewModel(_timeFilter);

			viewModel.SelectByInterval = true;
			using (var monitor = viewModel.Monitor())
			{
				viewModel.Maximum = new DateTime(2018, 8, 13, 23, 25, 0);
				_timeFilter.Maximum.Should().Be(new DateTime(2018, 8, 13, 23, 25, 0));
				viewModel.Description.Should().Be("Select until 8/13/2018 11:25:00 PM");
				monitor.Should().Raise(nameof(TimeFiltersViewModel.OnFiltersChanged));
			}
		}

		[Test]
		[SetCulture("en-US")]
		public void TestChangeMinimumAndMaximum()
		{
			var viewModel = new TimeFiltersViewModel(_timeFilter);
			viewModel.SelectByInterval = true;
			viewModel.Minimum = new DateTime(2018, 8, 13, 23, 25, 0);
			viewModel.Maximum = new DateTime(2018, 8, 14, 00, 30, 0);
			_timeFilter.Minimum.Should().Be(new DateTime(2018, 8, 13, 23, 25, 0));
			_timeFilter.Maximum.Should().Be(new DateTime(2018, 8, 14, 00, 30, 0));
			viewModel.Description.Should().Be("Select from 8/13/2018 11:25:00 PM to 8/14/2018 12:30:00 AM");
		}
	}
}
