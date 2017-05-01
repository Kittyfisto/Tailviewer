using System.ComponentModel;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls.SidePanel
{
	/// <summary>
	/// 
	/// </summary>
	public interface ISidePanelViewModel
		: INotifyPropertyChanged
	{
		/// <summary>
		/// 
		/// </summary>
		Geometry Icon { get; }

		/// <summary>
		/// 
		/// </summary>
		bool IsSelected { get; set; }

		/// <summary>
		/// 
		/// </summary>
		string Id { get; }
	}
}