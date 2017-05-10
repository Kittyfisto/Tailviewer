using System.Collections.Generic;
using System.ComponentModel;

namespace Tailviewer.Ui.Controls.Widgets
{
	public interface IWidgetLayoutViewModel
		: INotifyPropertyChanged
	{
		IEnumerable<IWidgetViewModel> Widgets { get; }
	}
}