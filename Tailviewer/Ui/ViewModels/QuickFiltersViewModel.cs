using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;
using QuickFilter = Tailviewer.BusinessLogic.Filters.QuickFilter;

namespace Tailviewer.Ui.ViewModels
{
	/// <summary>
	///     Represents the list of all quick filters.
	/// </summary>
	public sealed class QuickFiltersViewModel
		: AbstractSidePanelViewModel
	{
		private readonly ICommand _addCommand;
		private readonly IQuickFilters _quickFilters;
		private readonly IApplicationSettings _settings;
		private readonly ObservableCollection<QuickFilterViewModel> _viewModels;
		private IDataSourceViewModel _currentDataSource;
		private bool _isChangingCurrentDataSource;

		public QuickFiltersViewModel(IApplicationSettings settings, IQuickFilters quickFilters)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));
			if (quickFilters == null) throw new ArgumentNullException(nameof(quickFilters));

			_settings = settings;
			_quickFilters = quickFilters;
			_addCommand = new DelegateCommand(() => AddQuickFilter());
			_viewModels = new ObservableCollection<QuickFilterViewModel>();
			foreach (QuickFilter filter in quickFilters.Filters)
			{
				CreateAndAddViewModel(filter);
			}

			OnFiltersChanged += OnOnFiltersChanged;

			UpdateTooltip();
			PropertyChanged += OnPropertyChanged;
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
					break;
			}
		}

		private void OnOnFiltersChanged()
		{
			int count = _viewModels.Count(x => x.IsActive);
			QuickInfo = count > 0 ? string.Format("{0} active", count) : null;
		}

		public ICommand AddCommand => _addCommand;

		public IEnumerable<QuickFilterViewModel> QuickFilters => _viewModels;

		public IDataSourceViewModel CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == _currentDataSource)
					return;

				try
				{
					_isChangingCurrentDataSource = true;

					_currentDataSource = value;
					IDataSource source = value?.DataSource;
					foreach (QuickFilterViewModel viewModel in _viewModels)
					{
						viewModel.CurrentDataSource = source;
					}

					OnFiltersChanged?.Invoke();
				}
				finally
				{
					_isChangingCurrentDataSource = false;
				}
			}
		}

		/// <summary>
		///     This event is fired when a filter has been changed so the filtered contents MIGHT change.
		///     This includes: activate/deactive, changing the filter value, etc...
		/// </summary>
		public event Action OnFiltersChanged;

		public QuickFilterViewModel AddQuickFilter()
		{
			QuickFilter quickFilter = _quickFilters.Add();
			QuickFilterViewModel viewModel = CreateAndAddViewModel(quickFilter);
			_settings.SaveAsync();
			return viewModel;
		}

		private QuickFilterViewModel CreateAndAddViewModel(QuickFilter quickFilter)
		{
			var viewModel = new QuickFilterViewModel(quickFilter, OnRemoveQuickFilter)
				{
					CurrentDataSource = _currentDataSource?.DataSource
				};
			viewModel.PropertyChanged += QuickFilterOnPropertyChanged;
			_viewModels.Add(viewModel);
			return viewModel;
		}

		public IEnumerable<ILogEntryFilter> CreateFilterChain()
		{
			var filters = new List<ILogEntryFilter>(_viewModels.Count);
// ReSharper disable LoopCanBeConvertedToQuery
			foreach (QuickFilterViewModel quickFilter in _viewModels)
// ReSharper restore LoopCanBeConvertedToQuery
			{
				ILogEntryFilter filter = null;
				try
				{
					filter = quickFilter.CreateFilter();
				}
				catch (Exception)
				{
				}

				if (filter != null)
					filters.Add(filter);
			}

			if (filters.Count == 0)
				return null;

			return filters;
		}

		private void QuickFilterOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var model = sender as QuickFilterViewModel;
			if (model == null)
				return;

			switch (args.PropertyName)
			{
				case "Value":
				case "IsActive":
				case "IsInverted":
				case "DropType":
				case "MatchType":
					if (!_isChangingCurrentDataSource)
					{
						OnFiltersChanged?.Invoke();
					}
					break;
			}
		}

		private void OnRemoveQuickFilter(QuickFilterViewModel viewModel)
		{
			_viewModels.Remove(viewModel);
			_quickFilters.Remove(viewModel.Id);
			viewModel.PropertyChanged -= QuickFilterOnPropertyChanged;

			_settings.SaveAsync();

			if (viewModel.IsActive)
			{
				// If we've just deleted an active filter then we most definately need
				// to filter the log file again...
				OnFiltersChanged?.Invoke();
			}
		}

		public override Geometry Icon => Icons.Filter;

		public override string Id => "quickfilter";

		public override void Update()
		{
			
		}
	}
}