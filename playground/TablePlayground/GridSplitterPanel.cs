using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TablePlayground
{
	/// <summary>
	///     A grid that acts like an <see cref="ItemsControl" />:
	///     Each item is hosted inside a column of a grid.
	///     Grid splitters are placed in between to allow the user to resize columns.
	/// </summary>
	public sealed class GridSplitterPanel
		: Grid
	{
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof (IEnumerable), typeof (GridSplitterPanel),
			                            new PropertyMetadata(null, OnItemsSourceChanged));

		public static readonly DependencyProperty ItemTemplateProperty =
			DependencyProperty.Register("ItemTemplate", typeof (DataTemplate), typeof (GridSplitterPanel), new PropertyMetadata(default(DataTemplate)));

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate) GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((GridSplitterPanel) d).OnItemsSourceChanged((IEnumerable) e.OldValue, (IEnumerable) e.NewValue);
		}

		private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
		{
			var changed = oldValue as INotifyCollectionChanged;
			if (changed != null)
			{
				changed.CollectionChanged -= ItemsSourceOnCollectionChanged;
			}

			Clear();
			if (newValue != null)
			{
				foreach (var item in newValue)
				{
					Add(item);
				}
			}

			changed = newValue as INotifyCollectionChanged;
			if (changed != null)
			{
				changed.CollectionChanged += ItemsSourceOnCollectionChanged;
			}
		}

		private void Add(object item)
		{
			var index = ItemsSource.Cast<object>().Count() - 1;
			var container = GenerateContainer(item);
			var columnDefinition = new ColumnDefinition
				{
					Width = new GridLength(1, GridUnitType.Auto)
				};
			ColumnDefinitions.Add(columnDefinition);
			Children.Add(container);
			SetColumn(container, index);

			var gridSpliter = new GridSplitter
				{
					Width = 5,
					ResizeBehavior = GridResizeBehavior.CurrentAndNext,
					VerticalAlignment = VerticalAlignment.Stretch,
					HorizontalAlignment = HorizontalAlignment.Right,
					Background = Brushes.Transparent
				};
			Children.Add(gridSpliter);
			SetColumn(gridSpliter, index);
		}

		private void Clear()
		{
			ColumnDefinitions.Clear();
			Children.Clear();
		}

		private void ItemsSourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (var item in args.NewItems)
					{
						Add(item);
					}
					break;
			}
		}

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable) GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		private ContentPresenter GenerateContainer(object item)
		{
			return new ContentPresenter { Content = item, ContentTemplate = ItemTemplate };
		}
	}
}