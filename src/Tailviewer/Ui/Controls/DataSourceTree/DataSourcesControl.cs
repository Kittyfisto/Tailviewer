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
using log4net;
using Metrolib.Controls;
using Tailviewer.Ui.Controls.LogView;
using Tailviewer.Ui.Controls.SidePanel.DataSources;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.DataSourceTree
{
	/// <summary>
	///     Allows the user to inspect and interact with the currently opened data sources, each represented by a
	///     <see cref="IDataSourceViewModel" />.
	/// </summary>
	[TemplatePart(Name = PART_DataSources, Type = typeof(TreeView))]
	[TemplatePart(Name = PART_DataSourceSearch, Type = typeof(FilterTextBox))]
	public class DataSourcesControl : Control
	{
		public const string PART_DataSources = "PART_DataSources";
		public const string PART_DataSourceSearch = "PART_DataSourceSearch";
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IEnumerable<IDataSourceViewModel>),
			                            typeof(DataSourcesControl),
			                            new PropertyMetadata(defaultValue: null, OnDataSourcesChanged));

		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof(IDataSourceViewModel), typeof(DataSourcesControl),
			                            new PropertyMetadata(defaultValue: null, OnSelectedItemChanged));

		private static readonly DependencyPropertyKey FilteredItemsSourcePropertyKey
			= DependencyProperty.RegisterReadOnly("FilteredItemsSource",
			                                      typeof(ObservableCollection<IDataSourceViewModel>),
			                                      typeof(DataSourcesControl),
			                                      new FrameworkPropertyMetadata(defaultValue: null,
				                                                                    FrameworkPropertyMetadataOptions
					                                                                    .None));

		public static readonly DependencyProperty FilteredItemsSourceProperty =
			FilteredItemsSourcePropertyKey.DependencyProperty;

		public static readonly DependencyProperty StringFilterProperty =
			DependencyProperty.Register("SearchTerm", typeof(string), typeof(LogViewerControl),
			                            new PropertyMetadata(defaultValue: null, OnStringFilterChanged));

		public static readonly DependencyProperty IsPinnedProperty = DependencyProperty.Register(
		 "IsPinned", typeof(bool), typeof(DataSourcesControl), new PropertyMetadata(default(bool)));

		private static readonly DependencyPropertyKey NoDataSourcesReasonPropertyKey
			= DependencyProperty.RegisterReadOnly("NoDataSourcesReason", typeof(string), typeof(DataSourcesControl),
			                                      new FrameworkPropertyMetadata(default(string),
				                                                                    FrameworkPropertyMetadataOptions
					                                                                    .None));

		private static readonly DependencyPropertyKey NoDataSourcesActionsPropertyKey
			= DependencyProperty.RegisterReadOnly("NoDataSourcesActions", typeof(string), typeof(DataSourcesControl),
			                                      new FrameworkPropertyMetadata(default(string),
				                                                                    FrameworkPropertyMetadataOptions
					                                                                    .None));

		public static readonly DependencyProperty NoDataSourcesActionsProperty
			= NoDataSourcesActionsPropertyKey.DependencyProperty;

		public static readonly DependencyProperty NoDataSourcesReasonProperty
			= NoDataSourcesReasonPropertyKey.DependencyProperty;

		public static DataSourcesControl Instance;
		private TreeView _partDataSources;

		private FilterTextBox _partDataSourceSearch;

		static DataSourcesControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DataSourcesControl),
			                                         new FrameworkPropertyMetadata(typeof(DataSourcesControl)));
		}

		public DataSourcesControl()
		{
			FilteredItemsSource = new ObservableCollection<IDataSourceViewModel>();
			Instance = this;
			UpdateNoDataSourcesMessage();
		}

		public string NoDataSourcesActions
		{
			get { return (string) GetValue(NoDataSourcesActionsProperty); }
			protected set { SetValue(NoDataSourcesActionsPropertyKey, value); }
		}

		public string NoDataSourcesReason
		{
			get { return (string) GetValue(NoDataSourcesReasonProperty); }
			protected set { SetValue(NoDataSourcesReasonPropertyKey, value); }
		}

		public bool IsPinned
		{
			get { return (bool) GetValue(IsPinnedProperty); }
			set { SetValue(IsPinnedProperty, value); }
		}

		public string StringFilter
		{
			get { return (string) GetValue(StringFilterProperty); }
			set
			{
				var old = StringFilter;
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

		private static void OnSelectedItemChanged(DependencyObject dependencyObject,
		                                          DependencyPropertyChangedEventArgs e)
		{
			((DataSourcesControl) dependencyObject).OnSelectedItemChanged((IDataSourceViewModel) e.NewValue);
		}

		private void OnSelectedItemChanged(IDataSourceViewModel selectedViewModel)
		{
			var treeViewItem = GetTreeViewItem(selectedViewModel);
			if (treeViewItem != null)
				SelectItem(treeViewItem);
			else
				Dispatcher.BeginInvoke(new Action(() =>
				{
					var item = GetTreeViewItem(selectedViewModel);
					if (item != null) SelectItem(item);
				}));
		}

		private void SelectItem(TreeViewItem treeViewItem)
		{
			treeViewItem.IsSelected = true;
			treeViewItem.BringIntoView();
		}

		private static void OnStringFilterChanged(DependencyObject dependencyObject,
		                                          DependencyPropertyChangedEventArgs e)
		{
			((DataSourcesControl) dependencyObject).OnStringFilterChanged((string) e.OldValue, (string) e.NewValue);
		}

		private void OnStringFilterChanged(string oldValue, string newValue)
		{
			if (ItemsSource != null)
			{
				var sourceIndex = 0;
				var filterIndex = 0;
				foreach (var model in ItemsSource)
				{
					var passedOld = PassesFilter(model, oldValue);
					var passesNew = PassesFilter(model, newValue);

					if (!passedOld)
					{
						if (passesNew) FilteredItemsSource.Insert(filterIndex++, model);
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

			UpdateNoDataSourcesMessage();
		}

		private void UpdateNoDataSourcesMessage()
		{
			if (FilteredItemsSource.Any())
			{
				NoDataSourcesReason = null;
				NoDataSourcesActions = null;
			}
			else if (!string.IsNullOrEmpty(StringFilter))
			{
				NoDataSourcesReason = $"No data source matches '{StringFilter}'";
				NoDataSourcesActions = "Try changing the filter or add the desired data source";
			}
			else
			{
				NoDataSourcesReason = "No data source opened";
				NoDataSourcesActions = "Try opening a file or folder, create a new data source from the file menu or simply drag and drop a file into this window.";
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
			var viewModel = e.Data.GetData(typeof(FileDataSourceViewModel)) as IDataSourceViewModel ??
			                e.Data.GetData(typeof(MergedDataSourceViewModel)) as IDataSourceViewModel;
			var source = new TreeItem
			{
				ViewModel = viewModel
			};
			if (source.ViewModel == null)
				return false;

			var dropTarget = GetDataSourceFromPosition(e.GetPosition(_partDataSources));
			if (dropTarget == null)
				return false;

			var dropType = GetDropType(e, dropTarget, viewModel);
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

			var pos = e.GetPosition(destination.TreeViewItem);

			var dropType = DataSourceDropType.None;
			var height = destination.TreeViewItem.ActualHeight;
			if (source is FileDataSourceViewModel)
			{
				// Let's distribute it as follows:
				// 20% top => arrange
				// 60% middle => group
				// 20% bottom => arrange
				//
				// This way 60% of the height is used for grouping and 40% is used for arranging.
				if (pos.Y < height * 0.2)
					dropType |= DataSourceDropType.ArrangeTop;
				else if (pos.Y < height * 0.8)
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
				if (pos.Y < height * 0.5)
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
			var treeViewItem = GetTreeViewItemFromPosition(position);
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
				try
				{
					element = VisualTreeHelper.GetParent(element);
				}
				catch (InvalidOperationException e)
				{
					Log.WarnFormat("Caught unexpected exception: {0}", e);
					return null;
				}

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
				var source = SelectedItem;
				var treeViewItem = SelectedTreeViewItem;

				if (treeViewItem.IsMouseOver && ((DataSourcesViewModel) DataContext).CanBeDragged(source))
					DragLayer.DoDragDrop(source, treeViewItem, DragDropEffects.Move);
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

			var containerGenerator = partDataSources.ItemContainerGenerator;
			return GetTreeViewItem(containerGenerator, item);
		}

		private TreeViewItem GetTreeViewItem(ItemContainerGenerator containerGenerator, IDataSourceViewModel item)
		{
			var container = (TreeViewItem) containerGenerator.ContainerFromItem(item);
			if (container != null)
				return container;

			foreach (var childItem in containerGenerator.Items)
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
			if (old != null) old.CollectionChanged -= ItemsSourceOnCollectionChanged;

			FilteredItemsSource.Clear();

			if (newValue != null)
			{
				foreach (var model in newValue)
					if (PassesFilter(model))
						FilteredItemsSource.Add(model);

				var @new = newValue as INotifyCollectionChanged;
				if (@new != null) @new.CollectionChanged += ItemsSourceOnCollectionChanged;
			}

			UpdateNoDataSourcesMessage();
		}

		private void ItemsSourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					for (var i = 0; i < e.NewItems.Count; ++i)
					{
						var model = (IDataSourceViewModel) e.NewItems[i];
						if (PassesFilter(model))
						{
							var sourceIndex = e.NewStartingIndex + i;
							var filteredIndex = FindFilteredIndex(sourceIndex);
							FilteredItemsSource.Insert(filteredIndex, model);
						}
					}

					break;

				case NotifyCollectionChangedAction.Remove:
					foreach (IDataSourceViewModel model in e.OldItems) FilteredItemsSource.Remove(model);
					break;

				case NotifyCollectionChangedAction.Reset:
					FilteredItemsSource.Clear();
					break;

				default:
					throw new NotImplementedException();
			}

			UpdateNoDataSourcesMessage();
		}

		private int FindFilteredIndex(int sourceIndex)
		{
			for (var i = sourceIndex - 1; i >= 0; --i)
			{
				var previousModel = ItemsSource.ElementAt(i);
				if (PassesFilter(previousModel))
				{
					// We found the next predecessor to the given source index that should be
					// before "sourceIndex"
					var filteredIndex = FilteredItemsSource.IndexOf(previousModel);
					var desiredIndex = filteredIndex + 1;
					return desiredIndex;
				}
			}

			return 0;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.IsDown)
				switch (e.Key)
				{
					case Key.Down:
						break;
					case Key.Up:
						break;
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
				var index = ItemsSource.IndexOf(SelectedItem);
				var nextIndex = (index + 1) % ItemsSource.Count();
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
				var index = ItemsSource.IndexOf(SelectedItem);
				var nextIndex = index - 1;
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

			var idx = model.DisplayName.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase);
			if (idx != -1)
				return true;

			return false;
		}

		public void FocusSearch()
		{
			var element = _partDataSourceSearch;
			element?.Focus();
		}

		public static UIElement GetItemContainerFromItemsControl(ItemsControl itemsControl)
		{
			UIElement container;
			if (itemsControl != null && itemsControl.Items.Count > 0)
				container = itemsControl.ItemContainerGenerator.ContainerFromIndex(index: 0) as UIElement;
			else
				container = itemsControl;
			return container;
		}
	}
}