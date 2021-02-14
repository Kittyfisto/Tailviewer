using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Controls.SidePanel.Outline
{
	sealed class LogFilePropertyViewModel<T>
		: ILogFilePropertyViewModel
	{
		private readonly IReadOnlyPropertyDescriptor<T> _descriptor;
		private T _value;

		public LogFilePropertyViewModel(IReadOnlyPropertyDescriptor<T> descriptor)
		{
			_descriptor = descriptor;
		}

		public string Title => _descriptor.DisplayName;

		public object Value
		{
			get { return _value; }
		}

		public void Update(ILogFileProperties properties)
		{
			properties.TryGetValue(_descriptor, out var newValue);
			if (!Equals(newValue, _value))
			{
				_value = newValue;
				EmitPropertyChanged(nameof(Value));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}