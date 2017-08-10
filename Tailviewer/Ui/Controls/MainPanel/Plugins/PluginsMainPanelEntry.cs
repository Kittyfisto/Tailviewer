using System.Windows.Media;
using Metrolib;

namespace Tailviewer.Ui.Controls.MainPanel.Plugins
{
	public sealed class PluginsMainPanelEntry
		: IMainPanelEntry
	{
		public string Title => "Plugins";

		public string Id => "plugins";

		public Geometry Icon => Icons.Puzzle;
	}
}