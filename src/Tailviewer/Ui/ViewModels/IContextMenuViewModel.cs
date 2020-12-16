using System.Windows.Input;

namespace Tailviewer.Ui.ViewModels
{
	public interface IContextMenuViewModel
	{
		string Header { get; }

		ICommand Command { get; }
	}
}