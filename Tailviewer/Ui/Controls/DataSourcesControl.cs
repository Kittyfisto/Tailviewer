using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls
{
	[TemplatePart(Name = PART_DataSources, Type=typeof(TreeView))]
	[TemplatePart(Name = PART_DataSourceSearch, Type = typeof(FilterTextBox))]
	internal class DataSourcesControl : Control
	{
		public const string PART_DataSources = "PART_DataSources";
		public const string PART_DataSourceSearch = "PART_DataSourceSearch";

		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IEnumerable<IDataSourceViewModel>), typeof(DataSourcesControl),
			                            new PropertyMetadata(null, OnDataSourcesChanged));

		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof(IDataSourceViewModel), typeof(DataSourcesControl),
			                            new PropertyMetadata(null, OnSelectedItemChanged));

		private static readonly DependencyPropertyKey FilteredItemsSourcePropertyKey
			= DependencyProperty.RegisterReadOnly("FilteredItemsSource", typeof(ObservableCollection<IDataSourceViewModel>),
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
				var old = StringFilter;
				SetValue(StringFilterProperty, value);
				OnStringFilterChanged(old, value);
			}
		}

		public ObservableCollection<IDataSourceViewModel> FilteredItemsSource
		{
			get { return (ObservableCollection<IDataSourceViewModel>)GetValue(FilteredItemsSourceProperty); }
			private set { SetValue(FilteredItemsSourcePropertyKey, value); }
		}

		public IDataSourceViewModel SelectedItem
		{
			get { return (IDataSourceViewModel)GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public IEnumerable<IDataSourceViewModel> ItemsSource
		{
			get { return (IEnumerable<IDataSourceViewModel>)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
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
			_partDataSources.SelectedItemChanged += PartDataSourcesOnSelectedItemChanged;
		}

		private void PartDataSourcesOnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> args)
		{
			SelectedItem = args.NewValue as IDataSourceViewModel;
		}

		private static void OnDataSourcesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((DataSourcesControl)d).OnDataSourcesChanged((IEnumerable<IDataSourceViewModel>)e.OldValue,
														  (IEnumerable<IDataSourceViewModel>)e.NewValue);
		}

		private void OnDataSourcesChanged(IEnumerable<IDataSourceViewModel> oldValue, IEnumerable<IDataSourceViewModel> newValue)
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
						var model = (IDataSourceViewModel)e.NewItems[0];
						if (PassesFilter(model))
							FilteredItemsSource.Add(model);
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
			((DataSourcesControl) dependencyObject).OnSelectedItemChanged((SingleDataSourceViewModel) args.OldValue,
			                                                              (SingleDataSourceViewModel) args.NewValue);
		}

		private void OnSelectedItemChanged(SingleDataSourceViewModel oldValue, SingleDataSourceViewModel newValue)
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

		private void SetSelected(SingleDataSourceViewModel newValue, bool isSelected)
		{
			// Because why wouldn't we want to make selecting an item in a treeview
			// a hassle?!

			if (_partDataSources == null)
				return;

			var treeViewItem = _partDataSources
				.ItemContainerGenerator
				.ContainerFromItem(newValue);
			if (treeViewItem == null)
				return;

			MethodInfo selectMethod =
				typeof (TreeViewItem).GetMethod("Select",
				                                BindingFlags.NonPublic | BindingFlags.Instance);
			selectMethod.Invoke(treeViewItem, new object[] { isSelected });
		}

		public void FocusSearch()
		{
			FilterTextBox element = _partDataSourceSearch;
			if (element != null)
			{
				element.Focus();
			}
		}
	}
}