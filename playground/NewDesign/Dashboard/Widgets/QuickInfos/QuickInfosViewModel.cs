using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Metrolib;

namespace NewDesign.Dashboard.Widgets.QuickInfos
{
	public sealed class QuickInfosViewModel
		: AbstractWidgetViewModel
	{
		private readonly ObservableCollection<QuickInfoViewModel> _infos;

		public QuickInfosViewModel(params QuickInfoViewModel[] infos)
		{
			_infos = new ObservableCollection<QuickInfoViewModel>();
			foreach (var info in infos)
			{
				Add(info);
			}
			AddQuickInfoCommand = new DelegateCommand(AddQuickInfo);
		}

		public IEnumerable<QuickInfoViewModel> Infos => _infos;

		public ICommand AddQuickInfoCommand { get; }

		private void AddQuickInfo()
		{
			Add(new QuickInfoViewModel(""));
		}

		private void Add(QuickInfoViewModel quickInfoViewModel)
		{
			_infos.Add(quickInfoViewModel);
			quickInfoViewModel.OnRemove += QuickInfoViewModelOnOnRemove;
		}

		private void QuickInfoViewModelOnOnRemove(QuickInfoViewModel model)
		{
			_infos.Remove(model);
		}
	}
}