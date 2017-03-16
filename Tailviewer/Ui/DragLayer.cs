using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Tailviewer.Ui.Controls.DataSourceTree;

namespace Tailviewer.Ui
{
	internal static class DragLayer
	{
		private static DataSourceDragAdorner _dragAdorner;
		private static Point _dragStartPosition;
		private static AdornerLayer _adornerLayer;
		private static DataSourceDropAdorner _dropAdorner;
		private static DataSourceArrangeAdorner _arrangeAdorner;

		public static AdornerLayer AdornerLayer
		{
			get { return _adornerLayer; }
			set { _adornerLayer = value; }
		}

		public static void OnMouseMove(MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Released)
			{
				_dragStartPosition = e.GetPosition(_adornerLayer);
			}
		}

		public static void UpdateAdornerPosition(DragEventArgs e)
		{
			if (_dragAdorner != null)
			{
				Point position = e.GetPosition(_adornerLayer);
				_dragAdorner.UpdatePosition(position.X, position.Y);
			}
		}

		public static bool ShouldStartDrag(MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && _dragAdorner == null)
			{
				Point position = e.GetPosition(_adornerLayer);
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

		public static void AdornDropTarget(DropInfo dropInfo)
		{
			if (dropInfo == null) throw new ArgumentNullException(nameof(dropInfo));
			if (dropInfo.Type == DataSourceDropType.None)
				throw new ArgumentException("dropInfo.DropType may not be set to none");

			if (dropInfo.Type.HasFlag(DataSourceDropType.Group))
			{
				if (dropInfo.TargetGroup == null)
					throw new ArgumentException("dropInfo.Group is set to null but the type is set to group");

				AddDropAdorner(dropInfo.TargetGroup.TreeViewItem);
			}
			else
			{
				RemoveDropAdorner();
			}

			if (dropInfo.Type.HasFlag(DataSourceDropType.ArrangeTop) ||
			    dropInfo.Type.HasFlag(DataSourceDropType.ArrangeBottom))
			{
				AddArrangeAdorner(dropInfo.Target.TreeViewItem, dropInfo.Type);
			}
			else
			{
				RemoveArrangeAdorner();
			}
		}

		public static void RemoveArrangeAdorner()
		{
			if (_arrangeAdorner != null)
			{
				_adornerLayer.Remove(_arrangeAdorner);
				_arrangeAdorner = null;
			}
		}

		public static void RemoveDropAdorner()
		{
			if (_dropAdorner != null)
			{
				_adornerLayer.Remove(_dropAdorner);
				_dropAdorner = null;
			}
		}

		private static void AddArrangeAdorner(TreeViewItem treeViewItem, DataSourceDropType dropType)
		{
			if (_arrangeAdorner == null)
			{
				_arrangeAdorner = new DataSourceArrangeAdorner(treeViewItem, dropType);
				_adornerLayer.Add(_arrangeAdorner);
			}
			else if (!Equals(_arrangeAdorner.AdornedElement, treeViewItem) ||
			         _arrangeAdorner.DropType != dropType)
			{
				RemoveArrangeAdorner();

				_arrangeAdorner = new DataSourceArrangeAdorner(treeViewItem, dropType);
				_adornerLayer.Add(_arrangeAdorner);
			}
		}

		private static void AddDropAdorner(TreeViewItem treeViewItem)
		{
			if (_dropAdorner == null)
			{
				_dropAdorner = new DataSourceDropAdorner(treeViewItem);
				_adornerLayer.Add(_dropAdorner);
			}
			else if (!Equals(_dropAdorner.AdornedElement, treeViewItem))
			{
				RemoveDropAdorner();

				_dropAdorner = new DataSourceDropAdorner(treeViewItem);
				_adornerLayer.Add(_dropAdorner);
			}
		}
	}
}