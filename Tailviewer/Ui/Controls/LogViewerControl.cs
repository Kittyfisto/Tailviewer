using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Ui.ViewModels;
using log4net;

namespace Tailviewer.Ui.Controls
{
	internal class LogViewerControl : Control
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly DependencyProperty LogFileProperty =
			DependencyProperty.Register("LogFile", typeof (ILogFile), typeof (LogViewerControl),
			                            new PropertyMetadata(default(ILogFile)));

		public static readonly DependencyProperty DataSourceProperty =
			DependencyProperty.Register("DataSource", typeof (IDataSourceViewModel), typeof (LogViewerControl),
			                            new PropertyMetadata(null, OnDataSourceChanged));

		public static readonly DependencyProperty LogEntryCountProperty =
			DependencyProperty.Register("LogEntryCount", typeof (int), typeof (LogViewerControl),
			                            new PropertyMetadata(0));

		public static readonly DependencyProperty ShowFatalProperty =
			DependencyProperty.Register("ShowFatal", typeof (bool), typeof (LogViewerControl),
			                            new PropertyMetadata(false, OnFatalChanged));

		public static readonly DependencyProperty ShowErrorProperty =
			DependencyProperty.Register("ShowError", typeof (bool), typeof (LogViewerControl),
			                            new PropertyMetadata(false, OnErrorChanged));

		public static readonly DependencyProperty ShowWarningProperty =
			DependencyProperty.Register("ShowWarning", typeof (bool), typeof (LogViewerControl),
			                            new PropertyMetadata(false, OnWarningChanged));

		public static readonly DependencyProperty ShowInfoProperty =
			DependencyProperty.Register("ShowInfo", typeof (bool), typeof (LogViewerControl),
			                            new PropertyMetadata(false, OnInfoChanged));

		public static readonly DependencyProperty ShowDebugProperty =
			DependencyProperty.Register("ShowDebug", typeof (bool), typeof (LogViewerControl),
			                            new PropertyMetadata(false, OnDebugChanged));

		public static readonly DependencyProperty CopySelectedLineToClipboardCommandProperty =
			DependencyProperty.Register("CopySelectedLineToClipboardCommand", typeof (ICommand), typeof (LogViewerControl),
			                            new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty ShowAllProperty =
			DependencyProperty.Register("ShowAll", typeof (bool?), typeof (LogViewerControl),
			                            new PropertyMetadata(false, OnShowAllChanged));

		public static readonly DependencyProperty TotalLogEntryCountProperty =
			DependencyProperty.Register("TotalLogEntryCount", typeof (int), typeof (LogViewerControl),
			                            new PropertyMetadata(default(int)));

		public static readonly DependencyProperty ErrorMessageProperty =
			DependencyProperty.Register("ErrorMessage", typeof (string), typeof (LogViewerControl),
			                            new PropertyMetadata(default(string)));

		public static readonly DependencyProperty DetailedErrorMessageProperty =
			DependencyProperty.Register("DetailedErrorMessage", typeof (string), typeof (LogViewerControl),
			                            new PropertyMetadata(default(string)));

		private bool _logged;

		private LogEntryListView _partListView;
		private FilterTextBox _partStringFilter;
		private bool _scrollByUser;
		private ScrollViewer _scrollViewer;

		static LogViewerControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (LogViewerControl),
			                                         new FrameworkPropertyMetadata(typeof (LogViewerControl)));
		}

		public LogViewerControl()
		{
			CopySelectedLineToClipboardCommand = new DelegateCommand(CopySelectedLineToClipboard);
			SizeChanged += OnSizeChanged;
		}

		public ILogFile LogFile
		{
			get { return (ILogFile) GetValue(LogFileProperty); }
			set { SetValue(LogFileProperty, value); }
		}

		public string DetailedErrorMessage
		{
			get { return (string) GetValue(DetailedErrorMessageProperty); }
			set { SetValue(DetailedErrorMessageProperty, value); }
		}

		public string ErrorMessage
		{
			get { return (string) GetValue(ErrorMessageProperty); }
			set { SetValue(ErrorMessageProperty, value); }
		}

		public bool? ShowAll
		{
			get { return (bool?) GetValue(ShowAllProperty); }
			set { SetValue(ShowAllProperty, value); }
		}

		public ICommand CopySelectedLineToClipboardCommand
		{
			get { return (ICommand) GetValue(CopySelectedLineToClipboardCommandProperty); }
			set { SetValue(CopySelectedLineToClipboardCommandProperty, value); }
		}

		public bool ShowDebug
		{
			get { return (bool) GetValue(ShowDebugProperty); }
			set { SetValue(ShowDebugProperty, value); }
		}

		public bool ShowInfo
		{
			get { return (bool) GetValue(ShowInfoProperty); }
			set { SetValue(ShowInfoProperty, value); }
		}

		public bool ShowWarning
		{
			get { return (bool) GetValue(ShowWarningProperty); }
			set { SetValue(ShowWarningProperty, value); }
		}

		public bool ShowError
		{
			get { return (bool) GetValue(ShowErrorProperty); }
			set { SetValue(ShowErrorProperty, value); }
		}

		public bool ShowFatal
		{
			get { return (bool) GetValue(ShowFatalProperty); }
			set { SetValue(ShowFatalProperty, value); }
		}

		public IDataSourceViewModel DataSource
		{
			get { return (IDataSourceViewModel) GetValue(DataSourceProperty); }
			set { SetValue(DataSourceProperty, value); }
		}

		public int TotalLogEntryCount
		{
			get { return (int) GetValue(TotalLogEntryCountProperty); }
			set { SetValue(TotalLogEntryCountProperty, value); }
		}

		public int LogEntryCount
		{
			get { return (int) GetValue(LogEntryCountProperty); }
			set { SetValue(LogEntryCountProperty, value); }
		}

		private void CopySelectedLineToClipboard()
		{
			/*ListView listViewer = _partListView;
			if (listViewer != null)
			{
				IList items = listViewer.SelectedItems;
				IEnumerable<LogEntryViewModel> viewModels = items != null
					                                            ? items.Cast<LogEntryViewModel>()
					                                            : Enumerable.Empty<LogEntryViewModel>();
				var builder = new StringBuilder();
				foreach (LogEntryViewModel item in viewModels)
				{
					builder.AppendLine(item.Message);
				}
				string message = builder.ToString();
				Clipboard.SetText(message);
			}*/
		}

		private static void OnDataSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl) dependencyObject).OnDataSourceChanged(args.OldValue as IDataSourceViewModel,
			                                                          args.NewValue as IDataSourceViewModel);
		}

		private void OnShowAllChanged(bool? showAll)
		{
			if (showAll == true)
			{
				ShowDebug = true;
				ShowInfo = true;
				ShowWarning = true;
				ShowError = true;
				ShowFatal = true;
			}
			else if (showAll == false)
			{
				ShowDebug = false;
				ShowInfo = false;
				ShowWarning = false;
				ShowError = false;
				ShowFatal = false;
			}
		}

		private static void OnShowAllChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl) dependencyObject).OnShowAllChanged((bool?) args.NewValue);
		}

		private static void OnFatalChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl) dependencyObject).OnFatalChanged((bool) args.NewValue);
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
			((LogViewerControl) dependencyObject).OnErrorChanged((bool) args.NewValue);
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
			((LogViewerControl) dependencyObject).OnWarningChanged((bool) args.NewValue);
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
			((LogViewerControl) dependencyObject).OnInfoChanged((bool) args.NewValue);
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
			((LogViewerControl) dependencyObject).OnDebugChanged((bool) args.NewValue);
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

			LevelFlags levels = DataSource.LevelsFilter;

			ShowFatal = levels.HasFlag(LevelFlags.Fatal);
			ShowError = levels.HasFlag(LevelFlags.Error);
			ShowWarning = levels.HasFlag(LevelFlags.Warning);
			ShowInfo = levels.HasFlag(LevelFlags.Info);
			ShowDebug = levels.HasFlag(LevelFlags.Debug);

			if (levels == LevelFlags.All)
			{
				ShowAll = true;
			}
			else if (levels == LevelFlags.None)
			{
				ShowAll = false;
			}
			else
			{
				ShowAll = null;
			}
		}

		private void OnDataSourceChanged(IDataSourceViewModel oldValue, IDataSourceViewModel newValue)
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
			}
		}

		private void OnFollowTailChanged()
		{
			if (DataSource != null && DataSource.FollowTail)
			{
				ScrollToTail();
			}
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
					DependencyObject border = VisualTreeHelper.GetChild(_partListView, 0);
					DependencyObject child = VisualTreeHelper.GetChild(border, 0);
					_scrollViewer = child as ScrollViewer;
					if (_scrollViewer != null)
					{
						_scrollViewer.ScrollChanged += OnScrollChanged;
					}
					else
					{
						if (!_logged)
						{
							Log.ErrorFormat("Unable to find ScrollViewer in visual tree - can't scroll to bottom");
							_logged = true;
						}
					}
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

			_partListView = (LogEntryListView) GetTemplateChild("PART_ListView");
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