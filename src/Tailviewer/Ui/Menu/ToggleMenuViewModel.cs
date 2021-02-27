using System;
using Metrolib;

namespace Tailviewer.Ui.Menu
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
			OnIsCheckedChanged(isChecked);
			IsCheckedChanged += onIsCheckedChanged;
			IsCheckedChanged += OnIsCheckedChanged;
		}

		private void OnIsCheckedChanged(bool value)
		{
			Icon = value ? Icons.Check : null;
		}

		public override bool IsCheckable
		{
			get { return true; }
		}
	}
}