using System.Windows;
using System.Windows.Controls;
using Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.Layouts
{
	public sealed class HorizontalWidgetLayoutControl
		: ItemsControl
	{
		public HorizontalWidgetLayoutControl()
		{
			DragEnter += OnDragEnter;
			Drop += OnDrop;
		}

		private void OnDrop(object sender, DragEventArgs e)
		{
			HandleDrop(e);
		}

		private void OnDragEnter(object sender, DragEventArgs e)
		{
			HandleDrag(e);
		}

		private void HandleDrop(DragEventArgs e)
		{
			var viewModel = e.Data.GetData(typeof(WidgetFactoryViewModel)) as WidgetFactoryViewModel;
			if (viewModel != null)
			{
				var dataContext = DataContext as HorizontalWidgetLayoutViewModel;
				dataContext?.RaiseRequestAdd(viewModel.Plugin);
			}
		}

		private void HandleDrag(DragEventArgs e)
		{
			var factory = e.Data.GetData(typeof(WidgetFactoryViewModel)) as WidgetFactoryViewModel;
			if (factory != null)
			{
				e.Effects = DragDropEffects.Copy;
			}
		}
	}
}