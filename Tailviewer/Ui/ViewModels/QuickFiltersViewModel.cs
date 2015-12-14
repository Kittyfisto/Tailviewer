using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Ui.ViewModels
{
	/// <summary>
	/// Represents the list of all quick filters.
	/// </summary>
	internal sealed class QuickFiltersViewModel
	{
		private readonly QuickFilters _quickFilters;
		private readonly ObservableCollection<QuickFilterViewModel> _viewModels;
		private readonly ICommand _addQuickFilter;

		public QuickFiltersViewModel(QuickFilters quickFilters)
		{
			if (quickFilters == null) throw new ArgumentNullException("quickFilters");

			_quickFilters = quickFilters;
			_addQuickFilter = new DelegateCommand(AddQuickFilter);
			_viewModels = new ObservableCollection<QuickFilterViewModel>();
		}

		private void AddQuickFilter()
		{
			var quickFilter = _quickFilters.Add();
			var viewModel = new QuickFilterViewModel(quickFilter, OnRemoveQuickFilter);
			_viewModels.Add(viewModel);
		}

		private void OnRemoveQuickFilter(QuickFilterViewModel viewModel)
		{
			_viewModels.Remove(viewModel);
			_quickFilters.Remove(viewModel.Id);
		}

		public ICommand AddQuickFilter1
		{
			get { return _addQuickFilter; }
		}
	}
}