using System.ComponentModel;

namespace NewDesign.Dashboard
{
	public interface ISidePanelViewModel
		: INotifyPropertyChanged
	{
		bool IsSelected { get; set; }
	}
}