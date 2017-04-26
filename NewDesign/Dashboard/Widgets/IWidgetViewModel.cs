using System.ComponentModel;

namespace NewDesign.Dashboard.Widgets
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