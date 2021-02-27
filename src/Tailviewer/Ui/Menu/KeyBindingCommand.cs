using System;
using System.Text;
using System.Windows.Input;
using Metrolib;

namespace Tailviewer.Ui.Menu
{
	/// <summary>
	///     An <see cref="ICommand" /> which is bound to a shortcut.
	/// </summary>
	public sealed class KeyBindingCommand
		: ICommand
	{
		private readonly ICommand _command;

		public KeyBindingCommand(Action action)
			: this(new DelegateCommand2(action))
		{}

		public KeyBindingCommand(ICommand command)
		{
			_command = command;
			_command.CanExecuteChanged += CommandOnCanExecuteChanged;
		}

		private void CommandOnCanExecuteChanged(object sender, EventArgs e)
		{
			EmitCanExecuteChanged(e);
		}
		
		public Key GestureKey { get; set; }
		public ModifierKeys GestureModifier { get; set; }

		#region Overrides of Object

		public override string ToString()
		{
			var builder = new StringBuilder();
			void AddPlus()
			{
				if (builder.Length > 0)
					builder.Append("+");
			};

			if (GestureModifier != ModifierKeys.None)
			{
				if ((GestureModifier & ModifierKeys.Control) == ModifierKeys.Control)
				{
					AddPlus();
					builder.Append("Ctrl");
				}

				if ((GestureModifier & ModifierKeys.Shift) == ModifierKeys.Shift)
				{
					AddPlus();
					builder.Append("Shift");
				}

				if ((GestureModifier & ModifierKeys.Alt) == ModifierKeys.Alt)
				{
					AddPlus();
					builder.Append("Alt");
				}
			}

			AddPlus();
			builder.Append(GestureKey);

			return builder.ToString();
		}

		#endregion

		#region Implementation of ICommand

		public bool CanExecute(object parameter)
		{
			return _command.CanExecute(parameter);
		}

		public void Execute(object parameter)
		{
			_command.Execute(parameter);
		}

		public event EventHandler CanExecuteChanged;

		#endregion

		private void EmitCanExecuteChanged(EventArgs eventArgs)
		{
			CanExecuteChanged?.Invoke(this, eventArgs);
		}
	}
}