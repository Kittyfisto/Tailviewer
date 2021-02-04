using System.Windows.Media;
using Metrolib;

namespace Tailviewer.Ui.Controls.MainPanel.Help
{
	public sealed class HelpMainPanelEntry
		: IMainPanelEntry
	{
		public string Title => "Help";

		public string Id => "help";

		public string ToolTip => null;

		public Geometry Icon => Icons.HelpCircleOutline;
	}
}
