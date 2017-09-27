using System;
using System.Windows.Input;

namespace Tailviewer
{
	/// <summary>
	///     A simple <see cref="ICommand" /> implementation which forwards command invocations
	///     to the action given during construction. <see cref="CanBeExecuted" /> controls whether
	///     or not the command can be executed and is set to true by default.
	/// </summary>
	/// <remarks>
	///     TODO: Move to Metrolib and declare DelegateCommand obsolete
	/// </remarks>
	public sealed class DelegateCommand2
		: ICommand
	{
		private readonly Action _fn;
		private bool _canBeExecuted;

		public DelegateCommand2(Action fn)
		{
			if (fn == null)
				throw new ArgumentNullException(nameof(fn));

			_fn = fn;
			_canBeExecuted = true;
		}

		public bool CanBeExecuted
		{
			get { return _canBeExecuted; }
			set
			{
				if (value == _canBeExecuted)
					return;

				_canBeExecuted = value;
				EmitCanExecuteChanged();
			}
		}

		public bool CanExecute(object parameter)
		{
			return _canBeExecuted;
		}

		public void Execute(object parameter)
		{
			_fn();
		}

		public event EventHandler CanExecuteChanged;

		private void EmitCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}