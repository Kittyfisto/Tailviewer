using System.Collections.Generic;
using System.Collections.ObjectModel;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.SidePanel;
using Tailviewer.Ui.Controls.Widgets;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public sealed class AnalyseMainPanelViewModel
		: AbstractMainPanelViewModel
	{
		private readonly ObservableCollection<WidgetLayoutViewModel> _layouts;

		public AnalyseMainPanelViewModel(IApplicationSettings applicationSettings)
			: base(applicationSettings)
		{
			_layouts = new ObservableCollection<WidgetLayoutViewModel>();
		}

		public IEnumerable<WidgetLayoutViewModel> Layouts => _layouts;

		public override IEnumerable<ISidePanelViewModel> SidePanels => null;

		public override void Update()
		{
			
		}
	}
}