using System.Collections.Generic;
using System.ComponentModel;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public interface IMainPanelViewModel
		: INotifyPropertyChanged
	{
		IEnumerable<ISidePanelViewModel> SidePanels { get; }
		ISidePanelViewModel SelectedSidePanel { get; set; }

		void Update();
	}
}
