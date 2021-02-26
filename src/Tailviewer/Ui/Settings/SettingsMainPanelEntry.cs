using System.Windows.Media;
using Metrolib;
using Tailviewer.Ui.MainPanel;

namespace Tailviewer.Ui.Settings
{
	public sealed class SettingsMainPanelEntry
		: IMainPanelEntry
	{
		public string Title => "Settings";

		public string Id => "settings";

		public string ToolTip => "Configure tailviewer to your liking";

		public Geometry Icon => Icons.Settings;
	}
}