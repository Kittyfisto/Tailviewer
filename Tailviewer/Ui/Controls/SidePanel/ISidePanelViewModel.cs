using System.ComponentModel;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls.SidePanel
{
	/// <summary>
	/// </summary>
	public interface ISidePanelViewModel
		: INotifyPropertyChanged
	{
		/// <summary>
		/// </summary>
		Geometry Icon { get; }

		/// <summary>
		/// </summary>
		bool IsSelected { get; set; }

		/// <summary>
		/// </summary>
		string Id { get; }

		/// <summary>
		///     A short text that shall be displayed beneath the icon, if any.
		/// </summary>
		string QuickInfo { get; }
	}
}