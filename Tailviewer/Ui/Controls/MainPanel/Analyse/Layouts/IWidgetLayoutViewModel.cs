using System.ComponentModel;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts
{
	/// <summary>
	///     Responsible for controlling *how* a list of widgets is displayed.
	/// </summary>
	public interface IWidgetLayoutViewModel
		: INotifyPropertyChanged
	{
		void Add(IWidgetViewModel widget);
		void Remove(IWidgetViewModel widget);
	}
}