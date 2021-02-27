using System.Windows.Media;

namespace Tailviewer.Ui
{
	public interface IMainPanelEntry
	{
		string Title { get; }
		string Id { get; }
		string ToolTip { get; }
		Geometry Icon { get; }
	}
}