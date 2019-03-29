using System.Windows.Media;
using Metrolib;

namespace Tailviewer.Ui.Controls.MainPanel.Plugins
{
	/// <summary>
	/// Represents the "Plugins" tab on the left side of tailviewer's UI.
	/// </summary>
	public sealed class PluginsMainPanelEntry
		: IMainPanelEntry
	{
		public string Title => "Plugins";

		public string Id => "plugins";

		public string ToolTip => "View installed plugins";

		public Geometry Icon => Icons.Puzzle;
	}
}