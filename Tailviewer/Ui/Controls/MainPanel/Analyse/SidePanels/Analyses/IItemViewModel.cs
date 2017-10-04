using System.ComponentModel;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels.Analyses
{
	/// <summary>
	/// The interface for an item in the tree of available analyses, snapshots and templates.
	/// </summary>
	public interface IItemViewModel
		: INotifyPropertyChanged
	{
		string Name { get; }
	}
}