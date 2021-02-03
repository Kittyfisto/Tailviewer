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
using Metrolib.Controls;
using Tailviewer.Ui.ViewModels;
using log4net;
using Tailviewer.Ui.Controls.SidePanel.DataSources;

namespace Tailviewer.Ui.Controls.DataSourceTree
{
	[TemplatePart(Name = PART_DataSources, Type = typeof (TreeView))]
	[TemplatePart(Name = PART_DataSourceSearch, Type = typeof (FilterTextBox))]
	public class DataSourcesControl : Control
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
			DependencyProperty.Register("SearchTerm", typeof (string), typeof (LogView.LogViewerControl),
			                            new PropertyMetadata(null, OnStringFilterChanged));

		public static readonly DependencyProperty AddDataSourceFromFileCommandProperty =
			DependencyProperty.Register("AddDataSourceFromFileCommand", typeof (ICommand), typeof (DataSourcesControl),
			                            new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty AddDataSourceFromFolderCommandProperty = DependencyProperty.Register(
		                                                "AddDataSourceFromFolderCommand", typeof(ICommand), typeof(DataSourcesControl), new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty AddCustomDataSourceCommandProperty = DependencyProperty.Register(
		                                                                                                               "AddCustomDataSourceCommand", typeof(ICommand), typeof(DataSourcesControl), new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty CustomDataSourcesProperty = DependencyProperty.Register(
		                                                                                                           "CustomDataSources", typeof(IEnumerable<AddCustomDataSourceViewModel>), typeof(DataSourcesControl), new PropertyMetadata(default(IEnumerable<AddCustomDataSourceViewModel>)));

		public static DataSourcesControl Instance;

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
			Instance = this;
		}

		public ICommand AddDataSourceFromFileCommand
		{
			get { return (ICommand) GetValue(AddDataSourceFromFileCommandProperty); }
			set { SetValue(AddDataSourceFromFileCommandProperty, value); }
		}

		public ICommand AddDataSourceFromFolderCommand
		{
			get { return (ICommand) GetValue(AddDataSourceFromFolderCommandProperty); }
			set { SetValue(AddDataSourceFromFolderCommandProperty, value); }
		}

		public ICommand AddCustomDataSourceCommand
		{
			get { return (ICommand) GetValue(AddCustomDataSourceCommandProperty); }
			set { SetValue(AddCustomDataSourceCommandProperty, value); }
		}

		public IEnumerable<AddCustomDataSourceViewModel> CustomDataSources
		{
			get { return (IEnumerable<AddCustomDataSourceViewModel>) GetValue(CustomDataSourcesProperty); }
			set { SetValue(CustomDataSourcesProperty, value); }
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

		private static void OnSelectedItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			((DataSourcesControl) dependencyObject).OnSelectedItemChanged((IDataSourceViewModel) e.NewValue);
		}

		private void OnSelectedItemChanged(IDataSourceViewModel selectedViewModel)
		{
			TreeViewItem treeViewItem = GetTreeViewItem(selectedViewModel);
			if (treeViewItem != null)
			{
				SelectItem(treeViewItem);
			}
			else
			{
				Dispatcher.BeginInvoke(new Action(() =>
					{
						TreeViewItem item = GetTreeViewItem(selectedViewModel);
						if (item != null)
						{
							SelectItem(item);
						}
					}));
			}
		}

		private void SelectItem(TreeViewItem treeViewItem)
		{
			treeViewItem.IsSelected = true;
			treeViewItem.BringIntoView();
		}

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
			_partDataSources.MouseMove += PartDataSourcesOnMouseMove;
			_partDataSources.DragOver += PartDataSourcesOnDragOver;
			_partDataSources.DragEnter += PartDataSourcesOnDragEnter;
			_partDataSources.DragLeave += PartDataSourcesOnDragLeave;
			_partDataSources.Drop += PartDataSourcesOnDrop;
		}

		private bool IsValidDrop(DragEventArgs e, out DropInfo dropInfo)
		{
			dropInfo = null;
			IDataSourceViewModel viewModel = e.Data.GetData(typeof (FileDataSourceViewModel)) as IDataSourceViewModel ??
			                                 e.Data.GetData(typeof (MergedDataSourceViewModel)) as IDataSourceViewModel;
			var source = new TreeItem
				{
					ViewModel = viewModel
				};
			if (source.ViewModel == null)
				return false;

			TreeItem dropTarget = GetDataSourceFromPosition(e.GetPosition(_partDataSources));
			if (dropTarget == null)
				return false;

			DataSourceDropType dropType = GetDropType(e, dropTarget, viewModel);
			var model = (DataSourcesViewModel) DataContext;
			IDataSourceViewModel group;
			if (!model.CanBeDropped(source.ViewModel, dropTarget.ViewModel, dropType, out group))
				return false;

			dropInfo = new DropInfo
				{
					Source = source,
					Type = dropType,
					Target = dropTarget,
					TargetGroup = new TreeItem
						{
							ViewModel = group,
							TreeViewItem = GetTreeViewItem(group)
						}
				};
			return true;
		}

		private DataSourceDropType GetDropType(DragEventArgs e,
		                                       TreeItem destination,
		                                       IDataSourceViewModel source)
		{
			if (destination == null)
				return DataSourceDropType.None;

			Point pos = e.GetPosition(destination.TreeViewItem);

			var dropType = DataSourceDropType.None;
			double height = destination.TreeViewItem.ActualHeight;
			if (source is FileDataSourceViewModel)
			{
				// Let's distribute it as follows:
				// 20% top => arrange
				// 60% middle => group
				// 20% bottom => arrange
				//
				// This way 60% of the height is used for grouping and 40% is used for arranging.
				if (pos.Y < height*0.2)
					dropType |= DataSourceDropType.ArrangeTop;
				else if (pos.Y < height*0.8)
					dropType |= DataSourceDropType.Group;
				else
					dropType |= DataSourceDropType.ArrangeBottom;

				if (destination.ViewModel.Parent != null)
					dropType |= DataSourceDropType.Group;
			}
			else
			{
				// Groups can't be grouped any further, thus we
				// simply arrange it above or beneath the drop source
				// at a 50/50 split.
				if (pos.Y < height*0.5)
					dropType |= DataSourceDropType.ArrangeTop;
				else
					dropType |= DataSourceDropType.ArrangeBottom;
			}

			return dropType;
		}

		public void PartDataSourcesOnDrop(object sender, DragEventArgs e)
		{
			DropInfo dropInfo;
			if (IsValidDrop(e, out dropInfo))
			{
				var vm = DataContext as DataSourcesViewModel;
				vm?.OnDropped(dropInfo.Source.ViewModel,
					dropInfo.Target.ViewModel,
					dropInfo.Type);

				e.Handled = true;
			}
			DragLayer.RemoveArrangeAdorner();
			DragLayer.RemoveDropAdorner();
		}

		private void PartDataSourcesOnDragEnter(object sender, DragEventArgs e)
		{
			HandleDrag(e);
		}

		private void PartDataSourcesOnDragLeave(object sender, DragEventArgs e)
		{
			HandleDrag(e);
		}

		private void PartDataSourcesOnDragOver(object sender, DragEventArgs e)
		{
			HandleDrag(e);
		}

		public void HandleDrag(DragEventArgs e)
		{
			DragLayer.UpdateAdornerPosition(e);

			DropInfo dropInfo;
			if (IsValidDrop(e, out dropInfo))
			{
				DragLayer.AdornDropTarget(dropInfo);
				e.Effects = DragDropEffects.Move;
				e.Handled = true;
			}
		}

		private TreeItem GetDataSourceFromPosition(Point position)
		{
			TreeViewItem treeViewItem = GetTreeViewItemFromPosition(position);
			if (treeViewItem == null)
			{
				// I call bullshit, for whatever reason you can find that perfect position
				// that is neither on the top item, nor on the bottom item.
				// Let's try again at a different spot
				treeViewItem = GetTreeViewItemFromPosition(new Point(position.X, position.Y + 1));
				if (treeViewItem == null)
					return null;
			}

			var model = treeViewItem.DataContext as IDataSourceViewModel;
			return new TreeItem
				{
					ViewModel = model,
					TreeViewItem = treeViewItem
				};
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

		private void PartDataSourcesOnMouseMove(object sender, MouseEventArgs e)
		{
			if (DragLayer.ShouldStartDrag(e))
			{
				IDataSourceViewModel source = SelectedItem;
				TreeViewItem treeViewItem = SelectedTreeViewItem;

				if (treeViewItem.IsMouseOver && ((DataSourcesViewModel) DataContext).CanBeDragged(source))
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
		///     Yes, please don't make dealing with a tree complicated or anything...
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private TreeViewItem GetTreeViewItem(IDataSourceViewModel item)
		{
			var partDataSources = _partDataSources;
			if (partDataSources == null)
			{
				Log.Warn("Unable to get the selected tree view item: _partDataSources is not set!");
				return null;
			}

			ItemContainerGenerator containerGenerator = partDataSources.ItemContainerGenerator;
			return GetTreeViewItem(containerGenerator, item);
		}

		private TreeViewItem GetTreeViewItem(ItemContainerGenerator containerGenerator, IDataSourceViewModel item)
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

					/*case NotifyCollectionChangedAction.Reset:
					foreach (IDataSourceViewModel model in _dataSourceCopy)
					{
						model.PropertyChanged -= DataSourceOnPropertyChanged;
					}
					_dataSourceCopy.Clear();
					FilteredItemsSource.Clear();
					break;*/

				default:
					throw new NotImplementedException();
			}
		}

		private int FindFilteredIndex(int sourceIndex)
		{
			for (int i = sourceIndex - 1; i >= 0; --i)
			{
				IDataSourceViewModel previousModel = ItemsSource.ElementAt(i);
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

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.IsDown)
			{
				switch (e.Key)
				{
					case Key.Down:
						break;
					case Key.Up:
						break;
				}
			}
		}

		private void SelectNextDataSource()
		{
			if (SelectedItem == null)
			{
				SelectedItem = ItemsSource.FirstOrDefault();
			}
			else
			{
				int index = ItemsSource.IndexOf(SelectedItem);
				int nextIndex = (index + 1) % ItemsSource.Count();
				SelectedItem = ItemsSource.ElementAt(nextIndex);
			}
		}

		private void SelectPreviousDataSource()
		{
			if (SelectedItem == null)
			{
				SelectedItem = ItemsSource.LastOrDefault();
			}
			else
			{
				int index = ItemsSource.IndexOf(SelectedItem);
				int nextIndex = index - 1;
				if (nextIndex < 0)
					nextIndex = ItemsSource.Count() - 1;
				SelectedItem = ItemsSource.ElementAt(nextIndex);
			}
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

		public void FocusSearch()
		{
			FilterTextBox element = _partDataSourceSearch;
			element?.Focus();
		}

		public static UIElement GetItemContainerFromItemsControl(ItemsControl itemsControl)
		{
			UIElement container;
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