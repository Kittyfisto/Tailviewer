using System.Collections.Generic;
using System.ComponentModel;
using Tailviewer.Ui.Controls.SidePanel;
using Tailviewer.Ui.ViewModels.ContextMenu;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public interface IMainPanelViewModel
		: INotifyPropertyChanged
	{
		#region Main Window Context Menu Items

		IEnumerable<IMenuViewModel> FileMenuItems { get; }

		IEnumerable<IMenuViewModel> ViewMenuItems { get; }

		#endregion

		IEnumerable<ISidePanelViewModel> SidePanels { get; }
		ISidePanelViewModel SelectedSidePanel { get; set; }

		void Update();
	}
}
