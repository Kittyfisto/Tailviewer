using System.Windows.Media;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public interface IMainPanelEntry
	{
		string Title { get; }
		string Id { get; }
		Geometry Icon { get; }
	}
}