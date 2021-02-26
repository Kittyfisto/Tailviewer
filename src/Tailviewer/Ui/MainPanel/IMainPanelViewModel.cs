using System.Collections.Generic;
using System.ComponentModel;
using Tailviewer.Ui.ContextMenu;
using Tailviewer.Ui.LogView;
using Tailviewer.Ui.SidePanel;

namespace Tailviewer.Ui.MainPanel
{
	public interface IMainPanelViewModel
		: INotifyPropertyChanged
	{
		#region Main Window Menu Items

		IEnumerable<IMenuViewModel> FileMenuItems { get; }

		IEnumerable<IMenuViewModel> ViewMenuItems { get; }
		
		#endregion

		ISearchViewModel Search { get; }

		IFindAllViewModel FindAll { get; }
		
		IEnumerable<ISidePanelViewModel> SidePanels { get; }
		ISidePanelViewModel SelectedSidePanel { get; set; }

		void Update();
	}
}
