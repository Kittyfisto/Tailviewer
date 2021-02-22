using System;
using System.Windows.Input;
using Metrolib;

namespace Tailviewer.Ui.ViewModels
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class AddCustomDataSourceViewModel
	{
		private readonly ICommand _command;
		private readonly string _name;

		public ICommand AddCommand => _command;

		public string Name => _name;

		public AddCustomDataSourceViewModel(string name, Action add)
		{
			_name = name;
			_command = new DelegateCommand2(add);
		}
	}
}