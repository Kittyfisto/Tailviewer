using System.Windows.Media;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public interface IMainPanelEntry
	{
		string Title { get; }
		string Id { get; }
		string ToolTip { get; }
		Geometry Icon { get; }
	}
}