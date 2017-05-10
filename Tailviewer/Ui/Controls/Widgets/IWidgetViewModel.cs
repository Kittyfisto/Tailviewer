using System.ComponentModel;

namespace Tailviewer.Ui.Controls.Widgets
{
	public interface IWidgetViewModel
		: INotifyPropertyChanged
	{
		/// <summary>
		/// 
		/// </summary>
		bool IsEditing { get; set; }
	}
}