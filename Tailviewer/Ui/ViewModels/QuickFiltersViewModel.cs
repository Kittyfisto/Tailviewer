using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Ui.Controls.SidePanel;
using ApplicationSettings = Tailviewer.Settings.ApplicationSettings;
using QuickFilter = Tailviewer.BusinessLogic.Filters.QuickFilter;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Ui.ViewModels
{
	/// <summary>
	///     Represents the list of all quick filters.
	/// </summary>
	internal sealed class QuickFiltersViewModel
		: ISidePanelViewModel
	{
		private readonly ICommand _addCommand;
		private readonly QuickFilters _quickFilters;
		private readonly ApplicationSettings _settings;
		private readonly ObservableCollection<QuickFilterViewModel> _viewModels;
		private IDataSourceViewModel _currentDataSource;
		private bool _isChangingCurrentDataSource;
		private bool _isSelected;

		public QuickFiltersViewModel(ApplicationSettings settings, QuickFilters quickFilters)
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
			_settings.Save();
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

			_settings.Save();

			if (viewModel.IsActive)
			{
				// If we've just deleted an active filter then we most definately need
				// to filter the log file again...
				OnFiltersChanged?.Invoke();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public Geometry Icon => Icons.Filter;

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value == _isSelected)
					return;

				_isSelected = value;
				EmitPropertyChanged();
			}
		}

		public string Id => "quickfilter";

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}