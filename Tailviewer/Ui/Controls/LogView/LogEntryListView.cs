using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Ui.Converters;
using log4net;

namespace Tailviewer.Ui.Controls.LogView
{
	public class LogEntryListView
		: Grid
		  , ILogFileListener
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly DependencyProperty LogFileProperty =
			DependencyProperty.Register("LogFile", typeof (ILogFile), typeof (LogEntryListView),
			                            new PropertyMetadata(null, OnLogFileChanged));

		public static readonly DependencyProperty ForegroundBrushConverterProperty =
			DependencyProperty.Register("ForegroundBrushConverter", typeof (LevelToBrushConverter), typeof (LogEntryListView),
			                            new PropertyMetadata(default(LevelToBrushConverter)));

		public static readonly DependencyProperty BackgroundBrushConverterProperty =
			DependencyProperty.Register("BackgroundBrushConverter", typeof (LevelToBrushConverter), typeof (LogEntryListView),
			                            new PropertyMetadata(default(LevelToBrushConverter)));

		public static readonly DependencyProperty HoveredBackgroundBrushConverterProperty =
			DependencyProperty.Register("HoveredBackgroundBrushConverter", typeof (LevelToBrushConverter),
			                            typeof (LogEntryListView), new PropertyMetadata(default(LevelToBrushConverter)));

		public static readonly DependencyProperty FollowTailProperty =
			DependencyProperty.Register("FollowTail", typeof (bool), typeof (LogEntryListView),
			                            new PropertyMetadata(false, OnFollowTailChanged));

		public static readonly DependencyProperty ShowLineNumbersProperty =
			DependencyProperty.Register("ShowLineNumbers", typeof (bool), typeof (LogEntryListView),
			                            new PropertyMetadata(true, OnShowLineNumbersChanged));

		public static readonly DependencyProperty CurrentLineProperty =
			DependencyProperty.Register("CurrentLine", typeof (LogLineIndex), typeof (LogEntryListView),
			                            new PropertyMetadata(default(LogLineIndex), OnCurrentLineChanged));

		internal static readonly TimeSpan MaximumRefreshInterval = TimeSpan.FromMilliseconds(33);
		private readonly Rectangle _cornerRectangle;

		private readonly ScrollBar _horizontalScrollBar;
		private readonly LineNumberCanvas _lineNumberCanvas;
		private readonly TextCanvas _textCanvas;
		private readonly DispatcherTimer _timer;
		private readonly ScrollBar _verticalScrollBar;

		private int _maxLineWidth;
		private int _pendingModificationsCount;

		#region Scrolling

		private ScrollEvent _lastScroll;

		private struct ScrollEvent
		{
			public double NewValue;
			public double OldValue;
		}

		#endregion

		static LogEntryListView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (LogEntryListView),
			                                         new FrameworkPropertyMetadata(typeof (LogEntryListView)));
		}

		public LogEntryListView()
		{
			_verticalScrollBar = new ScrollBar
				{
					Name = "PART_VerticalScrollBar",
				};
			_verticalScrollBar.ValueChanged += VerticalScrollBarOnValueChanged;
			_verticalScrollBar.Scroll += VerticalScrollBarOnScroll;
			_verticalScrollBar.SetValue(RowProperty, 0);
			_verticalScrollBar.SetValue(ColumnProperty, 2);

			_horizontalScrollBar = new ScrollBar
				{
					Name = "PART_HorizontalScrollBar",
					Orientation = Orientation.Horizontal,
				};
			_horizontalScrollBar.SetValue(RowProperty, 1);
			_horizontalScrollBar.SetValue(ColumnProperty, 0);
			_horizontalScrollBar.SetValue(ColumnSpanProperty, 2);

			ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto)});
			ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
			ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto)});
			RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Star)});
			RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Auto)});

			_textCanvas = new TextCanvas(_horizontalScrollBar, _verticalScrollBar);
			_textCanvas.SetValue(RowProperty, 0);
			_textCanvas.SetValue(ColumnProperty, 1);
			_textCanvas.SetValue(RowProperty, 0);
			_textCanvas.SetValue(ColumnProperty, 1);
			_textCanvas.MouseWheelDown += TextCanvasOnMouseWheelDown;
			_textCanvas.MouseWheelUp += TextCanvasOnMouseWheelUp;
			_textCanvas.SizeChanged += TextCanvasOnSizeChanged;
			_textCanvas.VisibleLinesChanged += TextCanvasOnVisibleLinesChanged;
			_textCanvas.RequestBringIntoView += TextCanvasOnRequestBringIntoView;
			_textCanvas.VisibleSectionChanged += TextCanvasOnVisibleSectionChanged;
			_textCanvas.OnSelectionChanged += TextCanvasOnOnSelectionChanged;

			_lineNumberCanvas = new LineNumberCanvas();
			_lineNumberCanvas.SetValue(RowProperty, 0);
			_lineNumberCanvas.SetValue(ColumnProperty, 0);

			_cornerRectangle = new Rectangle
				{
					Width = 17,
					Height = 17,
					Fill = new SolidColorBrush(Color.FromRgb(0xce, 0xce, 0xce))
				};
			_cornerRectangle.SetValue(RowProperty, 1);
			_cornerRectangle.SetValue(ColumnProperty, 2);

			Children.Add(_lineNumberCanvas);
			Children.Add(_textCanvas);
			Children.Add(_verticalScrollBar);
			Children.Add(_horizontalScrollBar);
			Children.Add(_cornerRectangle);

			_timer = new DispatcherTimer(MaximumRefreshInterval, DispatcherPriority.Normal, OnTimer, Dispatcher);
			_timer.Start();
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

		public bool FollowTail
		{
			get { return (bool) GetValue(FollowTailProperty); }
			set { SetValue(FollowTailProperty, value); }
		}

		internal int PendingModificationsCount
		{
			get { return _pendingModificationsCount; }
		}

		internal ScrollBar VerticalScrollBar
		{
			get { return _verticalScrollBar; }
		}

		internal ScrollBar HorizontalScrollBar
		{
			get { return _horizontalScrollBar; }
		}

		public LevelToBrushConverter HoveredBackgroundBrushConverter
		{
			get { return (LevelToBrushConverter) GetValue(HoveredBackgroundBrushConverterProperty); }
			set { SetValue(HoveredBackgroundBrushConverterProperty, value); }
		}

		public LevelToBrushConverter BackgroundBrushConverter
		{
			get { return (LevelToBrushConverter) GetValue(BackgroundBrushConverterProperty); }
			set { SetValue(BackgroundBrushConverterProperty, value); }
		}

		public LevelToBrushConverter ForegroundBrushConverter
		{
			get { return (LevelToBrushConverter) GetValue(ForegroundBrushConverterProperty); }
			set { SetValue(ForegroundBrushConverterProperty, value); }
		}

		public ILogFile LogFile
		{
			get { return (ILogFile) GetValue(LogFileProperty); }
			set { SetValue(LogFileProperty, value); }
		}

		public List<TextLine> VisibleTextLines
		{
			get { return _textCanvas.VisibleTextLines; }
		}

		public IEnumerable<LogLineIndex> SelectedIndices
		{
			get { return _textCanvas.SelectedIndices; }
		}

		public TextCanvas PartTextCanvas
		{
			get { return _textCanvas; }
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			if (!section.InvalidateSection &&
			    !section.IsReset)
			{
				LogLine[] lines = logFile.GetSection(section);
				foreach (LogLine line in lines)
				{
					double width = TextHelper.EstimateWidthUpperLimit(line.Message);
					var upperWidth = (int) Math.Ceiling(width);

					// Setting an integer is an atomic operation and thus no
					// special synchronization is required.
					_maxLineWidth = Math.Max(_maxLineWidth, upperWidth);
				}
			}

			Interlocked.Increment(ref _pendingModificationsCount);
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
			_lineNumberCanvas.Visibility = showLineNumbers
				                               ? Visibility.Visible
				                               : Visibility.Collapsed;
		}

		private void OnCurrentLineChanged(LogLineIndex index)
		{
			LogFileSection current = _textCanvas.CurrentlyVisibleSection;
			if (index != LogLineIndex.Invalid)
			{
				_textCanvas.CurrentLine = (int)index;
				//< We don't want to call BringIntoView() everytime because that one scrolls to fully
				// bring a line into view. This would mean that if the currently visible section changed
				// so that the top line is only partially visible, then this method would always bring
				// it fully visible. Removing the following filter would completely remove per pixel scrolling...
				if (current.Index != index)
				{
					_verticalScrollBar.Value = (int)index * TextHelper.LineHeight;
				}
			}
		}

		private void TextCanvasOnVisibleSectionChanged(LogFileSection section)
		{
			CurrentLine = section.Index;
		}

		private void TextCanvasOnOnSelectionChanged(HashSet<LogLineIndex> selectedIndices)
		{
			var fn = SelectionChanged;
			if (fn != null)
				fn(selectedIndices);
		}

		public event Action<IEnumerable<LogLineIndex>> SelectionChanged;

		private void TextCanvasOnRequestBringIntoView(LogLineIndex logLineIndex)
		{
			BringIntoView(logLineIndex);
		}

		private void BringIntoView(LogLineIndex logLineIndex)
		{
			double height = _textCanvas.ActualHeight;
			double offset = _textCanvas.YOffset;
			int start = _textCanvas.CurrentLine;
			int diff = logLineIndex - start;
			double min = ((diff)*TextHelper.LineHeight + offset);
			double max = ((diff + 1)*TextHelper.LineHeight + offset);
			if (min < 0)
			{
				_verticalScrollBar.Value += min;
			}
			else if (max > height)
			{
				_verticalScrollBar.Value += (max - height);
			}
		}

		private void TextCanvasOnVisibleLinesChanged()
		{
			_lineNumberCanvas.UpdateLineNumbers(LogFile,
			                                    _textCanvas.CurrentlyVisibleSection,
			                                    _textCanvas.YOffset);
		}

		private void TextCanvasOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			UpdateScrollViewerRegions();
		}

		private void TextCanvasOnMouseWheelUp()
		{
			double delta = _verticalScrollBar.Value - _verticalScrollBar.Minimum;
			double toScroll = Math.Min(delta, TextHelper.LineHeight);

			if (toScroll > 0)
			{
				FollowTail = false;
				_verticalScrollBar.Value -= toScroll;
			}
		}

		private void TextCanvasOnMouseWheelDown()
		{
			double delta = _verticalScrollBar.Maximum - _verticalScrollBar.Value;
			double toScroll = Math.Min(delta, TextHelper.LineHeight);
			_verticalScrollBar.Value += toScroll;
		}

		private static void OnFollowTailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LogEntryListView) d).OnFollowTailChanged((bool) e.NewValue);
		}

		public event Action<bool> FollowTailChanged;

		private void OnFollowTailChanged(bool followTail)
		{
			if (followTail)
			{
				ScrollToBottom();
			}

			Action<bool> fn = FollowTailChanged;
			if (fn != null)
				fn(followTail);
		}

		private void ScrollToBottom()
		{
			_verticalScrollBar.Value = _verticalScrollBar.Maximum;
		}

		internal void OnTimer(object sender, EventArgs args)
		{
			if (Interlocked.Exchange(ref _pendingModificationsCount, 0) > 0)
			{
				try
				{
					_textCanvas.DetermineVerticalOffset();
					_textCanvas.CurrentlyVisibleSection = CalculateVisibleSection();

					UpdateScrollViewerRegions();
					ScrollToBottomIfRequired();

					_textCanvas.UpdateVisibleLines();
					_textCanvas.UpdateMouseOver();
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception while updating: {0}", e);

					// Let's pray that this was just a hickup and try next time...
					Interlocked.Increment(ref _pendingModificationsCount);
				}
			}
		}

		private void ScrollToBottomIfRequired()
		{
			if (FollowTail)
			{
				ScrollToBottom();
			}
		}

		/// <summary>
		///     The section of the log file that is currently visible.
		/// </summary>
		[Pure]
		public LogFileSection CalculateVisibleSection()
		{
			return _textCanvas.CalculateVisibleSection();
		}

		private static void OnLogFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LogEntryListView) d).OnLogFileChanged(e.OldValue as ILogFile, e.NewValue as ILogFile);
		}

		private void OnLogFileChanged(ILogFile oldValue, ILogFile newValue)
		{
			if (oldValue != null)
			{
				oldValue.RemoveListener(this);
			}

			_maxLineWidth = 0;
			_textCanvas.LogFile = newValue;

			if (newValue != null)
			{
				newValue.AddListener(this, TimeSpan.FromMilliseconds(100), 10000);

				UpdateScrollViewerRegions();
				_textCanvas.DetermineVerticalOffset();
				_textCanvas.UpdateVisibleSection();
				_textCanvas.UpdateVisibleLines();
			}

			MatchScrollbarValueToCurrentLine();
		}

		private void MatchScrollbarValueToCurrentLine()
		{
			var value = _textCanvas.CurrentLine*TextHelper.LineHeight;
			if (value >= 0 && value <= _verticalScrollBar.Maximum)
			{
				_verticalScrollBar.Value = value;
			}
			else
			{
				_verticalScrollBar.Value = 0;
			}

			_horizontalScrollBar.Value = 0;
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
				UpdateCornerRectangle();
			}
		}

		private void UpdateCornerRectangle()
		{
			_cornerRectangle.Visibility =
				_horizontalScrollBar.Visibility == Visibility.Visible &&
				_verticalScrollBar.Visibility == Visibility.Visible
					? Visibility.Visible
					: Visibility.Collapsed;
		}

		private void UpdateHorizontalScrollbar()
		{
			double totalWidth = _maxLineWidth;
			double usableWidth = _textCanvas.ActualWidth;
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
			int count = LogFile.Count;
			double totalHeight = count*TextHelper.LineHeight;
			double usableHeight = _textCanvas.ActualHeight;
			if (totalHeight > usableHeight)
			{
				_verticalScrollBar.Minimum = 0;
				_verticalScrollBar.Maximum = Math.Ceiling((totalHeight - usableHeight)/TextHelper.LineHeight)*TextHelper.LineHeight;
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
			{
				FollowTail = false;
			}
		}

		private void VerticalScrollBarOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
		{
			_lastScroll = new ScrollEvent
				{
					OldValue = args.OldValue,
					NewValue = args.NewValue,
				};
		}

		public void Select(LogLineIndex index)
		{
			_textCanvas.SetSelected(index, SelectMode.Replace);
		}

		public void Select(IEnumerable<LogLineIndex> indices)
		{
			_textCanvas.SetSelected(indices, SelectMode.Replace);
		}

		public void Select(params LogLineIndex[] indices)
		{
			_textCanvas.SetSelected(indices, SelectMode.Replace);
		}

		public void CopySelectedLinesToClipboard()
		{
			_textCanvas.OnCopyToClipboard();
		}
	}
}