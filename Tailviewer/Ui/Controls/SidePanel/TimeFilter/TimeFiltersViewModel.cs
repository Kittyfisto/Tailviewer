	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using Tailviewer.Core.Filters;
	using Tailviewer.Core.Settings;

namespace Tailviewer.Ui.Controls.SidePanel.TimeFilter
{
	public sealed class TimeFiltersViewModel
		: INotifyPropertyChanged
	{
		private readonly BusinessLogic.Filters.TimeFilter _timeFilter;
		private readonly IEnumerable<SpecialTimeRangeViewModel> _availableSpecialRanges;

		public IEnumerable<SpecialTimeRangeViewModel> AvailableSpecialRanges
		{
			get { return _availableSpecialRanges; }
		}

		public TimeFiltersViewModel(BusinessLogic.Filters.TimeFilter timeFilter)
		{
			if (timeFilter == null)
				throw new ArgumentNullException(nameof(timeFilter));

			_timeFilter = timeFilter;
			_availableSpecialRanges = new[]
			{
				new SpecialTimeRangeViewModel(SpecialTimeRange.Today),
				new SpecialTimeRangeViewModel(SpecialTimeRange.ThisWeek),
				new SpecialTimeRangeViewModel(SpecialTimeRange.ThisMonth),
				new SpecialTimeRangeViewModel(SpecialTimeRange.ThisYear),
				/*new SpecialTimeRangeViewModel(SpecialTimeRange.LastDay),
				new SpecialTimeRangeViewModel(SpecialTimeRange.LastWeek),
				new SpecialTimeRangeViewModel(SpecialTimeRange.LastMonth),
				new SpecialTimeRangeViewModel(SpecialTimeRange.LastYear)*/
			};

			HasNoTimeRange = true;

			SelectedTimeRange = _availableSpecialRanges.FirstOrDefault(x => x.Range == timeFilter.Range);
		}

		private IChoseTimeRangeViewModel _selectedTimeRange;
		private bool _hasNoTimeRange;
		private string _selectedTimeRangeTitle;

		public bool HasNoTimeRange
		{
			get { return _hasNoTimeRange; }
			set
			{
				if (value == _hasNoTimeRange)
					return;
				_hasNoTimeRange = value;
				EmitPropertyChanged();

				if (value)
					SelectedTimeRange = null;
				UpdateTitle();
			}
		}

		public string SelectedTimeRangeTitle
		{
			get { return _selectedTimeRangeTitle; }
			private set
			{
				if (value == _selectedTimeRangeTitle)
					return;

				_selectedTimeRangeTitle = value;
				EmitPropertyChanged();
			}
		}

		public IChoseTimeRangeViewModel SelectedTimeRange
		{
			get => _selectedTimeRange;
			set
			{
				if (Equals(value, _selectedTimeRange))
					return;

				_selectedTimeRange = value;
				EmitPropertyChanged();

				HasNoTimeRange = value == null;
				UpdateTitle();
				UpdateFilter(value);
			}
		}

		private void UpdateFilter(IChoseTimeRangeViewModel value)
		{
			if (value is SpecialTimeRangeViewModel model)
			{
				_timeFilter.Range = model.Range;
			}
			else
			{
				_timeFilter.Range = null;
			}

			OnFiltersChanged?.Invoke();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void UpdateTitle()
		{
			if (_selectedTimeRange == null)
			{
				SelectedTimeRangeTitle = "Select: Everything";
			}
			else
			{
				SelectedTimeRangeTitle = string.Format("Select: {0}", _selectedTimeRange.Title);
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public ILogEntryFilter CreateFilter()
		{
			return _timeFilter.CreateFilter();
		}

		public event Action OnFiltersChanged;
	}
}