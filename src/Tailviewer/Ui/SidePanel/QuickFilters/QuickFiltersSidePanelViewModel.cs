using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.Api;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Core;
using Tailviewer.Settings;
using Tailviewer.Ui.DataSourceTree;
using Tailviewer.Ui.QuickFilter;
using Tailviewer.Ui.SidePanel.QuickFilters.TimeFilter;

using Tailviewer.Localization;

namespace Tailviewer.Ui.SidePanel.QuickFilters
{
	/// <summary>
	///     Represents the list of all quick filters.
	///     Embedds a <see cref="QuickFiltersViewModel" /> into the context
	///     of the application wide list of quick filters for all data sources:
	///     Changes to filters are automatically persisted, etc...
	/// </summary>
	public sealed class QuickFiltersSidePanelViewModel
		: AbstractSidePanelViewModel
	{
		private readonly QuickFiltersViewModel _filters;
		private readonly IApplicationSettings _settings;
		private IDataSourceViewModel _currentDataSource;
		private readonly TimeFiltersViewModel _timeFilters;

		public QuickFiltersSidePanelViewModel(IApplicationSettings settings, IQuickFilters quickFilters)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));
			if (quickFilters == null) throw new ArgumentNullException(nameof(quickFilters));

			_settings = settings;
			_filters = new QuickFiltersViewModel(quickFilters);
			_filters.OnFilterAdded += QuickFiltersOnOnFilterAdded;
			_filters.OnFilterRemoved += QuickFiltersOnOnFilterRemoved;
			_filters.OnFiltersChanged += OnOnFiltersChanged;

			_timeFilters = new TimeFiltersViewModel(quickFilters.TimeFilter);
			_timeFilters.OnFiltersChanged += OnOnFiltersChanged;

			UpdateTooltip();
			PropertyChanged += OnPropertyChanged;
		}

		public ICommand AddCommand => _filters.AddCommand;

		public TimeFiltersViewModel TimeFilters => _timeFilters;

		public IEnumerable<QuickFilterViewModel> QuickFilters => _filters.QuickFilters;

		public IDataSourceViewModel CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == _currentDataSource)
					return;

				_currentDataSource = value;
				var source = value?.DataSource;
				_filters.CurrentDataSource = source;
				EmitPropertyChanged(nameof(FilterCombinationMode));
			}
		}

		public override Geometry Icon => Icons.Filter;

		public override string Id => "quickfilter";

		private void QuickFiltersOnOnFilterAdded()
		{
			_settings.SaveAsync();
		}

		private void QuickFiltersOnOnFilterRemoved()
		{
			_settings.SaveAsync();
		}

		private void UpdateTooltip()
		{
			Tooltip = IsSelected
				? Strings.HideQuickFilterList
				: Strings.ShowQuickFilterList;
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(IsSelected):
					UpdateTooltip();

					// It is visually jarring if a filter is added when the side panel closes,
					// therefore we only add the filter becomes selected!
					// See https://github.com/Kittyfisto/Tailviewer/issues/175
					if (IsSelected)
						AddEmptyFilterIfNecessary();
					break;
			}
		}

		private void AddEmptyFilterIfNecessary()
		{
			if (!HasEmptyFilter())
				AddQuickFilter();
		}

		private bool HasEmptyFilter()
		{
			foreach (var filter in QuickFilters)
			{
				if (string.IsNullOrEmpty(filter.Value))
				{
					return true;
				}
			}

			return false;
		}

		private void OnOnFiltersChanged()
		{
			var count = QuickFilters.Count(x => x.IsActive);
			if (!TimeFilters.SelectEverything)
				++count;

			QuickInfo = count > 0 ? string.Format(Strings.ActiveFilterCount, count) : null;
			OnFiltersChanged?.Invoke();
		}

		/// <summary>
		///     This event is fired when a filter has been changed so the filtered contents MIGHT change.
		///     This includes: activate/deactive, changing the filter value, etc...
		/// </summary>
		public event Action OnFiltersChanged;

		public QuickFilterViewModel AddQuickFilter()
		{
			return _filters.AddQuickFilter();
		}

		public IEnumerable<ILogEntryFilter> CreateFilterChain()
		{
			return _filters.CreateFilterChain();
		}

		public ILogEntryFilter CreateTimeFilter()
		{
			return _timeFilters.CreateFilter();
		}

		public FilterCombinationMode FilterCombinationMode
		{
			get
			{
				var dataSource = CurrentDataSource?.DataSource;
				return dataSource?.FilterCombinationMode ?? FilterCombinationMode.Or;
			}
			set
			{
				var dataSource = CurrentDataSource?.DataSource;
				if (dataSource == null)
					return;

				if (dataSource.FilterCombinationMode == value)
					return;

				dataSource.FilterCombinationMode = value;
				OnFiltersChanged?.Invoke();
			}
		}

		public override void Update()
		{
		}
	}
}