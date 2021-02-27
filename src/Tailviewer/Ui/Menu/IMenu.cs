using System.Collections.Generic;

namespace Tailviewer.Ui.Menu
{
	public interface IMenu
	{
		/// <summary>
		///     The items which make up the menu.
		/// </summary>
		IEnumerable<IMenuViewModel> Items { get; }
	}
}