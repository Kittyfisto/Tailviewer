using System;

namespace Tailviewer.Ui.ViewModels.ContextMenu
{
	/// <summary>
	///     Toggles a boolean value between true and false when the user clicks on the corresponding menu item.
	/// </summary>
	public sealed class ToggleMenuViewModel
		: AbstractMenuViewModel
	{
		public ToggleMenuViewModel(bool isChecked, Action<bool> onIsCheckedChanged)
		{
			IsChecked = isChecked;
			IsCheckedChanged += onIsCheckedChanged;
		}

		public override bool IsCheckable
		{
			get { return true; }
		}
	}
}