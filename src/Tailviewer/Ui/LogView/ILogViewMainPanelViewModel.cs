using System.Windows.Input;

namespace Tailviewer.Ui.LogView
{
	public interface ILogViewMainPanelViewModel
		: IMainPanelViewModel
	{
		ICommand AddBookmarkCommand { get; }
		ICommand RemoveAllBookmarkCommand { get; }
	}
}