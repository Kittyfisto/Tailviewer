using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SharpTail.BusinessLogic;
using SharpTail.Ui.ViewModels;

namespace SharpTail.Ui.Controls
{
	public class LogViewerControl : Control
	{
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof (IEnumerable<LogEntryViewModel>), typeof (LogViewerControl),
			                            new PropertyMetadata(null, OnItemsSourceChanged));

		public static readonly DependencyProperty LogEntryCountProperty =
			DependencyProperty.Register("LogEntryCount", typeof (int), typeof (LogViewerControl),
			                            new PropertyMetadata(0));

		public static readonly DependencyProperty AllLogEntryCountProperty =
			DependencyProperty.Register("AllLogEntryCount", typeof (int), typeof (LogViewerControl),
			                            new PropertyMetadata(0));

		public static readonly DependencyProperty StringFilterProperty =
			DependencyProperty.Register("StringFilter", typeof (string), typeof (LogViewerControl),
			                            new PropertyMetadata(null));

		public static readonly DependencyProperty FollowTailProperty =
			DependencyProperty.Register("FollowTail", typeof (bool),
			                            typeof (LogViewerControl),
			                            new PropertyMetadata(false, OnFollowTailChanged));

		public static readonly DependencyProperty FileSizeProperty =
			DependencyProperty.Register("FileSize", typeof (Size), typeof (LogViewerControl), new PropertyMetadata(default(Size)));

		public static readonly DependencyProperty LevelsFilterProperty =
			DependencyProperty.Register("LevelsFilter", typeof (LevelFlags), typeof (LogViewerControl),
			                            new PropertyMetadata(default(LevelFlags)));

		private ListView _partListView;
		private ScrollViewer _scrollViewer;

		static LogViewerControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (LogViewerControl),
			                                         new FrameworkPropertyMetadata(typeof (LogViewerControl)));
		}

		public LevelFlags LevelsFilter
		{
			get { return (LevelFlags) GetValue(LevelsFilterProperty); }
			set { SetValue(LevelsFilterProperty, value); }
		}

		public Size FileSize
		{
			get { return (Size) GetValue(FileSizeProperty); }
			set { SetValue(FileSizeProperty, value); }
		}

		public string StringFilter
		{
			get { return (string) GetValue(StringFilterProperty); }
			set { SetValue(StringFilterProperty, value); }
		}

		public int AllLogEntryCount
		{
			get { return (int) GetValue(AllLogEntryCountProperty); }
			set { SetValue(AllLogEntryCountProperty, value); }
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

		public bool FollowTail
		{
			get { return (bool) GetValue(FollowTailProperty); }
			set { SetValue(FollowTailProperty, value); }
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
			if (FollowTail)
			{
				ScrollToTail();
			}
		}

		private static void OnFollowTailChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			((LogViewerControl) dependencyObject).OnFollowTailChanged((bool) e.NewValue);
		}

		private void OnFollowTailChanged(bool followTail)
		{
			if (followTail)
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
				_scrollViewer.ScrollToBottom();
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_partListView = (ListView) GetTemplateChild("PART_ListView");
			if (_partListView != null)
			{
			}
		}

		private void OnScrollChanged(object sender, ScrollChangedEventArgs args)
		{
			if (args.VerticalChange < 0)
			{
				FollowTail = false;
			}
		}
	}
}