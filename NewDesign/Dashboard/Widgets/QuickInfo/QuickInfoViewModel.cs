using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NewDesign.Dashboard.Widgets.QuickInfo
{
	public sealed class QuickInfoViewModel
		: AbstractWidgetViewModel
	{
		private readonly ObservableCollection<INamedValueViewModel> _infos;

		public QuickInfoViewModel(params INamedValueViewModel[] infos)
		{
			_infos = new ObservableCollection<INamedValueViewModel>(infos);
		}

		public IEnumerable<INamedValueViewModel> Infos => _infos;
	}
}