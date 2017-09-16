using System.Windows.Media;
using Metrolib;
using Tailviewer.Ui.Controls.SidePanel;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels
{
	internal sealed class WidgetsSidePanel
		: AbstractSidePanelViewModel
	{
		public override Geometry Icon => Icons.Widgets;

		public override string Id => "Widgets";

		public override void Update()
		{
			
		}
	}
}