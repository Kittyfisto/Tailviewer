using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tailviewer.Core.Analysis;
using Tailviewer.Ui.Analysis;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts
{
	public abstract class AbstractWidgetLayoutViewModel
		: IWidgetLayoutViewModel
	{
		private readonly ObservableCollection<WidgetViewModelProxy> _widgets;

		protected AbstractWidgetLayoutViewModel()
		{
			_widgets = new ObservableCollection<WidgetViewModelProxy>();
		}

		public abstract IWidgetLayoutTemplate Template { get; }

		public void Add(WidgetViewModelProxy widget)
		{
			_widgets.Add(widget);
		}

		public void Remove(WidgetViewModelProxy widget)
		{
			_widgets.Remove(widget);
		}

		public IEnumerable<WidgetViewModelProxy> Widgets => _widgets;

		public event Action<IWidgetPlugin> RequestAdd;

		public event PropertyChangedEventHandler PropertyChanged;

		protected void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void RaiseRequestAdd(IWidgetPlugin obj)
		{
			RequestAdd?.Invoke(obj);
		}
	}
}