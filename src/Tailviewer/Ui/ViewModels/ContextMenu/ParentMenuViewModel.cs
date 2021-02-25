using System.Collections.Generic;

namespace Tailviewer.Ui.ViewModels.ContextMenu
{
	/// <summary>
	///     A menu view model which holds a list of children.
	/// </summary>
	public sealed class ParentMenuViewModel
		: AbstractMenuViewModel
	{
		public ParentMenuViewModel(IEnumerable<IMenuViewModel> children)
		{
			Children = children;
		}

		public override bool IsCheckable
		{
			get { return false; }
		}
	}
}