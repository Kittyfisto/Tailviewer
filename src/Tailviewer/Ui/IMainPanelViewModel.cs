using System.ComponentModel;
using Tailviewer.Ui.LogView;

namespace Tailviewer.Ui
{
	public interface IMainPanelViewModel
		: INotifyPropertyChanged
	{
		ISearchViewModel Search { get; }

		IFindAllViewModel FindAll { get; }

		void Update();
	}
}
