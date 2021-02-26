using System.Windows.Media;
using Metrolib;
using Tailviewer.Ui.MainPanel;

namespace Tailviewer.Ui.About
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