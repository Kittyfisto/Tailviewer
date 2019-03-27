using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels.Analyses
{
	/// <summary>
	/// Represents a snapshot in the tree of available analyses, snapshots and templates.
	/// </summary>
	public sealed class AnalysisSnapshotItemViewModel
		: IItemViewModel
	{
		private readonly string _name;

		public AnalysisSnapshotItemViewModel(string name)
		{
			_name = name;
		}

		public string Name => _name;

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
