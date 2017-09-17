using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets
{
	public sealed class HorizontalWidgetLayoutViewModel
		: IWidgetLayoutViewModel
	{
		private readonly ObservableCollection<IWidgetViewModel> _widgets;

		public HorizontalWidgetLayoutViewModel()
		{
			_widgets = new ObservableCollection<IWidgetViewModel>();
		}

		public void Add(IWidgetViewModel widget)
		{
			_widgets.Add(widget);
		}

		public void Remove(IWidgetViewModel widget)
		{
			_widgets.Remove(widget);
		}

		public ICollection<IWidgetViewModel> Widgets => _widgets;

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}