using System;
using System.Windows.Input;

namespace Tailviewer.Ui.ViewModels.ActionCenter
{
	public interface INotificationViewModel
	{
		event Action<INotificationViewModel> OnRemove;
		ICommand RemoveCommand { get; }
		string Title { get; }
		bool IsRead { get; }
	}
}