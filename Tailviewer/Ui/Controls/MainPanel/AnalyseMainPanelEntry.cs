using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Metrolib;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public sealed class AnalyseMainPanelEntry
		: IMainPanelEntry
	{
		private readonly ISidePanelViewModel[] _sidePanels;
		private ISidePanelViewModel _selectedSidePanel;

		public AnalyseMainPanelEntry()
		{
			_sidePanels = new ISidePanelViewModel[]
			{
				null
			};
		}

		public string Title => "Analyse";

		public string Id => "analyse";

		public Geometry Icon => Icons.ChartBar;

		public IEnumerable<ISidePanelViewModel> SidePanels => _sidePanels;

		public ISidePanelViewModel SelectedSidePanel
		{
			get { return _selectedSidePanel; }
			set
			{
				if (value == _selectedSidePanel)
					return;

				_selectedSidePanel = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}