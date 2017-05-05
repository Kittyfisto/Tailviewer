using System.Windows.Media;
using Metrolib;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public sealed class AnalyseMainPanelEntry
		: IMainPanelEntry
	{
		public string Title => "Analyse";

		public string Id => "analyse";

		public Geometry Icon => Icons.ChartBar;
	}
}