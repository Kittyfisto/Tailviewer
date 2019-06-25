using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.QuickFilter;
using Tailviewer.Ui.Controls.SidePanel.TimeFilter;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.SidePanel.QuickFilters
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
				? "Hide the list of quick filters"
				: "Show the list of quick filters";
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
			QuickInfo = count > 0 ? string.Format("{0} active", count) : null;
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
			var quickFilter = _filters.CreateFilterChain();
			var timeFilter = _timeFilters.CreateFilter();
			if (timeFilter == null)
				return quickFilter;
			if (quickFilter == null)
				quickFilter = new List<ILogEntryFilter>();

			quickFilter.Add(timeFilter);
			return quickFilter;
		}

		public override void Update()
		{
		}
	}
}