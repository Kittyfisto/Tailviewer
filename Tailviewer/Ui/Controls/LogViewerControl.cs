using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tailviewer.BusinessLogic;
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

		public static readonly DependencyProperty ShowFatalProperty =
			DependencyProperty.Register("ShowFatal", typeof(bool), typeof(LogViewerControl),
										new PropertyMetadata(false, OnFatalChanged));

		public static readonly DependencyProperty ShowErrorProperty =
			DependencyProperty.Register("ShowError", typeof(bool), typeof(LogViewerControl),
										new PropertyMetadata(false, OnErrorChanged));

		public static readonly DependencyProperty ShowWarningProperty =
			DependencyProperty.Register("ShowWarning", typeof(bool), typeof(LogViewerControl),
										new PropertyMetadata(false, OnWarningChanged));

		public static readonly DependencyProperty ShowInfoProperty =
			DependencyProperty.Register("ShowInfo", typeof(bool), typeof(LogViewerControl),
										new PropertyMetadata(false, OnInfoChanged));

		public static readonly DependencyProperty ShowDebugProperty =
			DependencyProperty.Register("ShowDebug", typeof(bool), typeof(LogViewerControl),
										new PropertyMetadata(false, OnDebugChanged));

		public static readonly DependencyProperty ShowOtherProperty =
			DependencyProperty.Register("ShowOther", typeof(bool), typeof(LogViewerControl),
										new PropertyMetadata(false, OnShowOtherChanged));

		private ListView _partListView;
		private FilterTextBox _partStringFilter;
		private bool _scrollByUser;
		private ScrollViewer _scrollViewer;

		static LogViewerControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (LogViewerControl),
			                                         new FrameworkPropertyMetadata(typeof (LogViewerControl)));
		}

		public bool ShowOther
		{
			get { return (bool)GetValue(ShowOtherProperty); }
			set { SetValue(ShowOtherProperty, value); }
		}

		public bool ShowDebug
		{
			get { return (bool)GetValue(ShowDebugProperty); }
			set { SetValue(ShowDebugProperty, value); }
		}

		public bool ShowInfo
		{
			get { return (bool)GetValue(ShowInfoProperty); }
			set { SetValue(ShowInfoProperty, value); }
		}

		public bool ShowWarning
		{
			get { return (bool)GetValue(ShowWarningProperty); }
			set { SetValue(ShowWarningProperty, value); }
		}

		public bool ShowError
		{
			get { return (bool)GetValue(ShowErrorProperty); }
			set { SetValue(ShowErrorProperty, value); }
		}

		public bool ShowFatal
		{
			get { return (bool)GetValue(ShowFatalProperty); }
			set { SetValue(ShowFatalProperty, value); }
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

		private void OnOtherFilterChanged()
		{
			if (DataSource == null)
				return;

			ShowOther = !DataSource.OtherFilter;
		}

		private static void OnShowOtherChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl)dependencyObject).OnShowOtherChanged((bool)args.NewValue);
		}

		private void OnShowOtherChanged(bool showOther)
		{
			if (DataSource == null)
				return;

			DataSource.OtherFilter = !showOther;
		}

		private static void OnFatalChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl)dependencyObject).OnFatalChanged((bool)args.NewValue);
		}

		private void OnFatalChanged(bool isChecked)
		{
			if (DataSource == null)
				return;

			const LevelFlags level = LevelFlags.Fatal;
			if (isChecked)
			{
				DataSource.LevelsFilter |= level;
			}
			else
			{
				DataSource.LevelsFilter &= ~level;
			}
		}

		private static void OnErrorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl)dependencyObject).OnErrorChanged((bool)args.NewValue);
		}

		private void OnErrorChanged(bool isChecked)
		{
			if (DataSource == null)
				return;

			const LevelFlags level = LevelFlags.Error;
			if (isChecked)
			{
				DataSource.LevelsFilter |= level;
			}
			else
			{
				DataSource.LevelsFilter &= ~level;
			}
		}

		private static void OnWarningChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl)dependencyObject).OnWarningChanged((bool)args.NewValue);
		}

		private void OnWarningChanged(bool isChecked)
		{
			if (DataSource == null)
				return;

			const LevelFlags level = LevelFlags.Warning;
			if (isChecked)
			{
				DataSource.LevelsFilter |= level;
			}
			else
			{
				DataSource.LevelsFilter &= ~level;
			}
		}

		private static void OnInfoChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl)dependencyObject).OnInfoChanged((bool)args.NewValue);
		}

		private void OnInfoChanged(bool isChecked)
		{
			if (DataSource == null)
				return;

			const LevelFlags level = LevelFlags.Info;
			if (isChecked)
			{
				DataSource.LevelsFilter |= level;
			}
			else
			{
				DataSource.LevelsFilter &= ~level;
			}
		}

		private static void OnDebugChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl)dependencyObject).OnDebugChanged((bool)args.NewValue);
		}

		private void OnDebugChanged(bool isChecked)
		{
			if (DataSource == null)
				return;

			const LevelFlags level = LevelFlags.Debug;
			if (isChecked)
			{
				DataSource.LevelsFilter |= level;
			}
			else
			{
				DataSource.LevelsFilter &= ~level;
			}
		}

		private void OnLevelsChanged()
		{
			if (DataSource == null)
				return;

			var levels = DataSource.LevelsFilter;
			ShowFatal = levels.HasFlag(LevelFlags.Fatal);
			ShowError = levels.HasFlag(LevelFlags.Error);
			ShowWarning = levels.HasFlag(LevelFlags.Warning);
			ShowInfo = levels.HasFlag(LevelFlags.Info);
			ShowDebug = levels.HasFlag(LevelFlags.Debug);
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
			OnLevelsChanged();
			OnOtherFilterChanged();
		}

		private void DataSourceOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case "FollowTail":
					OnFollowTailChanged();
					break;

				case "LevelsFilter":
					OnLevelsChanged();
					break;

				case "OtherFilter":
					OnOtherFilterChanged();
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

		public LogViewerControl()
		{
			SizeChanged += OnSizeChanged;
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			// We don't want to disable auto scroll because the user resized
			// the window which in turn caused the scroll to change by a negative value...
			_scrollByUser = false;
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

			if (DataSource != null)
			{
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