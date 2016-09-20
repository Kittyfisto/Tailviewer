using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.Settings;
using QuickFilter = Tailviewer.BusinessLogic.Filters.QuickFilter;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer.Ui.ViewModels
{
	/// <summary>
	///     Represents the list of all quick filters.
	/// </summary>
	internal sealed class QuickFiltersViewModel
	{
		private readonly ICommand _addCommand;
		private readonly QuickFilters _quickFilters;
		private readonly ApplicationSettings _settings;
		private readonly ObservableCollection<QuickFilterViewModel> _viewModels;
		private IDataSourceViewModel _currentDataSource;
		private bool _isChangingCurrentDataSource;

		public QuickFiltersViewModel(ApplicationSettings settings, QuickFilters quickFilters)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (quickFilters == null) throw new ArgumentNullException("quickFilters");

			_settings = settings;
			_quickFilters = quickFilters;
			_addCommand = new DelegateCommand(() => AddQuickFilter());
			_viewModels = new ObservableCollection<QuickFilterViewModel>();
			foreach (QuickFilter filter in quickFilters.Filters)
			{
				CreateAndAddViewModel(filter);
			}
		}

		public ICommand AddCommand
		{
			get { return _addCommand; }
		}

		public IEnumerable<QuickFilterViewModel> Observable
		{
			get { return _viewModels; }
		}

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
					IDataSource source = value != null ? value.DataSource : null;
					foreach (QuickFilterViewModel viewModel in _viewModels)
					{
						viewModel.CurrentDataSource = source;
					}

					Action fn = OnFiltersChanged;
					if (fn != null)
						fn();
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
					CurrentDataSource = _currentDataSource != null ? _currentDataSource.DataSource : null
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
						Action fn = OnFiltersChanged;
						if (fn != null)
							fn();
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
		}
	}
}