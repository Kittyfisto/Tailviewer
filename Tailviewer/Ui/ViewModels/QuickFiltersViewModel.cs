using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using QuickFilter = Tailviewer.BusinessLogic.QuickFilter;
using QuickFilters = Tailviewer.BusinessLogic.QuickFilters;

namespace Tailviewer.Ui.ViewModels
{
	/// <summary>
	/// Represents the list of all quick filters.
	/// </summary>
	internal sealed class QuickFiltersViewModel
	{
		private readonly QuickFilters _quickFilters;
		private readonly ObservableCollection<QuickFilterViewModel> _viewModels;
		private readonly ICommand _addCommand;
		private readonly ApplicationSettings _settings;
		private IDataSourceViewModel _currentDataSource;
		private bool _isChangingCurrentDataSource;

		/// <summary>
		/// This event is fired when a filter has been changed so the filtered contents MIGHT change.
		/// This includes: activate/deactive, changing the filter value, etc...
		/// </summary>
		public event Action OnFiltersChanged;

		public QuickFiltersViewModel(ApplicationSettings settings, QuickFilters quickFilters)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (quickFilters == null) throw new ArgumentNullException("quickFilters");

			_settings = settings;
			_quickFilters = quickFilters;
			_addCommand = new DelegateCommand(() => AddQuickFilter());
			_viewModels = new ObservableCollection<QuickFilterViewModel>();
			foreach (var filter in quickFilters.Filters)
			{
				CreateAndAddViewModel(filter);
			}
		}

		public QuickFilterViewModel AddQuickFilter()
		{
			var quickFilter = _quickFilters.Add();
			var viewModel = CreateAndAddViewModel(quickFilter);
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
			foreach (var quickFilter in _viewModels)
// ReSharper restore LoopCanBeConvertedToQuery
			{
				ILogEntryFilter filter = null;
				try
				{
					filter = quickFilter.CreateFilter();
				}
				catch (Exception)
				{}

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
				case "Type":
					if (!_isChangingCurrentDataSource)
					{
						var fn = OnFiltersChanged;
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
					var source = value != null ? value.DataSource : null;
					foreach (var viewModel in _viewModels)
					{
						viewModel.CurrentDataSource = source;
					}

					var fn = OnFiltersChanged;
					if (fn != null)
						fn();
				}
				finally
				{
					_isChangingCurrentDataSource = false;
				}
			}
		}
	}
}