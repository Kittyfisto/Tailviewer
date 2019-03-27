using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tailviewer.Ui.Controls.MainPanel.Analyse.SidePanels
{
	public sealed class WidgetsControl
		: ItemsControl
	{
		public WidgetsControl()
		{
			MouseMove += OnMouseMove;
			DragOver += OnDragOver;
			DragEnter += OnDragEnter;
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (DragLayer.ShouldStartDrag(e))
			{
				// Get the item under the mouse
				var relativePosition = e.GetPosition(this);
				var element = InputHitTest(relativePosition) as FrameworkElement;
				var uiElement = element.FindFirstAncestorWithName("PART_WidgetFactory");
				var factory = uiElement?.DataContext as WidgetFactoryViewModel;
				if (factory != null)
				{
					DragLayer.DoDragDrop(factory, uiElement, DragDropEffects.Copy);
				}
			}
		}

		private void OnDragOver(object sender, DragEventArgs e)
		{
			// We do not allow any sort of drop here, we're just the start of the drag
			e.Effects = DragDropEffects.None;
		}

		private void OnDragEnter(object sender, DragEventArgs e)
		{
			// We do not allow any sort of drop here, we're just the start of the drag
			e.Effects = DragDropEffects.None;
		}
	}
}
