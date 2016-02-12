using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Tailviewer.Ui.Controls;

namespace Tailviewer.Ui
{
	public static class DragLayer
	{
		private static DataSourceDragAdorner _itemAdorner;
		private static MainWindow _mainWindow;
		private static Point _dragStartPosition;
		private static AdornerLayer _adornerLayer;

		public static MainWindow MainWindow
		{
			get { return _mainWindow; }
			set
			{
				_mainWindow = value;
				_adornerLayer = value.PART_DragDecorator.AdornerLayer;
			}
		}

		public static void OnMouseMove(MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Released)
			{
				_dragStartPosition = e.GetPosition(_mainWindow);
			}
		}

		public static void UpdateAdornerPosition(DragEventArgs e)
		{
			if (_itemAdorner != null)
			{
				var position = e.GetPosition(_mainWindow);
				_itemAdorner.UpdatePosition(position.X, position.Y);
			}
		}

		public static bool ShouldStartDrag(MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && _itemAdorner == null)
			{
				Point position = e.GetPosition(_mainWindow);
				if ((Math.Abs(position.X - _dragStartPosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
					Math.Abs(position.Y - _dragStartPosition.Y) > SystemParameters.MinimumVerticalDragDistance))
				{
					return true;
				}
			}

			return false;
		}

		public static void DoDragDrop(object dragData, UIElement dragElement, DragDropEffects effects)
		{
			_itemAdorner = new DataSourceDragAdorner(dragData,
			                                         null,
			                                         dragElement,
			                                         _adornerLayer);
			try
			{
				_itemAdorner.UpdatePosition(_dragStartPosition.X, _dragStartPosition.Y);
				DragDropEffects de = DragDrop.DoDragDrop(dragElement, dragData, effects |
				                                                                DragDropEffects.None);
			}
			finally
			{
				if (_itemAdorner != null)
				{
					_itemAdorner.Destroy();
					_itemAdorner = null;
				}
			}
		}
	}
}