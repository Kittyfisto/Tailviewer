using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Tailviewer.Settings;
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
		private DataSourceViewModel _currentDataSource;

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
				_viewModels.Add(new QuickFilterViewModel(filter, OnRemoveQuickFilter));
			}
		}

		public QuickFilterViewModel AddQuickFilter()
		{
			var quickFilter = _quickFilters.Add();
			var viewModel = new QuickFilterViewModel(quickFilter, OnRemoveQuickFilter)
				{
					CurrentDataSource = _currentDataSource != null ? _currentDataSource.DataSource : null
				};
			_viewModels.Add(viewModel);
			_settings.Save();
			return viewModel;
		}

		private void OnRemoveQuickFilter(QuickFilterViewModel viewModel)
		{
			_viewModels.Remove(viewModel);
			_quickFilters.Remove(viewModel.Id);

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

		public DataSourceViewModel CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == _currentDataSource)
					return;

				_currentDataSource = value;
				var source = value != null ? value.DataSource : null;
				foreach (var viewModel in _viewModels)
				{
					viewModel.CurrentDataSource = source;
				}
			}
		}
	}
}