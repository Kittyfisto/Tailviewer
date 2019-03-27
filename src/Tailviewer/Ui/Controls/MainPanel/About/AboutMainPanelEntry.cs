using System.Windows.Media;
using Metrolib;

namespace Tailviewer.Ui.Controls.MainPanel.About
{
	public sealed class AboutMainPanelEntry
		: IMainPanelEntry
	{
		public string Title => "About";

		public string Id => "about";

		public string ToolTip => null;

		public Geometry Icon => Icons.InformationOutline;
	}
}