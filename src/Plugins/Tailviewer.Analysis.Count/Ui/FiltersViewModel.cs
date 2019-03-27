using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Metrolib;
using Tailviewer.Core.Settings;

namespace Tailviewer.Analysis.Count.Ui
{
	public sealed class FiltersViewModel
	{
		private readonly QuickFilters _configuration;
		private readonly ObservableCollection<FilterViewModel> _filters;
		private readonly DelegateCommand _addCommand;

		public FiltersViewModel(QuickFilters configuration)
		{
			_configuration = configuration;

			_filters = new ObservableCollection<FilterViewModel>();
			_addCommand = new DelegateCommand(Add);
		}

		private void Add()
		{
			var filter = new Core.Settings.QuickFilter();
			var viewModel = new FilterViewModel(filter);
			viewModel.OnRemoved += FilterOnOnRemoved;
			_filters.Add(viewModel);
			_configuration.Add(filter);
		}

		private void FilterOnOnRemoved(FilterViewModel filterViewModel)
		{
			_filters.Remove(filterViewModel);
			_configuration.Remove(filterViewModel.Filter);
		}

		public ICommand AddCommand => _addCommand;
		public IEnumerable<FilterViewModel> Filters => _filters;
	}
}