using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Tailviewer.BusinessLogic.Searches;

namespace Tailviewer.Ui.LogView
{
	public interface IFindAllViewModel
		: INotifyPropertyChanged
	{
		IEnumerable<LogLineIndex> SelectedLogLines { get; set; }
		ILogSource LogSource { get; }
		ILogSourceSearch Search { get; }
		string SearchTerm { get; set; }
		bool Show { get; }
		string ErrorMessage { get; }
		bool IsEmpty { get; }
		ICommand CloseCommand { get; }
	}
}