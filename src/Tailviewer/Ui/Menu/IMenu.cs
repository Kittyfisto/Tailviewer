using System.Collections.Generic;
using System.ComponentModel;

namespace Tailviewer.Ui.Menu
{
	public interface IMenu
		: INotifyPropertyChanged
	{
		/// <summary>
		///     The items which make up the menu.
		/// </summary>
		IEnumerable<IMenuViewModel> Items { get; }

		bool HasItems { get; }
	}
}