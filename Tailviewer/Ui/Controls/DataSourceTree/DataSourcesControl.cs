using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.DataSourceTree
{
	[TemplatePart(Name = PART_DataSources, Type = typeof (TreeView))]
	[TemplatePart(Name = PART_DataSourceSearch, Type = typeof (FilterTextBox))]
	internal class DataSourcesControl : Control
	{
		public const string PART_DataSources = "PART_DataSources";
		public const string PART_DataSourceSearch = "PART_DataSourceSearch";

		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof (IEnumerable<IDataSourceViewModel>), typeof (DataSourcesControl),
			                            new PropertyMetadata(null, OnDataSourcesChanged));

		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof (IDataSourceViewModel), typeof (DataSourcesControl),
			                            new PropertyMetadata(null, OnSelectedItemChanged));

		private static readonly DependencyPropertyKey FilteredItemsSourcePropertyKey
			= DependencyProperty.RegisterReadOnly("FilteredItemsSource", typeof (ObservableCollection<IDataSourceViewModel>),
			                                      typeof (DataSourcesControl),
			                                      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

		public static readonly DependencyProperty FilteredItemsSourceProperty =
			FilteredItemsSourcePropertyKey.DependencyProperty;

		public static readonly DependencyProperty StringFilterProperty =
			DependencyProperty.Register("StringFilter", typeof (string), typeof (LogViewerControl),
			                            new PropertyMetadata(null, OnStringFilterChanged));

		private FilterTextBox _partDataSourceSearch;
		private TreeView _partDataSources;

		static DataSourcesControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (DataSourcesControl),
			                                         new FrameworkPropertyMetadata(typeof (DataSourcesControl)));
		}

		public DataSourcesControl()
		{
			FilteredItemsSource = new ObservableCollection<IDataSourceViewModel>();
		}

		public string StringFilter
		{
			get { return (string) GetValue(StringFilterProperty); }
			set
			{
				string old = StringFilter;
				SetValue(StringFilterProperty, value);
				OnStringFilterChanged(old, value);
			}
		}

		public ObservableCollection<IDataSourceViewModel> FilteredItemsSource
		{
			get { return (ObservableCollection<IDataSourceViewModel>) GetValue(FilteredItemsSourceProperty); }
			private set { SetValue(FilteredItemsSourcePropertyKey, value); }
		}

		public IDataSourceViewModel SelectedItem
		{
			get { return (IDataSourceViewModel) GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public IEnumerable<IDataSourceViewModel> ItemsSource
		{
			get { return (IEnumerable<IDataSourceViewModel>) GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		private TreeViewItem SelectedTreeViewItem { get; set; }

		private static void OnStringFilterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			((DataSourcesControl) dependencyObject).OnStringFilterChanged((string) e.OldValue, (string) e.NewValue);
		}

		private void OnStringFilterChanged(string oldValue, string newValue)
		{
			if (ItemsSource != null)
			{
				int sourceIndex = 0;
				int filterIndex = 0;
				foreach (IDataSourceViewModel model in ItemsSource)
				{
					bool passedOld = PassesFilter(model, oldValue);
					bool passesNew = PassesFilter(model, newValue);

					if (!passedOld)
					{
						if (passesNew)
						{
							FilteredItemsSource.Insert(filterIndex++, model);
						}
					}
					else if (!passesNew)
					{
						FilteredItemsSource.RemoveAt(filterIndex);
					}
					else
					{
						++filterIndex;
					}

					++sourceIndex;
				}
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_partDataSourceSearch = (FilterTextBox) GetTemplateChild(PART_DataSourceSearch);
			_partDataSources = (TreeView) GetTemplateChild(PART_DataSources);
			_partDataSources.AllowDrop = true;
			_partDataSources.SelectedItemChanged += PartDataSourcesOnSelectedItemChanged;
			_partDataSources.PreviewMouseMove += PartDataSourcesOnPreviewMouseMove;
			_partDataSources.DragOver += PartDataSourcesOnDragOver;
			_partDataSources.DragEnter += PartDataSourcesOnDragEnter;
			_partDataSources.DragLeave += PartDataSourcesOnDragLeave;
			_partDataSources.Drop += PartDataSourcesOnDrop;
		}

		private bool IsValidDrop(DragEventArgs e,
			out IDataSourceViewModel source,
			out IDataSourceViewModel dest,
			out TreeViewItem destItem,
			out DataSourceDropType dropType,
			out IDataSourceViewModel finalDest)
		{
			source = e.Data.GetData(typeof (SingleDataSourceViewModel)) as IDataSourceViewModel;
			dest = GetDataSourceFromPosition(e.GetPosition(_partDataSources), out destItem);

			// Let's find out if this is a "rearrange"- or a "group" drop.
			dropType = GetDropType(e, destItem);

			var model = (MainWindowViewModel) DataContext;
			return model.CanBeDropped(source, dest, dropType, out finalDest);
		}

		private DataSourceDropType GetDropType(DragEventArgs e, TreeViewItem destItem)
		{
			if (destItem == null)
				return DataSourceDropType.None;

			var pos = e.GetPosition(destItem);
			// Let's distribute it as follows:
			// 20% top => arrange
			// 60% middle => group
			// 20% bottom => arrange
			//
			// This way 60% of the height is used for grouping and 40% is used for arranging.

			double height = destItem.ActualHeight;
			if (pos.Y < height*0.2)
				return DataSourceDropType.ArrangeTop;

			if (pos.Y < height*0.8)
				return DataSourceDropType.Group;

			return DataSourceDropType.ArrangeBottom;
		}

		private void PartDataSourcesOnDrop(object sender, DragEventArgs e)
		{
			IDataSourceViewModel source, dest, unused2;
			DataSourceDropType dropType;
			TreeViewItem unused1;
			if (IsValidDrop(e, out source, out dest, out unused1, out dropType, out unused2))
			{
				var vm = DataContext as MainWindowViewModel;
				if (vm != null)
				{
					vm.OnDropped(source, dest, dropType);
				}

				e.Handled = true;
			}
			DragLayer.AdornDropTarget(null, null, DataSourceDropType.None);
		}

		private void PartDataSourcesOnDragEnter(object sender, DragEventArgs e)
		{
			HandleDrag(e);
		}

		private void PartDataSourcesOnDragLeave(object sender, DragEventArgs e)
		{
			DragLayer.AdornDropTarget(null, null, DataSourceDropType.None);
		}

		private void PartDataSourcesOnDragOver(object sender, DragEventArgs e)
		{
			HandleDrag(e);
		}

		public void HandleDrag(DragEventArgs e)
		{
			DragLayer.UpdateAdornerPosition(e);

			IDataSourceViewModel source, dest, finalDest;
			TreeViewItem destItem;
			DataSourceDropType dropType;
			if (IsValidDrop(e, out source, out dest, out destItem, out dropType, out finalDest))
			{
				var finalDestItem = GetTreeViewItem(finalDest);
				DragLayer.AdornDropTarget(finalDestItem, finalDest, dropType);
				e.Effects = DragDropEffects.Move;
			}
			else
			{
				DragLayer.AdornDropTarget(null, null, DataSourceDropType.None);
				e.Effects = DragDropEffects.None;
			}
			e.Handled = true;
		}

		private IDataSourceViewModel GetDataSourceFromPosition(Point position, out TreeViewItem treeViewItem)
		{
			treeViewItem = GetTreeViewItemFromPosition(position);
			if (treeViewItem == null)
				return null;

			var model = treeViewItem.DataContext as IDataSourceViewModel;
			return model;
		}

		private TreeViewItem GetTreeViewItemFromPosition(Point position)
		{
			var element = _partDataSources.InputHitTest(position) as DependencyObject;
			while (element != null)
			{
				element = VisualTreeHelper.GetParent(element);
				var treeViewItem = element as TreeViewItem;
				if (treeViewItem != null)
					return treeViewItem;
			}

			return null;
		}

		private void PartDataSourcesOnPreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (DragLayer.ShouldStartDrag(e))
			{
				var source = SelectedItem;
				var treeViewItem = SelectedTreeViewItem;
				if (((MainWindowViewModel) DataContext).CanBeDragged(source))
				{
					DragLayer.DoDragDrop(source, treeViewItem, DragDropEffects.Move);
				}
			}
		}

		private void PartDataSourcesOnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> args)
		{
			SelectedItem = args.NewValue as IDataSourceViewModel;
			SelectedTreeViewItem = GetTreeViewItem(SelectedItem);
		}

		/// <summary>
		/// Yes, please don't make dealing with a tree complicated or anything...
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private TreeViewItem GetTreeViewItem(object item)
		{
			var containerGenerator = _partDataSources.ItemContainerGenerator;
			return GetTreeViewItem(containerGenerator, item);
		}

		private TreeViewItem GetTreeViewItem( ItemContainerGenerator containerGenerator, object item)
		{
			var container = (TreeViewItem) containerGenerator.ContainerFromItem(item);
			if (container != null)
				return container;

			foreach (object childItem in containerGenerator.Items)
			{
				var parent = containerGenerator.ContainerFromItem(childItem) as TreeViewItem;
				if (parent == null)
					continue;

				container = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
				if (container != null)
					return container;

				container = GetTreeViewItem(parent.ItemContainerGenerator, item);
				if (container != null)
					return container;
			}
			return null;
		}

		private static void OnDataSourcesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((DataSourcesControl) d).OnDataSourcesChanged((IEnumerable<IDataSourceViewModel>) e.OldValue,
			                                              (IEnumerable<IDataSourceViewModel>) e.NewValue);
		}

		private void OnDataSourcesChanged(IEnumerable<IDataSourceViewModel> oldValue,
		                                  IEnumerable<IDataSourceViewModel> newValue)
		{
			var old = oldValue as INotifyCollectionChanged;
			if (old != null)
			{
				old.CollectionChanged -= ItemsSourceOnCollectionChanged;
			}

			FilteredItemsSource.Clear();
			if (newValue != null)
			{
				foreach (IDataSourceViewModel model in newValue)
				{
					if (PassesFilter(model))
					{
						FilteredItemsSource.Add(model);
						if (model.IsOpen)
						{
							Dispatcher.BeginInvoke(new Action(() => { SelectedItem = model; }));
						}
					}
				}

				var @new = newValue as INotifyCollectionChanged;
				if (@new != null)
				{
					@new.CollectionChanged += ItemsSourceOnCollectionChanged;
				}
			}
		}

		private void ItemsSourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					for (int i = 0; i < e.NewItems.Count; ++i)
					{
						var model = (IDataSourceViewModel) e.NewItems[i];
						if (PassesFilter(model))
						{
							int sourceIndex = e.NewStartingIndex + i;
							int filteredIndex = FindFilteredIndex(sourceIndex);
							FilteredItemsSource.Insert(filteredIndex, model);
						}
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					foreach (IDataSourceViewModel model in e.OldItems)
					{
						FilteredItemsSource.Remove(model);
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					FilteredItemsSource.Clear();
					break;

				default:
					throw new NotImplementedException();
			}
		}

		private int FindFilteredIndex(int sourceIndex)
		{
			for (int i = sourceIndex-1; i >= 0; --i)
			{
				var previousModel = ItemsSource.ElementAt(i);
				if (PassesFilter(previousModel))
				{
					// We found the next predecessor to the given source index that should be
					// before "sourceIndex"
					int filteredIndex = FilteredItemsSource.IndexOf(previousModel);
					int desiredIndex = filteredIndex + 1;
					return desiredIndex;
				}
			}

			return 0;
		}

		[Pure]
		private bool PassesFilter(IDataSourceViewModel model)
		{
			return PassesFilter(model, StringFilter);
		}

		[Pure]
		private bool PassesFilter(IDataSourceViewModel model, string filter)
		{
			if (filter == null)
				return true;

			int idx = model.DisplayName.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase);
			if (idx != -1)
				return true;

			return false;
		}

		private static void OnSelectedItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((DataSourcesControl) dependencyObject).OnSelectedItemChanged((IDataSourceViewModel) args.OldValue,
			                                                              (IDataSourceViewModel) args.NewValue);
		}

		private void OnSelectedItemChanged(IDataSourceViewModel oldValue, IDataSourceViewModel newValue)
		{
			if (oldValue != null)
			{
				oldValue.IsOpen = false;
				SetSelected(oldValue, false);
			}
			if (newValue != null)
			{
				newValue.IsOpen = true;
				SetSelected(newValue, true);
			}
		}

		private void SetSelected(IDataSourceViewModel newValue, bool isSelected)
		{
			// Because why wouldn't we want to make selecting an item in a treeview
			// a hassle?!

			if (_partDataSources == null)
				return;

			DependencyObject treeViewItem = GetTreeViewItem(newValue);
			if (treeViewItem == null)
				return;

			MethodInfo selectMethod =
				typeof (TreeViewItem).GetMethod("Select",
				                                BindingFlags.NonPublic | BindingFlags.Instance);
			selectMethod.Invoke(treeViewItem, new object[] {isSelected});
		}

		public void FocusSearch()
		{
			FilterTextBox element = _partDataSourceSearch;
			if (element != null)
			{
				element.Focus();
			}
		}

		public static UIElement GetItemContainerFromItemsControl(ItemsControl itemsControl)
		{
			UIElement container = null;
			if (itemsControl != null && itemsControl.Items.Count > 0)
			{
				container = itemsControl.ItemContainerGenerator.ContainerFromIndex(0) as UIElement;
			}
			else
			{
				container = itemsControl;
			}
			return container;
		}
	}
}