using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace Tailviewer.Ui.Menu
{
	public interface IMenuViewModel
	{
		/// <summary>
		/// 
		/// </summary>
		string Header { get; }

		/// <summary>
		/// 
		/// </summary>
		string ToolTip { get; }

		/// <summary>
		/// 
		/// </summary>
		string Shortcut { get; }

		/// <summary>
		///     The icon to display, if any.
		/// </summary>
		Geometry Icon { get; }

		ICommand Command { get; }

		/// <summary>
		///     When set to true, then clicking on the menu item bound to this view model will toggle the <see cref="IsChecked" /> property.
		///     When set to false, then clicking on the menu item bound to this view will instead invoke <see cref="ICommand.Execute"/>.
		/// </summary>
		bool IsCheckable { get; }

		/// <summary>
		///     When <see cref="IsCheckable" />, then this value may be toggled by the user.
		/// </summary>
		bool IsChecked { get; set; }

		/// <summary>
		///     The child menu items of this one, if any.
		/// </summary>
		IEnumerable<IMenuViewModel> Children { get; }
	}
}