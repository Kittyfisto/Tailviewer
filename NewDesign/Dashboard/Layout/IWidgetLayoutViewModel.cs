using System.Collections.Generic;
using System.ComponentModel;
using NewDesign.Dashboard.Widgets;

namespace NewDesign.Dashboard.Layout
{
	public interface IWidgetLayoutViewModel
		: INotifyPropertyChanged
	{
		IEnumerable<IWidgetViewModel> Widgets { get; }
	}
}