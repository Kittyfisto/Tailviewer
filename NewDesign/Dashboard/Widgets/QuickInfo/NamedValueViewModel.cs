using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NewDesign.Dashboard.Widgets.QuickInfo
{
	public sealed class NamedValueViewModel
		: INamedValueViewModel
	{
		private object _value;
		private string _name;

		public NamedValueViewModel(string name, object value = null)
		{
			Name = name;
			Value = value;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string Name
		{
			get { return _name; }
			set
			{
				if (value == _name)
					return;

				_name = value;
				EmitPropertyChanged();
			}
		}

		public object Value
		{
			get { return _value; }
			set
			{
				if (value == _value)
					return;

				_value = value;
				EmitPropertyChanged();
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}