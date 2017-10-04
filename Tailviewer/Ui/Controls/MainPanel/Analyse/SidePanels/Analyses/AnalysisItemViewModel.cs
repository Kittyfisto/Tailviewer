using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels.Analyses
{
	/// <summary>
	/// Represents an active analysis in the tree of available analyses, snapshots and templates.
	/// </summary>
	public sealed class AnalysisItemViewModel
		: IItemViewModel
	{
		public string Name
		{
			get { throw new System.NotImplementedException(); }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}