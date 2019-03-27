using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NewDesign.Dashboard.Widgets.Events
{
	public sealed class EventViewModel
		: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public DateTime Timestamp { get; }
		public string Value { get; }

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}