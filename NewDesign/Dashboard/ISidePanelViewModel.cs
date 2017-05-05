using System.ComponentModel;
using System.Windows.Media;

namespace NewDesign.Dashboard
{
	public interface ISidePanelViewModel
		: INotifyPropertyChanged
	{
		Geometry Icon { get; }
		bool IsSelected { get; set; }
	}
}