using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls
{
	internal class LogViewerControl : Control
	{
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof (IEnumerable<LogEntryViewModel>), typeof (LogViewerControl),
			                            new PropertyMetadata(null, OnItemsSourceChanged));

		public static readonly DependencyProperty DataSourceProperty =
			DependencyProperty.Register("DataSource", typeof (DataSourceViewModel), typeof (LogViewerControl),
			                            new PropertyMetadata(null, OnDataSourceChanged));

		public static readonly DependencyProperty LogEntryCountProperty =
			DependencyProperty.Register("LogEntryCount", typeof (int), typeof (LogViewerControl),
			                            new PropertyMetadata(0));

		private ListView _partListView;
		private FilterTextBox _partStringFilter;
		private bool _scrollByUser;
		private ScrollViewer _scrollViewer;

		static LogViewerControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (LogViewerControl),
			                                         new FrameworkPropertyMetadata(typeof (LogViewerControl)));
		}

		public DataSourceViewModel DataSource
		{
			get { return (DataSourceViewModel) GetValue(DataSourceProperty); }
			set { SetValue(DataSourceProperty, value); }
		}

		public int LogEntryCount
		{
			get { return (int) GetValue(LogEntryCountProperty); }
			set { SetValue(LogEntryCountProperty, value); }
		}

		public IEnumerable<LogEntryViewModel> ItemsSource
		{
			get { return (IEnumerable<LogEntryViewModel>) GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		private static void OnDataSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl) dependencyObject).OnDataSourceChanged(args.OldValue as DataSourceViewModel,
			                                                          args.NewValue as DataSourceViewModel);
		}

		private void OnDataSourceChanged(DataSourceViewModel oldValue, DataSourceViewModel newValue)
		{
			if (oldValue != null)
			{
				oldValue.PropertyChanged -= DataSourceOnPropertyChanged;
			}
			if (newValue != null)
			{
				newValue.PropertyChanged += DataSourceOnPropertyChanged;
			}
		}

		private void DataSourceOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case "FollowTail":
					OnFollowTailChanged();
					break;
			}
		}

		private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LogViewerControl) d).OnItemsSourceChanged(e.OldValue as INotifyCollectionChanged,
			                                            e.NewValue as INotifyCollectionChanged);
		}

		private void OnItemsSourceChanged(INotifyCollectionChanged oldValue, INotifyCollectionChanged newValue)
		{
			if (oldValue != null)
			{
				oldValue.CollectionChanged -= ItemsSourceOnCollectionChanged;
			}
			if (newValue != null)
			{
				newValue.CollectionChanged += ItemsSourceOnCollectionChanged;
			}
		}

		private void ItemsSourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (DataSource != null && DataSource.FollowTail)
			{
				ScrollToTail();
			}
		}

		private void OnFollowTailChanged()
		{
			if (DataSource != null && DataSource.FollowTail)
			{
				ScrollToTail();
			}
		}

		private void ScrollToTail()
		{
			if (_partListView != null && _scrollViewer == null)
			{
				if (VisualTreeHelper.GetChildrenCount(_partListView) > 0)
				{
					var border = (Border) VisualTreeHelper.GetChild(_partListView, 0);
					_scrollViewer = (ScrollViewer) VisualTreeHelper.GetChild(border, 0);
					_scrollViewer.ScrollChanged += OnScrollChanged;
				}
			}

			if (_scrollViewer != null)
			{
				_scrollByUser = false;
				_scrollViewer.ScrollToBottom();
			}
		}

		private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.VerticalChange == 0.0)
				return;

			if (_scrollByUser)
			{
				if (e.VerticalChange > 0.0)
				{
					double scrollerOffset = e.VerticalOffset + e.ViewportHeight;
					if (Math.Abs(scrollerOffset - e.ExtentHeight) < 5.0)
					{
						DataSource.FollowTail = true;
					}
				}
				else
				{
					DataSource.FollowTail = false;
				}
			}
			_scrollByUser = true;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_partListView = (ListView) GetTemplateChild("PART_ListView");
			_partStringFilter = (FilterTextBox) GetTemplateChild("PART_StringFilter");
		}

		public void FocusStringFilter()
		{
			FilterTextBox element = _partStringFilter;
			if (element != null)
			{
				element.Focus();
			}
		}
	}
}