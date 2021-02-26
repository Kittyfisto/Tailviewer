using System.ComponentModel;

namespace Tailviewer.Ui.LogView
{
	public interface ISearchViewModel
		: INotifyPropertyChanged
	{
		string Term { get; set; }
		int ResultCount { get; }
		int CurrentResultIndex { get; set; }
	}
}