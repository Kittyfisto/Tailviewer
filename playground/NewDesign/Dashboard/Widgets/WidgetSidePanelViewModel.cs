using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Metrolib;

namespace NewDesign.Dashboard.Widgets
{
	public sealed class WidgetSidePanelViewModel
		: ISidePanelViewModel
	{
		private bool _isSelected;
		public event PropertyChangedEventHandler PropertyChanged;

		public Geometry Icon => Icons.Widgets;

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value == _isSelected)
					return;

				_isSelected = value;
				EmitPropertyChanged();
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}