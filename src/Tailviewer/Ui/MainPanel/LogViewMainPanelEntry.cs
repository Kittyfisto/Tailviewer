using System.Windows.Media;
using Metrolib;

namespace Tailviewer.Ui.MainPanel
{
	public sealed class LogViewMainPanelEntry
		: IMainPanelEntry
	{
		public string Title => "Raw";

		public string Id => "log";

		public string ToolTip => "View one or more log files as is";

		public Geometry Icon => Icons.ViewHeadline;
	}
}