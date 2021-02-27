using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Settings;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Ui.LogView
{
	public partial class LogViewerControl
	{
		public static readonly DependencyProperty LogViewProperty =
			DependencyProperty.Register("LogView", typeof(LogViewerViewModel), typeof(LogViewerControl),
				new PropertyMetadata(default(LogViewerViewModel), OnLogViewChanged));

		public static readonly DependencyProperty LogSourceProperty =
			DependencyProperty.Register("LogSource", typeof(ILogSource), typeof(LogViewerControl),
				new PropertyMetadata(default(ILogSource)));

		public static readonly DependencyProperty FindAllProperty = DependencyProperty.Register(
		                                                "FindAll", typeof(IFindAllViewModel), typeof(LogViewerControl), new PropertyMetadata(default(IFindAllViewModel)));

		public static readonly DependencyProperty SearchProperty =
			DependencyProperty.Register("Search", typeof(ILogSourceSearch), typeof(LogViewerControl),
				new PropertyMetadata(default(ILogSourceSearch)));

		public static readonly DependencyProperty DataSourceProperty =
			DependencyProperty.Register("DataSource", typeof(IDataSourceViewModel), typeof(LogViewerControl),
				new PropertyMetadata(defaultValue: null, propertyChangedCallback: OnDataSourceChanged));

		public static readonly DependencyProperty LogEntryCountProperty =
			DependencyProperty.Register("LogEntryCount", typeof(int), typeof(LogViewerControl),
				new PropertyMetadata(defaultValue: 0));

		public static readonly DependencyProperty CurrentLogLineProperty =
			DependencyProperty.Register("CurrentLogLine", typeof(LogLineIndex), typeof(LogViewerControl),
				new PropertyMetadata(default(LogLineIndex), OnCurrentLogLineChanged));

		public static readonly DependencyProperty ShowLineNumbersProperty =
			DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(LogViewerControl),
				new PropertyMetadata(default(bool), OnShowLineNumbersChanged));

		public static readonly DependencyProperty ShowFatalProperty =
			DependencyProperty.Register("ShowFatal", typeof(bool), typeof(LogViewerControl),
				new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnFatalChanged));

		public static readonly DependencyProperty ShowErrorProperty =
			DependencyProperty.Register("ShowError", typeof(bool), typeof(LogViewerControl),
				new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnErrorChanged));

		public static readonly DependencyProperty ShowWarningProperty =
			DependencyProperty.Register("ShowWarning", typeof(bool), typeof(LogViewerControl),
				new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnWarningChanged));

		public static readonly DependencyProperty ShowInfoProperty =
			DependencyProperty.Register("ShowInfo", typeof(bool), typeof(LogViewerControl),
				new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnInfoChanged));

		public static readonly DependencyProperty ShowDebugProperty =
			DependencyProperty.Register("ShowDebug", typeof(bool), typeof(LogViewerControl),
				new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnDebugChanged));

		public static readonly DependencyProperty ShowTraceProperty =
			DependencyProperty.Register("ShowTrace", typeof(bool), typeof(LogViewerControl),
				new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnTraceChanged));

		public static readonly DependencyProperty ShowOtherProperty =
			DependencyProperty.Register("ShowOther", typeof(bool), typeof(LogViewerControl),
			                            new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnOtherChanged));

		public static readonly DependencyProperty ShowAllProperty =
			DependencyProperty.Register("ShowAll", typeof(bool?), typeof(LogViewerControl),
				new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnShowAllChanged));

		public static readonly DependencyProperty TotalLogEntryCountProperty =
			DependencyProperty.Register("TotalLogEntryCount", typeof(int), typeof(LogViewerControl),
				new PropertyMetadata(default(int)));

		public static readonly DependencyProperty ErrorIconProperty =
			DependencyProperty.Register("ErrorIcon", typeof(Geometry), typeof(LogViewerControl),
			                            new PropertyMetadata(default(Geometry)));

		public static readonly DependencyProperty ErrorMessageProperty =
			DependencyProperty.Register("ErrorMessage", typeof(string), typeof(LogViewerControl),
				new PropertyMetadata(default(string)));

		public static readonly DependencyProperty ErrorMessageActionProperty =
			DependencyProperty.Register("ErrorMessageAction", typeof(string), typeof(LogViewerControl),
				new PropertyMetadata(default(string)));

		public static readonly DependencyProperty MergedDataSourceDisplayModeProperty = DependencyProperty.Register(
			"MergedDataSourceDisplayMode", typeof(DataSourceDisplayMode), typeof(LogViewerControl),
			new PropertyMetadata(default(DataSourceDisplayMode), OnMergedDataSourceDisplayModeChanged));

		public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
		                                                "Settings", typeof(ILogViewerSettings), typeof(LogViewerControl),
		                                                new PropertyMetadata(null));

		private static void OnMergedDataSourceDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl) d).OnMergedDataSourceDisplayModeChanged((DataSourceDisplayMode)args.NewValue);
		}

		private void OnMergedDataSourceDisplayModeChanged(DataSourceDisplayMode displayMode)
		{
			var dataSource = DataSource as IMergedDataSourceViewModel;
			if (dataSource != null)
				dataSource.DisplayMode = displayMode;
		}

		private bool _changingLogView;

		public LogViewerControl()
		{
			InitializeComponent();

			PART_ListView.SelectionChanged += PartListViewOnSelectionChanged;
			PART_ListView.FollowTailChanged += OnFollowTailChanged;
			PART_ListView.HorizontalScrollBar.ValueChanged += HorizontalScrollBarOnValueChanged;
		}

		public DataSourceDisplayMode MergedDataSourceDisplayMode
		{
			get { return (DataSourceDisplayMode) GetValue(MergedDataSourceDisplayModeProperty); }
			set { SetValue(MergedDataSourceDisplayModeProperty, value); }
		}

		public LogViewerViewModel LogView
		{
			get { return (LogViewerViewModel) GetValue(LogViewProperty); }
			set { SetValue(LogViewProperty, value); }
		}

		public LogLineIndex CurrentLogLine
		{
			get { return (LogLineIndex) GetValue(CurrentLogLineProperty); }
			set { SetValue(CurrentLogLineProperty, value); }
		}

		public bool ShowLineNumbers
		{
			get { return (bool) GetValue(ShowLineNumbersProperty); }
			set { SetValue(ShowLineNumbersProperty, value); }
		}

		public ILogSource LogSource
		{
			get { return (ILogSource) GetValue(LogSourceProperty); }
			set { SetValue(LogSourceProperty, value); }
		}

		public IFindAllViewModel FindAll
		{
			get { return (IFindAllViewModel) GetValue(FindAllProperty); }
			set { SetValue(FindAllProperty, value); }
		}

		public ILogSourceSearch Search
		{
			get { return (ILogSourceSearch) GetValue(SearchProperty); }
			set { SetValue(SearchProperty, value); }
		}

		public Geometry ErrorIcon
		{
			get { return (Geometry) GetValue(ErrorMessageActionProperty); }
			set { SetValue(ErrorMessageActionProperty, value); }
		}

		public string ErrorMessageAction
		{
			get { return (string) GetValue(ErrorMessageActionProperty); }
			set { SetValue(ErrorMessageActionProperty, value); }
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

		public bool ShowOther
		{
			get { return (bool) GetValue(ShowOtherProperty); }
			set { SetValue(ShowOtherProperty, value); }
		}

		public bool ShowTrace
		{
			get { return (bool) GetValue(ShowTraceProperty); }
			set { SetValue(ShowTraceProperty, value); }
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

		public ILogViewerSettings Settings
		{
			get { return (ILogViewerSettings)GetValue(SettingsProperty); }
			set { SetValue(SettingsProperty, value); }
		}

		public IEnumerable<LogLineIndex> SelectedIndices => PART_ListView.SelectedIndices;

		public LogEntryListView PartListView => PART_ListView;

		public void Select(LogLineIndex index)
		{
			PART_ListView.Select(index);
		}

		public void Select(IEnumerable<LogLineIndex> indices)
		{
			PART_ListView.Select(indices);
		}

		public void Select(params LogLineIndex[] indices)
		{
			PART_ListView.Select(indices);
		}

		private void SetHorizontalOffset(double horizontalOffset)
		{
			PART_ListView.SetHorizontalOffset(horizontalOffset);
		}

		private void PartListViewOnSelectionChanged(IEnumerable<LogLineIndex> logLineIndices)
		{
			var dataSource = DataSource;
			if (dataSource != null)
				dataSource.SelectedLogLines = new HashSet<LogLineIndex>(logLineIndices);
		}

		private static void OnLogViewChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl) dependencyObject).OnLogViewChanged((LogViewerViewModel) args.OldValue,
				(LogViewerViewModel) args.NewValue);
		}

		private void OnLogViewChanged(LogViewerViewModel oldView, LogViewerViewModel newView)
		{
			try
			{
				_changingLogView = true;

				if (oldView != null)
					oldView.PropertyChanged -= LogViewOnPropertyChanged;

				if (newView != null)
				{
					newView.PropertyChanged += LogViewOnPropertyChanged;
					DataSource = newView.DataSource;
					LogSource = newView.LogSource;
					FindAll = newView.DataSource.FindAll;
					Search = newView.Search;
					CurrentLogLine = newView.DataSource.VisibleLogLine;

					Select(newView.DataSource.SelectedLogLines);
					SetHorizontalOffset(newView.DataSource.HorizontalOffset);
				}
				else
				{
					DataSource = null;
					LogSource = null;
					FindAll = null;
				}
			}
			finally
			{
				_changingLogView = false;
			}
		}

		private void OnDataSourceChanged(IDataSourceViewModel oldValue, IDataSourceViewModel newValue)
		{
			if (oldValue != null)
			{
				oldValue.OnRequestBringIntoView -= DataSourceOnRequestBringIntoView;
				oldValue.PropertyChanged -= DataSourceOnPropertyChanged;
				oldValue.Search.PropertyChanged -= SearchOnPropertyChanged;
			}
			if (newValue != null)
			{
				newValue.OnRequestBringIntoView += DataSourceOnRequestBringIntoView;
				newValue.PropertyChanged += DataSourceOnPropertyChanged;
				newValue.Search.PropertyChanged += SearchOnPropertyChanged;
				PART_ListView.FollowTail = newValue.FollowTail;
				PART_ListView.SelectedSearchResultIndex = newValue.Search.CurrentResultIndex;

				ShowLineNumbers = newValue.ShowLineNumbers;
				if (newValue is IMergedDataSourceViewModel merged)
					MergedDataSourceDisplayMode = merged.DisplayMode;
			}
			OnLevelsChanged();
		}

		private void DataSourceOnRequestBringIntoView(IDataSourceViewModel dataSource, LogLineIndex index)
		{
			PART_ListView.BringIntoView(index);
			PART_ListView.PartTextCanvas?.Focus();
		}

		private void LogViewOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch (args.PropertyName)
			{
				case nameof(LogViewerViewModel.LogSource):
					LogSource = LogView.LogSource;
					break;
			}
		}

		private static void OnCurrentLogLineChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl) dependencyObject).OnCurrentLogLineChanged((LogLineIndex) args.NewValue);
		}

		private void OnCurrentLogLineChanged(LogLineIndex index)
		{
			var dataSource = DataSource;
			if (dataSource != null)
				if (!_changingLogView)
					dataSource.VisibleLogLine = index;
		}

		private static void OnShowLineNumbersChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl) dependencyObject).OnShowLineNumbersChanged((bool) args.NewValue);
		}

		private void OnShowLineNumbersChanged(bool showLineNumbers)
		{
			var dataSource = DataSource;
			if (dataSource != null)
				dataSource.ShowLineNumbers = showLineNumbers;
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
				ShowOther = true;
				ShowTrace = true;
				ShowDebug = true;
				ShowInfo = true;
				ShowWarning = true;
				ShowError = true;
				ShowFatal = true;
			}
			else if (showAll == false)
			{
				ShowOther = false;
				ShowTrace = false;
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
				DataSource.LevelsFilter |= level;
			else
				DataSource.LevelsFilter &= ~level;
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
				DataSource.LevelsFilter |= level;
			else
				DataSource.LevelsFilter &= ~level;
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
				DataSource.LevelsFilter |= level;
			else
				DataSource.LevelsFilter &= ~level;
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
				DataSource.LevelsFilter |= level;
			else
				DataSource.LevelsFilter &= ~level;
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
				DataSource.LevelsFilter |= level;
			else
				DataSource.LevelsFilter &= ~level;
		}

		private static void OnOtherChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl) dependencyObject).OnOtherChanged((bool) args.NewValue);
		}

		private void OnOtherChanged(bool isChecked)
		{
			if (DataSource == null)
				return;
			
			const LevelFlags level = LevelFlags.Other;
			if (isChecked)
				DataSource.LevelsFilter |= level;
			else
				DataSource.LevelsFilter &= ~level;
		}

		private static void OnTraceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogViewerControl) dependencyObject).OnTraceChanged((bool) args.NewValue);
		}

		private void OnTraceChanged(bool isChecked)
		{
			if (DataSource == null)
				return;

			const LevelFlags level = LevelFlags.Trace;
			if (isChecked)
				DataSource.LevelsFilter |= level;
			else
				DataSource.LevelsFilter &= ~level;
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
			ShowTrace = levels.HasFlag(LevelFlags.Trace);
			ShowOther = levels.HasFlag(LevelFlags.Other);

			if (levels == LevelFlags.All)
				ShowAll = true;
			else if (levels == LevelFlags.None)
				ShowAll = false;
			else
				ShowAll = null;
		}

		private void DataSourceOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var dataSource = (IDataSourceViewModel) sender;
			switch (args.PropertyName)
			{
				case nameof(IDataSourceViewModel.FollowTail):
					PART_ListView.FollowTail = dataSource.FollowTail;
					break;

				case nameof(IDataSourceViewModel.ShowLineNumbers):
					PART_ListView.ShowLineNumbers = dataSource.ShowLineNumbers;
					break;

				case nameof(IDataSourceViewModel.ShowElapsedTime):
					PART_ListView.ShowElapsedTime = dataSource.ShowElapsedTime;
					break;

				case nameof(IDataSourceViewModel.ShowDeltaTimes):
					PART_ListView.ShowDeltaTimes = dataSource.ShowDeltaTimes;
					break;

				case nameof(IDataSourceViewModel.ColorByLevel):
					PART_ListView.ColorByLevel = dataSource.ColorByLevel;
					break;

				case nameof(IMergedDataSourceViewModel.DisplayMode):
					if (dataSource is IMergedDataSourceViewModel mergedDataSource)
						MergedDataSourceDisplayMode = mergedDataSource.DisplayMode;
					break;

				case "LevelsFilter":
					OnLevelsChanged();
					break;

				case "VisibleLogLine":
					CurrentLogLine = dataSource.VisibleLogLine;
					break;

				case "SelectedLogLines":
					PART_ListView.SelectedIndices = dataSource.SelectedLogLines;
					break;
			}
		}

		private void SearchOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var search = (ISearchViewModel) sender;
			switch (args.PropertyName)
			{
				case nameof(ISearchViewModel.CurrentResultIndex):
					PART_ListView.SelectedSearchResultIndex = search.CurrentResultIndex;
					break;
			}
		}

		private void OnFollowTailChanged(bool followTail)
		{
			var dataSource = DataSource;
			if (dataSource != null)
				dataSource.FollowTail = followTail;
		}

		private void HorizontalScrollBarOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
		{
			var dataSource = DataSource;
			if (dataSource != null && !_changingLogView)
				dataSource.HorizontalOffset = args.NewValue;
		}

		private void OverlayOnMouseDown(object sender, MouseButtonEventArgs e)
		{
			DataSourceToggleButton.IsChecked = false;
		}
	}
}