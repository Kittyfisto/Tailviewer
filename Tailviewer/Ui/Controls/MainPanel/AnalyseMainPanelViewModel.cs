using System.Collections.Generic;
using System.Collections.ObjectModel;
using Tailviewer.Ui.Controls.Widgets;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public sealed class AnalyseMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private readonly ObservableCollection<WidgetLayoutViewModel> _layouts;

		public AnalyseMainPanelViewModel()
		{
			_layouts = new ObservableCollection<WidgetLayoutViewModel>();
		}

		public IEnumerable<WidgetLayoutViewModel> Layouts => _layouts;

		public override void Update()
		{
			
		}
	}
}