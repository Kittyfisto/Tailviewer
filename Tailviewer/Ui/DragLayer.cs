using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Tailviewer.Ui.Controls;
using Tailviewer.Ui.Controls.DataSourceTree;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui
{
	internal static class DragLayer
	{
		private static DataSourceDragAdorner _dragAdorner;
		private static MainWindow _mainWindow;
		private static Point _dragStartPosition;
		private static AdornerLayer _adornerLayer;
		private static DataSourceDropAdorner _dropAdorner;

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
			if (_dragAdorner != null)
			{
				var position = e.GetPosition(_mainWindow);
				_dragAdorner.UpdatePosition(position.X, position.Y);
			}
		}

		public static bool ShouldStartDrag(MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && _dragAdorner == null)
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
			_dragAdorner = new DataSourceDragAdorner(dragData,
			                                         null,
			                                         dragElement,
			                                         _adornerLayer);
			try
			{
				_dragAdorner.UpdatePosition(_dragStartPosition.X, _dragStartPosition.Y);
				DragDropEffects de = DragDrop.DoDragDrop(dragElement, dragData, effects |
				                                                                DragDropEffects.None);
			}
			finally
			{
				if (_dragAdorner != null)
				{
					_dragAdorner.Destroy();
					_dragAdorner = null;
				}
			}
		}

		public static void AdornDropTarget(TreeViewItem treeViewItem, IDataSourceViewModel dest, DataSourceDropType type)
		{
			if (treeViewItem != null)
			{
				if (_dropAdorner == null)
				{
					AddDropAdorner(treeViewItem, dest, type);
				}
				else if (_dropAdorner.DataSource != dest)
				{
					RemoveDropAdorner();
					AddDropAdorner(treeViewItem, dest, type);
				}
				else
				{
					_dropAdorner.Type = type;
				}
			}
			else
			{
				if (_dropAdorner != null)
				{
					RemoveDropAdorner();
				}
			}
		}

		private static void AddDropAdorner(TreeViewItem treeViewItem, IDataSourceViewModel dest, DataSourceDropType type)
		{
			_dropAdorner = new DataSourceDropAdorner(treeViewItem, dest, type);
			_adornerLayer.Add(_dropAdorner);
		}

		private static void RemoveDropAdorner()
		{
				_adornerLayer.Remove(_dropAdorner);
				_dropAdorner = null;
		}
	}
}