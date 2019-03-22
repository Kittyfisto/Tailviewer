using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tailviewer.Ui.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts
{
	public sealed class HorizontalWidgetLayoutViewModel
		: IWidgetLayoutViewModel
	{
		private readonly ObservableCollection<WidgetViewModelProxy> _widgets;

		public HorizontalWidgetLayoutViewModel()
		{
			_widgets = new ObservableCollection<WidgetViewModelProxy>();
		}

		public void Add(WidgetViewModelProxy widget)
		{
			_widgets.Add(widget);
		}

		public void Remove(WidgetViewModelProxy widget)
		{
			_widgets.Remove(widget);
		}

		public void RaiseRequestAdd(IWidgetPlugin plugin)
		{
			RequestAdd?.Invoke(plugin);
		}

		public event Action<IWidgetPlugin> RequestAdd;

		public ICollection<WidgetViewModelProxy> Widgets => _widgets;

		public event PropertyChangedEventHandler PropertyChanged;

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}