using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using log4net;
using Metrolib.Controls;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.LogView.Any;
using Tailviewer.Ui.Controls.LogView.DataSource;
using Tailviewer.Ui.Controls.LogView.DeltaTimes;
using Tailviewer.Ui.Controls.LogView.ElapsedTime;
using Tailviewer.Ui.Controls.LogView.LineNumbers;
using Tailviewer.Ui.Controls.LogView.LogLevels;
using Tailviewer.Ui.Controls.LogView.Messages;
using Tailviewer.Ui.Controls.LogView.Timestamps;

namespace Tailviewer.Ui.Controls.LogView
{
	public class LogEntryListView
		: Grid
		, ILogFileListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly DependencyProperty DataSourceProperty =
			DependencyProperty.Register("DataSource", typeof(IDataSource), typeof(LogEntryListView),
				new PropertyMetadata(propertyChangedCallback: null));

		public static readonly DependencyProperty LogFileProperty =
			DependencyProperty.Register("LogFile", typeof(ILogFile), typeof(LogEntryListView),
				new PropertyMetadata(defaultValue: null, propertyChangedCallback: OnLogFileChanged));

		public static readonly DependencyProperty SearchProperty =
			DependencyProperty.Register("Search", typeof(ILogFileSearch), typeof(LogEntryListView),
				new PropertyMetadata(defaultValue: null, propertyChangedCallback: OnSearchChanged));

		public static readonly DependencyProperty FollowTailProperty =
			DependencyProperty.Register("FollowTail", typeof(bool), typeof(LogEntryListView),
				new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnFollowTailChanged));

		public static readonly DependencyProperty ShowLineNumbersProperty =
			DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(LogEntryListView),
				new PropertyMetadata(defaultValue: true, propertyChangedCallback: OnShowLineNumbersChanged));

		public static readonly DependencyProperty ShowDeltaTimesProperty =
			DependencyProperty.Register("ShowDeltaTimes", typeof(bool), typeof(LogEntryListView),
			                            new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnShowDeltaTimesChanged));

		public static readonly DependencyProperty ShowElapsedTimeProperty =
			DependencyProperty.Register("ShowElapsedTime", typeof(bool), typeof(LogEntryListView),
			                            new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnShowElapsedTimeChanged));

		public static readonly DependencyProperty CurrentLineProperty =
			DependencyProperty.Register("CurrentLine", typeof(LogLineIndex), typeof(LogEntryListView),
				new PropertyMetadata(default(LogLineIndex), OnCurrentLineChanged));

		public static readonly DependencyProperty ColorByLevelProperty =
			DependencyProperty.Register("ColorByLevel", typeof(bool), typeof(LogEntryListView),
				new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnColorByLevelChanged));

		public static readonly DependencyProperty MergedDataSourceDisplayModeProperty = DependencyProperty.Register(
			"MergedDataSourceDisplayMode", typeof(DataSourceDisplayMode), typeof(LogEntryListView),
			new PropertyMetadata(default(DataSourceDisplayMode), OnMergedDataSourceDisplayModeChanged));

		public static readonly DependencyProperty SelectedIndicesProperty = DependencyProperty.Register(
			"SelectedIndices", typeof(IEnumerable<LogLineIndex>), typeof(LogEntryListView), new PropertyMetadata(new LogLineIndex[0], OnSelectedIndicesChanged));

		public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
		                                                "Settings", typeof(ILogViewerSettings), typeof(LogEntryListView), new PropertyMetadata(null, OnSettingsChanged));

		public static readonly TimeSpan MaximumRefreshInterval = TimeSpan.FromMilliseconds(value: 33);

		private readonly IReadOnlyDictionary<ILogFileColumnDescriptor, Func<TextSettings, AbstractLogColumnPresenter>>
			_columnPresenterFactories;
		private readonly Dictionary<ILogFileColumnDescriptor, AbstractLogColumnPresenter> _columnPresenters;
		private readonly Dictionary<ILogFileColumnDescriptor, ColumnDefinition> _columnDefinitionsByColumn;
		private readonly Dictionary<ColumnDefinition, ILogFileColumnDescriptor> _columnsByColumnDefinition;
		private readonly IReadOnlyList<ILogFileColumnDescriptor> _fixedColumns;
		private readonly ColumnDefinition _messageColumnDefinition;

		private readonly DataSourceCanvas _dataSourceCanvas;
		private readonly DeltaTimeColumnPresenter _deltaTimesColumn;
		private readonly ElapsedTimeColumnPresenter _elapsedTimeColumn;

		private readonly FlatScrollBar _horizontalScrollBar;

		private readonly OriginalLineNumberColumnPresenter _lineNumberColumn;
		private readonly DispatcherTimer _timer;
		private readonly FlatScrollBar _verticalScrollBar;

		private int _maxLineWidth;
		private int _pendingModificationsCount;
		private TextSettings _textSettings;
		private TextBrushes _textBrushes;

		static LogEntryListView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LogEntryListView),
				new FrameworkPropertyMetadata(typeof(LogEntryListView)));
		}

		public LogEntryListView()
		{
			var textSettings = new TextSettings();
			var textBrushes = new TextBrushes(null);

			RowDefinitions.Add(new RowDefinition { Height = new GridLength(value: 1, type: GridUnitType.Star) });
			RowDefinitions.Add(new RowDefinition { Height = new GridLength(value: 1, type: GridUnitType.Auto) });

			_verticalScrollBar = new FlatScrollBar
			{
				Name = "PART_VerticalScrollBar",
				Thickness = 18
			};
			_verticalScrollBar.ValueChanged += VerticalScrollBarOnValueChanged;
			_verticalScrollBar.Scroll += VerticalScrollBarOnScroll;
			_verticalScrollBar.SetValue(RowProperty, value: 0);
			_verticalScrollBar.SetValue(ColumnProperty, value: 6);

			_horizontalScrollBar = new FlatScrollBar
			{
				Name = "PART_HorizontalScrollBar",
				Orientation = Orientation.Horizontal,
				Thickness = 18
			};
			_horizontalScrollBar.SetValue(RowProperty, value: 1);
			_horizontalScrollBar.SetValue(ColumnProperty, value: 0);
			_horizontalScrollBar.SetValue(ColumnSpanProperty, value: 6);

			_columnPresenterFactories = new Dictionary<ILogFileColumnDescriptor, Func<TextSettings, AbstractLogColumnPresenter>>
			{
				{LogFileColumns.DeltaTime, settings => new DeltaTimeColumnPresenter(settings)},
				{LogFileColumns.ElapsedTime, settings => new ElapsedTimeColumnPresenter(settings) },
				{LogFileColumns.OriginalLineNumber, settings => new OriginalLineNumberColumnPresenter(settings) },
				{LogFileColumns.LogLevel, settings => new  LogLevelColumnPresenter(settings)},
				{LogFileColumns.Message, settings => new MessageColumnPresenter(settings) },
				{LogFileColumns.Timestamp, settings => new TimestampColumnPresenter(settings)}
			};
			_columnPresenters = new Dictionary<ILogFileColumnDescriptor, AbstractLogColumnPresenter>();
			_columnDefinitionsByColumn = new Dictionary<ILogFileColumnDescriptor, ColumnDefinition>();
			_columnsByColumnDefinition = new Dictionary<ColumnDefinition, ILogFileColumnDescriptor>();

			_lineNumberColumn = (OriginalLineNumberColumnPresenter) AddColumn(LogFileColumns.OriginalLineNumber);
			_lineNumberColumn.SetValue(MarginProperty, new Thickness(left: 5, top: 0, right: 5, bottom: 0));

			_dataSourceCanvas = new DataSourceCanvas(textSettings);
			_dataSourceCanvas.SetValue(RowProperty, value: 0);
			_dataSourceCanvas.SetValue(ColumnProperty, value: 1);
			_dataSourceCanvas.SetValue(MarginProperty, new Thickness(left: 0, top: 0, right: 5, bottom: 0));
			ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(value: 1, type: GridUnitType.Auto) });

			_deltaTimesColumn = (DeltaTimeColumnPresenter) AddColumn(LogFileColumns.DeltaTime);
			_deltaTimesColumn.Visibility = Visibility.Collapsed;
			_deltaTimesColumn.SetValue(MarginProperty, new Thickness(left: 0, top: 0, right: 5, bottom: 0));

			_elapsedTimeColumn = (ElapsedTimeColumnPresenter) AddColumn(LogFileColumns.ElapsedTime);
			_elapsedTimeColumn.Visibility = Visibility.Collapsed;
			_elapsedTimeColumn.SetValue(MarginProperty, new Thickness(left: 0, top: 0, right: 5, bottom: 0));

			_fixedColumns = _columnPresenters.Keys.ToList();

			_messageColumnDefinition = new ColumnDefinition { Width = new GridLength(value: 1, type: GridUnitType.Star) };
			ColumnDefinitions.Add(_messageColumnDefinition);
			var messageColumnIndex = ColumnDefinitions.IndexOf(_messageColumnDefinition);

			PartTextCanvas = new TextCanvas(_horizontalScrollBar, _verticalScrollBar, textSettings);
			PartTextCanvas.SetValue(RowProperty, value: 0);
			PartTextCanvas.SetValue(ColumnProperty, value: messageColumnIndex);
			PartTextCanvas.MouseWheelDown += TextCanvasOnMouseWheelDown;
			PartTextCanvas.MouseWheelUp += TextCanvasOnMouseWheelUp;
			PartTextCanvas.SizeChanged += TextCanvasOnSizeChanged;
			PartTextCanvas.VisibleLinesChanged += TextCanvasOnVisibleLinesChanged;
			PartTextCanvas.RequestBringIntoView += TextCanvasOnRequestBringIntoView;
			PartTextCanvas.VisibleSectionChanged += TextCanvasOnVisibleSectionChanged;
			PartTextCanvas.OnSelectionChanged += TextCanvasOnOnSelectionChanged;

			ChangeTextSettings(textSettings, textBrushes);

			var separator = new Rectangle
			{
				Fill = new SolidColorBrush(Color.FromRgb(225, 228, 232)),
				Width = 2
			};
			separator.SetValue(RowProperty, value: 0);
			separator.SetValue(ColumnProperty, value: messageColumnIndex);
			separator.SetValue(MarginProperty, new Thickness(left: 0, top: 0, right: 5, bottom: 0));
			ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(value: 1, type: GridUnitType.Auto) });

			Children.Add(_lineNumberColumn);
			Children.Add(_dataSourceCanvas);
			Children.Add(_deltaTimesColumn);
			Children.Add(_elapsedTimeColumn);
			Children.Add(separator);
			Children.Add(PartTextCanvas);
			Children.Add(_verticalScrollBar);
			Children.Add(_horizontalScrollBar);

			_timer = new DispatcherTimer(DispatcherPriority.Normal)
				{
					Interval = MaximumRefreshInterval,
				};
			_timer.Tick += OnTimer;
			Loaded += OnLoaded;
			Unloaded +=  OnUnloaded;
		}

		public DataSourceDisplayMode MergedDataSourceDisplayMode
		{
			get { return (DataSourceDisplayMode) GetValue(MergedDataSourceDisplayModeProperty); }
			set { SetValue(MergedDataSourceDisplayModeProperty, value); }
		}

		public bool ColorByLevel
		{
			get { return (bool) GetValue(ColorByLevelProperty); }
			set { SetValue(ColorByLevelProperty, value); }
		}

		public LogLineIndex CurrentLine
		{
			get { return (LogLineIndex) GetValue(CurrentLineProperty); }
			set { SetValue(CurrentLineProperty, value); }
		}

		public bool ShowLineNumbers
		{
			get { return (bool) GetValue(ShowLineNumbersProperty); }
			set { SetValue(ShowLineNumbersProperty, value); }
		}

		public bool ShowDeltaTimes
		{
			get { return (bool)GetValue(ShowDeltaTimesProperty); }
			set { SetValue(ShowDeltaTimesProperty, value); }
		}
		
		public bool ShowElapsedTime
		{
			get { return (bool)GetValue(ShowElapsedTimeProperty); }
			set { SetValue(ShowElapsedTimeProperty, value); }
		}

		public bool FollowTail
		{
			get { return (bool) GetValue(FollowTailProperty); }
			set { SetValue(FollowTailProperty, value); }
		}

		public int PendingModificationsCount => _pendingModificationsCount;

		public ScrollBar VerticalScrollBar => _verticalScrollBar;

		public ScrollBar HorizontalScrollBar => _horizontalScrollBar;

		public IDataSource DataSource
		{
			get { return (IDataSource) GetValue(DataSourceProperty); }
			set { SetValue(DataSourceProperty, value); }
		}

		public ILogFile LogFile
		{
			get { return (ILogFile) GetValue(LogFileProperty); }
			set { SetValue(LogFileProperty, value); }
		}

		public ILogFileSearch Search
		{
			get { return (ILogFileSearch) GetValue(SearchProperty); }
			set { SetValue(SearchProperty, value); }
		}

		public List<TextLine> VisibleTextLines => PartTextCanvas.VisibleTextLines;

		public IEnumerable<LogLineIndex> SelectedIndices
		{
			get { return (IEnumerable<LogLineIndex>)GetValue(SelectedIndicesProperty); }
			set { SetValue(SelectedIndicesProperty, value); }
		}

		public TextCanvas PartTextCanvas { get; }

		public int SelectedSearchResultIndex
		{
			get { return PartTextCanvas.SelectedSearchResultIndex; }
			set { PartTextCanvas.SelectedSearchResultIndex = value; }
		}

		public ILogViewerSettings Settings
		{
			get { return (ILogViewerSettings)GetValue(SettingsProperty); }
			set { SetValue(SettingsProperty, value); }
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			var width = _textSettings.EstimateWidthUpperLimit(logFile.MaxCharactersPerLine);
			var upperWidth = (int) Math.Ceiling(width);

			// Setting an integer is an atomic operation and thus no
			// special synchronization is required.
			_maxLineWidth = Math.Max(_maxLineWidth, upperWidth);

			Interlocked.Increment(ref _pendingModificationsCount);
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			_timer.Start();
		}

		private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
		{
			_timer.Stop();
		}

		private void ChangeDisplay(bool gridDisplay)
		{
			if (gridDisplay)
			{
				var columns = LogFile.Columns;
				var missingColumns = columns.Except(_columnPresenters.Keys).ToList();
				var superfluousColumns = _columnPresenters.Keys.Except(columns).ToList();

				foreach (var column in superfluousColumns)
				{
					RemoveColumn(column);
				}

				foreach (var column in missingColumns)
				{
					AddColumn(column);
				}
			}
			else
			{
				var superfluousColumns = _columnPresenters.Keys.Except(_fixedColumns).ToList();
				foreach (var column in superfluousColumns)
				{
					RemoveColumn(column);
				}
			}
		}

		private AbstractLogColumnPresenter CreatePresenterFor(ILogFileColumnDescriptor column)
		{
			if (_columnPresenterFactories.TryGetValue(column, out var factory))
			{
				return factory(_textSettings);
			}

			var columnPresenterType = typeof(AnyColumnPresenter<>).MakeGenericType(column.DataType);
			var presenter = (AbstractLogColumnPresenter)Activator.CreateInstance(columnPresenterType, new object[] {column, _textSettings});
			return presenter;
		}

		private AbstractLogColumnPresenter AddColumn(ILogFileColumnDescriptor column)
		{
			var columnPresenter = CreatePresenterFor(column);

			// The 2nd column is reserved for the data source name which hasn't been
			// ported to the new ILogFileColumnPresenter interface just yet
			int columnIndex;
			if (_columnPresenters.Count == 0)
			{
				columnIndex = 0;
			}
			else if (Equals(column, LogFileColumns.Message))
			{
				columnIndex = ColumnDefinitions.IndexOf(_messageColumnDefinition);
			}
			else
			{
				columnIndex = _columnPresenters.Count + 1;
			}

			columnPresenter.SetValue(RowProperty, 0);
			columnPresenter.SetValue(ColumnProperty, columnIndex);
			_columnPresenters.Add(columnPresenter.Column, columnPresenter);

			var columnDefinition = new ColumnDefinition {Width = new GridLength(value: 1, type: GridUnitType.Auto)};
			_columnDefinitionsByColumn.Add(columnPresenter.Column, columnDefinition);
			_columnsByColumnDefinition.Add(columnDefinition, columnPresenter.Column);

			int insertionIndex;
			if (_messageColumnDefinition != null)
			{
				insertionIndex = ColumnDefinitions.IndexOf(_messageColumnDefinition);
			}
			else
			{
				insertionIndex = ColumnDefinitions.Count;
			}
			ColumnDefinitions.Insert(insertionIndex, columnDefinition);

			return columnPresenter;
		}

		private void RemoveColumn(ILogFileColumnDescriptor column)
		{
			if (!_columnPresenters.Remove(column))
			{
				Log.WarnFormat("Unable to remove column '{0}' it's not presented", column);
				return;
			}

			if (!_columnDefinitionsByColumn.TryGetValue(column, out var columnDefinition))
			{
				Log.ErrorFormat("Inconsistency detected: A column presenter exists, but not column definition for column '{0}'!", column);
				return;
			}

			var firstIndex = ColumnDefinitions.IndexOf(columnDefinition);
			_columnDefinitionsByColumn.Remove(column);
			_columnsByColumnDefinition.Remove(columnDefinition);
			ColumnDefinitions.Remove(columnDefinition);

			OffsetColumnsFrom(firstIndex, -1);
		}

		private void OffsetColumnsFrom(int firstColumnIndex, int offset)
		{
			for (int i = firstColumnIndex; i < ColumnDefinitions.Count; ++i)
			{
				var columnDefinition = ColumnDefinitions[i];
				var column = _columnsByColumnDefinition[columnDefinition];
				var presenter = _columnPresenters[column];

				var currentColumn = (int) presenter.GetValue(Grid.ColumnProperty);
				presenter.SetValue(Grid.ColumnProperty, currentColumn + offset);
			}
		}

		private static void OnMergedDataSourceDisplayModeChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs args)
		{
			((LogEntryListView) dependencyObject).OnMergedDataSourceDisplayModeChanged((DataSourceDisplayMode) args.NewValue);
		}

		private void OnMergedDataSourceDisplayModeChanged(DataSourceDisplayMode displayMode)
		{
			_dataSourceCanvas.DisplayMode = displayMode;
		}

		private static void OnSearchChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogEntryListView) dependencyObject).OnSearchChanged((ILogFileSearch) args.NewValue);
		}

		private void OnSearchChanged(ILogFileSearch search)
		{
			PartTextCanvas.Search = search;
		}

		private static void OnColorByLevelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogEntryListView) dependencyObject).OnColorByLevelChanged((bool) args.NewValue);
		}

		private void OnColorByLevelChanged(bool colorByLevel)
		{
			PartTextCanvas.ColorByLevel = colorByLevel;
		}

		private static void OnCurrentLineChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			((LogEntryListView) dependencyObject).OnCurrentLineChanged((LogLineIndex) args.NewValue);
		}

		private static void OnShowLineNumbersChanged(DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs args)
		{
			((LogEntryListView) dependencyObject).OnShowLineNumbersChanged((bool) args.NewValue);
		}

		private void OnShowLineNumbersChanged(bool showLineNumbers)
		{
			_lineNumberColumn.Visibility = showLineNumbers
				? Visibility.Visible
				: Visibility.Collapsed;
			UpdateColumn(_lineNumberColumn);
		}
		
		private static void OnShowElapsedTimeChanged(DependencyObject dependencyObject,
		                                            DependencyPropertyChangedEventArgs args)
		{
			((LogEntryListView)dependencyObject).OnShowElapsedTimeChanged((bool)args.NewValue);
		}

		private void OnShowElapsedTimeChanged(bool showLineNumbers)
		{
			_elapsedTimeColumn.Visibility = showLineNumbers
				? Visibility.Visible
				: Visibility.Collapsed;
			UpdateColumn(_elapsedTimeColumn);
		}

		private static void OnShowDeltaTimesChanged(DependencyObject dependencyObject,
		                                             DependencyPropertyChangedEventArgs args)
		{
			((LogEntryListView)dependencyObject).OnShowDeltaTimesChanged((bool)args.NewValue);
		}

		private void OnShowDeltaTimesChanged(bool showLineNumbers)
		{
			_deltaTimesColumn.Visibility = showLineNumbers
				? Visibility.Visible
				: Visibility.Collapsed;
			UpdateColumn(_deltaTimesColumn);
		}

		private void OnCurrentLineChanged(LogLineIndex index)
		{
			var current = PartTextCanvas.CurrentlyVisibleSection;
			if (index != LogLineIndex.Invalid)
			{
				PartTextCanvas.CurrentLine = (int) index;
				//< We don't want to call BringIntoView() everytime because that one scrolls to fully
				// bring a line into view. This would mean that if the currently visible section changed
				// so that the top line is only partially visible, then this method would always bring
				// it fully visible. Removing the following filter would completely remove per pixel scrolling...
				if (current.Index != index)
					_verticalScrollBar.Value = (int) index * _textSettings.LineHeight;
			}
		}

		private void TextCanvasOnVisibleSectionChanged(LogFileSection section)
		{
			CurrentLine = section.Index;
		}

		private void TextCanvasOnOnSelectionChanged(HashSet<LogLineIndex> selectedIndices)
		{
			SelectedIndices = selectedIndices.ToList();
			SelectionChanged?.Invoke(selectedIndices);
		}

		public event Action<IEnumerable<LogLineIndex>> SelectionChanged;

		private void TextCanvasOnRequestBringIntoView(LogLineIndex logLineIndex, LogLineMatch match)
		{
			BringIntoView(logLineIndex, match);
		}

		public void BringIntoView(LogLineIndex logLineIndex, LogLineMatch match = new LogLineMatch())
		{
			var height = PartTextCanvas.ActualHeight;
			var offset = PartTextCanvas.YOffset;
			var start = PartTextCanvas.CurrentLine;
			var diff = logLineIndex - start;
			var minY = diff * _textSettings.LineHeight + offset;
			var maxY = (diff + 1) * _textSettings.LineHeight + offset;
			if (minY < 0)
				_verticalScrollBar.Value += minY;
			else if (maxY > height)
				_verticalScrollBar.Value += maxY - height;

			var minX = _textSettings.GlyphWidth * match.Index;
			var maxX = _textSettings.GlyphWidth * (match.Index + match.Count);
			var visibleMinX = _horizontalScrollBar.Value;
			var visibleMaxX = visibleMinX + _horizontalScrollBar.ViewportSize;
			if (maxX < visibleMinX || minX > visibleMaxX)
				_horizontalScrollBar.Value = minX;

			var logFile = LogFile;
			if (logFile != null)
			{
				var count = logFile.Count;
				if (count > 0)
					FollowTail = logLineIndex >= count - 1;
			}
		}

		private void TextCanvasOnVisibleLinesChanged()
		{
			foreach (var columnPresenter in _columnPresenters.Values)
			{
				UpdateColumn(columnPresenter);
			}

			_dataSourceCanvas.UpdateDataSources(DataSource,
			                                    PartTextCanvas.CurrentlyVisibleSection,
			                                    PartTextCanvas.YOffset);
		}

		private void UpdateColumn(ILogFileColumnPresenter column)
		{
			column.FetchValues(LogFile,
			                   PartTextCanvas.CurrentlyVisibleSection,
			                   PartTextCanvas.YOffset);
		}

		private void TextCanvasOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			UpdateScrollViewerRegions();
		}

		private void TextCanvasOnMouseWheelUp()
		{
			var delta = _verticalScrollBar.Value - _verticalScrollBar.Minimum;
			var linesToScroll = Settings?.LinesScrolledPerWheelTick ?? LogViewerSettings.DefaultLinesScrolledPerWheelTick;
			var heightToScroll = linesToScroll * _textSettings.LineHeight;
			var toScroll = Math.Min(delta, heightToScroll);

			if (toScroll > 0)
			{
				FollowTail = false;
				_verticalScrollBar.Value -= toScroll;
			}
		}

		private void TextCanvasOnMouseWheelDown()
		{
			var delta = _verticalScrollBar.Maximum - _verticalScrollBar.Value;
			var linesToScroll = Settings?.LinesScrolledPerWheelTick ?? LogViewerSettings.DefaultLinesScrolledPerWheelTick;
			var heightToScroll = linesToScroll * _textSettings.LineHeight;
			// We might not be able to scroll that far because there's less content available so we'll have to clamp
			// that value...
			var maximumPossibleScroll = Math.Min(delta, heightToScroll);
			_verticalScrollBar.Value += maximumPossibleScroll;

			if (_verticalScrollBar.Value >= _verticalScrollBar.Maximum)
				FollowTail = true;
		}

		private static void OnSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LogEntryListView) d).OnSettingsChanged((ILogViewerSettings)e.NewValue);
		}

		private void OnSettingsChanged(ILogViewerSettings settings)
		{
			var textSettings = settings != null
				? new TextSettings(settings.FontSize, settings.TabWidth)
				: TextSettings.Default;
			var textBrushes = new TextBrushes(settings);
			ChangeTextSettings(textSettings, textBrushes);
		}

		private void ChangeTextSettings(TextSettings textSettings, TextBrushes textBrushes)
		{
			_textSettings = textSettings;
			_textBrushes = textBrushes;

			_verticalScrollBar.SetValue(RangeBase.SmallChangeProperty, _textSettings.LineHeight);
			_verticalScrollBar.SetValue(RangeBase.LargeChangeProperty, 10 * _textSettings.LineHeight);

			_horizontalScrollBar.SetValue(RangeBase.SmallChangeProperty, _textSettings.LineHeight);
			_horizontalScrollBar.SetValue(RangeBase.LargeChangeProperty, 10 * _textSettings.LineHeight);

			foreach (var columnPresenter in _columnPresenters.Values)
			{
				columnPresenter.TextSettings = _textSettings;
			}
			PartTextCanvas.ChangeTextSettings(_textSettings, _textBrushes);

			if (LogFile != null)
			{
				TextCanvasOnVisibleLinesChanged();
			}
		}

		private static void OnFollowTailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LogEntryListView) d).OnFollowTailChanged((bool) e.NewValue);
		}

		public event Action<bool> FollowTailChanged;

		private void OnFollowTailChanged(bool followTail)
		{
			if (followTail)
				ScrollToBottom();

			FollowTailChanged?.Invoke(followTail);
		}

		private void ScrollToBottom()
		{
			_verticalScrollBar.Value = _verticalScrollBar.Maximum;
		}

		public void OnTimer(object sender, EventArgs args)
		{
			if (Interlocked.Exchange(ref _pendingModificationsCount, value: 0) > 0)
			{
				try
				{
					PartTextCanvas.DetermineVerticalOffset();
					PartTextCanvas.CurrentlyVisibleSection = CalculateVisibleSection();

					UpdateScrollViewerRegions();
					ScrollToBottomIfRequired();

					PartTextCanvas.UpdateVisibleLines();
					PartTextCanvas.OnMouseMove();
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception while updating: {0}", e);

					// Common sense says that functions which fail once work when tried again.
					// The same goes for this control: If drawing fails then it's conceivable
					// that it will work when we try again in a few milliseconds...
					Interlocked.Increment(ref _pendingModificationsCount);
				}
			}
		}

		private void ScrollToBottomIfRequired()
		{
			if (FollowTail)
				ScrollToBottom();
		}

		/// <summary>
		///     The section of the log file that is currently visible.
		/// </summary>
		[Pure]
		public LogFileSection CalculateVisibleSection()
		{
			return PartTextCanvas.CalculateVisibleSection();
		}

		private static void OnLogFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LogEntryListView) d).OnLogFileChanged(e.OldValue as ILogFile, e.NewValue as ILogFile);
		}

		private void OnLogFileChanged(ILogFile oldValue, ILogFile newValue)
		{
			oldValue?.RemoveListener(this);

			PartTextCanvas.LogFile = newValue;

			if (newValue != null)
			{
				newValue.AddListener(this, TimeSpan.FromMilliseconds(value: 100), maximumLineCount: 10000);

				_maxLineWidth = (int) Math.Ceiling(_textSettings.EstimateWidthUpperLimit(newValue.MaxCharactersPerLine));
				UpdateScrollViewerRegions();
				PartTextCanvas.DetermineVerticalOffset();
				PartTextCanvas.UpdateVisibleSection();
				PartTextCanvas.UpdateVisibleLines();
			}
			else
			{
				_maxLineWidth = 0;
			}

			MatchScrollbarValueToCurrentLine();
		}

		private void MatchScrollbarValueToCurrentLine()
		{
			var value = PartTextCanvas.CurrentLine * _textSettings.LineHeight;
			if (value >= 0 && value <= _verticalScrollBar.Maximum)
				_verticalScrollBar.Value = value;
			else
				_verticalScrollBar.Value = 0;
		}

		private void UpdateScrollViewerRegions()
		{
			if (LogFile == null)
			{
			}
			else
			{
				//
				// Depending on the current size of the logfile and the size of the viewport,
				// we need to determine the range of our scrollers.
				//

				UpdateHorizontalScrollbar();
				UpdateVerticalScrollbar();
			}
		}

		private void UpdateHorizontalScrollbar()
		{
			double totalWidth = _maxLineWidth;
			var usableWidth = PartTextCanvas.ActualWidth;
			if (totalWidth > usableWidth)
			{
				_horizontalScrollBar.Minimum = 0;
				_horizontalScrollBar.Maximum = totalWidth - usableWidth;
				_horizontalScrollBar.ViewportSize = usableWidth;
				_horizontalScrollBar.Visibility = Visibility.Visible;
			}
			else
			{
				_horizontalScrollBar.Minimum = 0;
				_horizontalScrollBar.Maximum = 0;
				_horizontalScrollBar.ViewportSize = usableWidth;
				_horizontalScrollBar.Visibility = Visibility.Collapsed;
			}
		}

		private void UpdateVerticalScrollbar()
		{
			var count = LogFile.Count;
			var totalHeight = count * _textSettings.LineHeight;
			var usableHeight = PartTextCanvas.ActualHeight;
			if (totalHeight > usableHeight)
			{
				_verticalScrollBar.Minimum = 0;
				_verticalScrollBar.Maximum = Math.Ceiling((totalHeight - usableHeight) / _textSettings.LineHeight) *
				                             _textSettings.LineHeight;
				_verticalScrollBar.ViewportSize = usableHeight;
				_verticalScrollBar.Visibility = Visibility.Visible;
			}
			else
			{
				_verticalScrollBar.Minimum = 0;
				_verticalScrollBar.Maximum = 0;
				_verticalScrollBar.ViewportSize = usableHeight;
				_verticalScrollBar.Visibility = Visibility.Collapsed;
			}
		}

		private void VerticalScrollBarOnScroll(object sender, ScrollEventArgs scrollEventArgs)
		{
			if (_lastScroll.NewValue < _lastScroll.OldValue)
				FollowTail = false;
			if (scrollEventArgs.NewValue >= _verticalScrollBar.Maximum)
				FollowTail = true;
		}

		private void VerticalScrollBarOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
		{
			_lastScroll = new ScrollEvent
			{
				OldValue = args.OldValue,
				NewValue = args.NewValue
			};
		}

		private static void OnSelectedIndicesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LogEntryListView) d).OnSelectedIndicesChanged((IEnumerable<LogLineIndex>) e.NewValue);
		}

		private void OnSelectedIndicesChanged(IEnumerable<LogLineIndex> selectedIndices)
		{
			PartTextCanvas.SelectedIndices = selectedIndices;
		}

		public void Select(LogLineIndex index)
		{
			PartTextCanvas.SetSelected(index, SelectMode.Replace);
		}

		public void Select(IEnumerable<LogLineIndex> indices)
		{
			PartTextCanvas.SetSelected(indices, SelectMode.Replace);
		}

		public void Select(params LogLineIndex[] indices)
		{
			PartTextCanvas.SetSelected(indices, SelectMode.Replace);
		}

		public void CopySelectedLinesToClipboard()
		{
			PartTextCanvas.OnCopyToClipboard();
		}

		public void SetHorizontalOffset(double value)
		{
			if (value < _horizontalScrollBar.Minimum)
				_horizontalScrollBar.Value = _horizontalScrollBar.Minimum;
			else if (value > _horizontalScrollBar.Maximum)
				_horizontalScrollBar.Value = _horizontalScrollBar.Maximum;
			else
				_horizontalScrollBar.Value = value;
		}

		#region Scrolling

		private ScrollEvent _lastScroll;

		private struct ScrollEvent
		{
			public double NewValue;
			public double OldValue;
		}

		#endregion
	}
}
