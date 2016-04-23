using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Ui.Controls.LogView;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls
{
	internal class LogViewerControl : Control
	{
		public static readonly DependencyProperty LogFileProperty =
			DependencyProperty.Register("LogFile", typeof (ILogFile), typeof (LogViewerControl),
			                            new PropertyMetadata(default(ILogFile)));

		public static readonly DependencyProperty DataSourceProperty =
			DependencyProperty.Register("DataSource", typeof (IDataSourceViewModel), typeof (LogViewerControl),
			                            new PropertyMetadata(null, OnDataSourceChanged));

		public static readonly DependencyProperty LogEntryCountProperty =
			DependencyProperty.Register("LogEntryCount", typeof (int), typeof (LogViewerControl),
			                            new PropertyMetadata(0));

		public static readonly DependencyProperty ShowLineNumbersProperty =
			DependencyProperty.Register("ShowLineNumbers", typeof (bool), typeof (LogViewerControl),
			                            new PropertyMetadata(default(bool), OnShowLineNumbersChanged));

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

		private LogEntryListView _partListView;

		private FilterTextBox _partStringFilter;

		static LogViewerControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (LogViewerControl),
			                                         new FrameworkPropertyMetadata(typeof (LogViewerControl)));
		}

		public LogViewerControl()
		{
			CopySelectedLineToClipboardCommand = new DelegateCommand(CopySelectedLineToClipboard);
		}

		public bool ShowLineNumbers
		{
			get { return (bool) GetValue(ShowLineNumbersProperty); }
			set { SetValue(ShowLineNumbersProperty, value); }
		}

		public LogEntryListView PartListView
		{
			get { return _partListView; }
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

		private static void OnShowLineNumbersChanged(DependencyObject dependencyObject,
		                                             DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl) dependencyObject).OnShowLineNumbersChanged((bool) args.NewValue);
		}

		private void OnShowLineNumbersChanged(bool showLineNumbers)
		{
			IDataSourceViewModel dataSource = DataSource;
			if (dataSource != null)
				dataSource.ShowLineNumbers = showLineNumbers;
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
				if (_partListView != null)
					_partListView.FollowTail = newValue.FollowTail;

				ShowLineNumbers = newValue.ShowLineNumbers;
			}
			OnLevelsChanged();
		}

		private void DataSourceOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case "FollowTail":
					_partListView.FollowTail = ((IDataSourceViewModel) sender).FollowTail;
					break;

				case "LevelsFilter":
					OnLevelsChanged();
					break;
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_partListView != null)
			{
				_partListView.FollowTailChanged -= OnFollowTailChanged;
			}

			_partListView = (LogEntryListView) GetTemplateChild("PART_ListView");

			if (_partListView != null)
			{
				_partListView.FollowTailChanged += OnFollowTailChanged;

				IDataSourceViewModel dataSource = DataSource;
				if (dataSource != null)
					_partListView.FollowTail = dataSource.FollowTail;
			}

			_partStringFilter = (FilterTextBox) GetTemplateChild("PART_StringFilter");
		}

		private void OnFollowTailChanged(bool followTail)
		{
			IDataSourceViewModel dataSource = DataSource;
			if (dataSource != null)
				dataSource.FollowTail = followTail;
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