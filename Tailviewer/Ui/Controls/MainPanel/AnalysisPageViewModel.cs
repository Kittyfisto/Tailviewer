using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tailviewer.Ui.Controls.Widgets;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public sealed class AnalysisPageViewModel
		: INotifyPropertyChanged
	{
		private readonly ObservableCollection<WidgetLayoutViewModel> _layouts;

		public AnalysisPageViewModel()
		{
			_layouts = new ObservableCollection<WidgetLayoutViewModel>();
		}

		public IEnumerable<WidgetLayoutViewModel> Layouts => _layouts;

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}