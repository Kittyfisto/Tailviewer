	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Runtime.CompilerServices;
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
				new SpecialTimeRangeViewModel(SpecialDateTimeInterval.Today),
				new SpecialTimeRangeViewModel(SpecialDateTimeInterval.ThisWeek),
				new SpecialTimeRangeViewModel(SpecialDateTimeInterval.ThisMonth),
				new SpecialTimeRangeViewModel(SpecialDateTimeInterval.ThisYear)
				/*new SpecialTimeRangeViewModel(DateTimeInterval.LastDay),
				new SpecialTimeRangeViewModel(DateTimeInterval.LastWeek),
				new SpecialTimeRangeViewModel(DateTimeInterval.LastMonth),
				new SpecialTimeRangeViewModel(DateTimeInterval.LastYear)*/
			};

			SelectEverything = timeFilter.Mode == TimeFilterMode.Everything;
			SelectByInterval = timeFilter.Mode == TimeFilterMode.Interval;
			SelectBySpecialInterval = timeFilter.Mode == TimeFilterMode.SpecialInterval;

			SelectedSpecialInterval = _availableSpecialRanges.FirstOrDefault(x => x.Interval == timeFilter.SpecialInterval);
			Minimum = timeFilter.Minimum;
			Maximum = timeFilter.Maximum;
		}

		private SpecialTimeRangeViewModel _selectedSpecialInterval;
		private bool _selectEverything;
		private string _description;
		private bool _selectByInterval;
		private bool _selectBySpecialInterval;

		public bool SelectEverything
		{
			get { return _selectEverything; }
			set
			{
				if (value == _selectEverything)
					return;
				_selectEverything = value;
				EmitPropertyChanged();

				if (value)
					SetFilterMode(TimeFilterMode.Everything);
			}
		}

		public bool SelectBySpecialInterval
		{
			get { return _selectBySpecialInterval; }
			set
			{
				if (value == _selectBySpecialInterval)
					return;
				_selectBySpecialInterval = value;
				EmitPropertyChanged();

				if (value)
					SetFilterMode(TimeFilterMode.SpecialInterval);
			}
		}

		public bool SelectByInterval
		{
			get { return _selectByInterval; }
			set
			{
				if (value == _selectByInterval)
					return;
				_selectByInterval = value;
				EmitPropertyChanged();

				if (value)
				{
					SetFilterMode(TimeFilterMode.Interval);
				}
			}
		}

		public string Description
		{
			get { return _description; }
			private set
			{
				if (value == _description)
					return;

				_description = value;
				EmitPropertyChanged();
			}
		}

		public SpecialTimeRangeViewModel SelectedSpecialInterval
		{
			get => _selectedSpecialInterval;
			set
			{
				if (Equals(value, _selectedSpecialInterval))
					return;

				_selectedSpecialInterval = value;
				EmitPropertyChanged();

				if (value != null)
					_timeFilter.SpecialInterval = value.Interval;

				UpdateTitle();
				OnFiltersChanged?.Invoke();
			}
		}

		public DateTime? Minimum
		{
			get => _timeFilter.Minimum;
			set
			{
				if (value == _timeFilter.Minimum)
					return;

				_timeFilter.Minimum = value;
				EmitPropertyChanged();
				UpdateTitle();
				OnFiltersChanged?.Invoke();
			}
		}

		public DateTime? Maximum
		{
			get => _timeFilter.Maximum;
			set
			{
				if (value == _timeFilter.Maximum)
					return;

				_timeFilter.Maximum = value;
				EmitPropertyChanged();
				UpdateTitle();
				OnFiltersChanged?.Invoke();
			}
		}

		public ILogEntryFilter CreateFilter()
		{
			return _timeFilter.CreateFilter();
		}

		public event Action OnFiltersChanged;

		public event PropertyChangedEventHandler PropertyChanged;

		private void UpdateTitle()
		{
			Description = BuildDescription();
		}

		[Pure]
		private string BuildDescription()
		{
			if (SelectEverything)
			{
				return "Select: Everything";
			}

			if (SelectBySpecialInterval)
			{
				return string.Format("Select: {0}", _selectedSpecialInterval?.Title);
			}

			if (Minimum != null)
			{
				if (Maximum != null)
					return string.Format("Select from {0} to {1}", Minimum, Maximum);

				return string.Format("Select from {0}", Minimum);
			}

			if (Maximum != null)
			{
				return string.Format("Select until {0}", Maximum);
			}

			return "Select from - to -";
		}

		private void SetFilterMode(TimeFilterMode mode)
		{
			SelectEverything = mode == TimeFilterMode.Everything;
			SelectByInterval = mode == TimeFilterMode.Interval;
			SelectBySpecialInterval = mode == TimeFilterMode.SpecialInterval;

			_timeFilter.Mode = mode;

			UpdateTitle();

			OnFiltersChanged?.Invoke();
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}