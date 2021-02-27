using System.Windows.Input;

namespace Tailviewer.Ui.Menu
{
	/// <summary>
	///     Executes a particular command when the user clicks on the corresponding menu item.
	/// </summary>
	public sealed class CommandMenuViewModel
		: AbstractMenuViewModel
	{
		public CommandMenuViewModel(ICommand command)
		{
			Command = command;
		}

		public override bool IsCheckable
		{
			get { return false; }
		}
	}
}