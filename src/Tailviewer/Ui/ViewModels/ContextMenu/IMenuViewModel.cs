using System.Windows.Input;
using System.Windows.Shapes;

namespace Tailviewer.Ui.ViewModels.ContextMenu
{
	public interface IMenuViewModel
	{
		string Header { get; }

		string ToolTip { get; }

		/// <summary>
		///     The icon to display, if any.
		/// </summary>
		Path Icon { get; }

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
	}
}