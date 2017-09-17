using System.ComponentModel;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets
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