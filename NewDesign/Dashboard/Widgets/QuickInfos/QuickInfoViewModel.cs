using System;
using System.Windows.Input;
using Metrolib;

namespace NewDesign.Dashboard.Widgets.QuickInfos
{
	public sealed class QuickInfoViewModel
		: NamedValueViewModel
	{
		private string _query;
		private readonly ICommand _removeCommand;

		public event Action<QuickInfoViewModel> OnRemove;

		public QuickInfoViewModel(string name, object value = null) : base(name, value)
		{
			_removeCommand = new DelegateCommand(Remove);
		}

		private void Remove()
		{
			OnRemove?.Invoke(this);
		}

		public string Query
		{
			get { return _query; }
			set
			{
				if (value == _query)
					return;

				_query = value;
				EmitPropertyChanged();
			}
		}

		public ICommand RemoveCommand
		{
			get { return _removeCommand; }
		}
	}
}