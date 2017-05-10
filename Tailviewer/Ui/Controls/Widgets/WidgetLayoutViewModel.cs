using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tailviewer.Ui.Controls.Widgets
{
	public abstract class WidgetLayoutViewModel
		: IWidgetLayoutViewModel
	{
		private readonly ObservableCollection<IWidgetViewModel> _widgets;

		protected WidgetLayoutViewModel()
		{
			_widgets = new ObservableCollection<IWidgetViewModel>();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ICollection<IWidgetViewModel> Widgets => _widgets;

		IEnumerable<IWidgetViewModel> IWidgetLayoutViewModel.Widgets => _widgets;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}