using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Metrolib;
using Metrolib.Controls;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Ui.Converters;
using log4net;

namespace Tailviewer.Ui.Controls
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

		internal static readonly TimeSpan MaximumRefreshInterval = TimeSpan.FromMilliseconds(33);

		private readonly DispatcherTimer _timer;
		private readonly ScrollBar _horizontalScrollBar;
		private readonly ScrollBar _verticalScrollBar;
		private readonly Canvas _canvas;
		private readonly Rectangle _cornerRectangle;

		private int _pendingModificationsCount;
		private int _maxLineWidth;


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
			_verticalScrollBar.SetValue(ColumnProperty, 1);

			_horizontalScrollBar = new ScrollBar
				{
					Name = "PART_HorizontalScrollBar",
					Orientation = Orientation.Horizontal,
				};
			_horizontalScrollBar.SetValue(RowProperty, 1);
			_horizontalScrollBar.SetValue(ColumnProperty, 0);

			ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)});
			ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
			RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Star)});
			RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});

			_canvas = new Canvas(_horizontalScrollBar, _verticalScrollBar);
			_canvas.SetValue(RowProperty, 0);
			_canvas.SetValue(ColumnProperty, 0);
			_canvas.MouseWheelDown += CanvasOnMouseWheelDown;
			_canvas.MouseWheelUp += CanvasOnMouseWheelUp;
			_canvas.SizeChanged += CanvasOnSizeChanged;

			_cornerRectangle = new Rectangle
				{
					Width = 17,
					Height = 17,
					Fill = new SolidColorBrush(Color.FromRgb(0xce, 0xce, 0xce))
				};
			_cornerRectangle.SetValue(RowProperty, 1);
			_cornerRectangle.SetValue(ColumnProperty, 1);

			Children.Add(_canvas);
			Children.Add(_verticalScrollBar);
			Children.Add(_horizontalScrollBar);
			Children.Add(_cornerRectangle);

			_timer = new DispatcherTimer(MaximumRefreshInterval, DispatcherPriority.Normal, OnTimer, Dispatcher);
			_timer.Start();

			ClipToBounds = true;
		}

		private void CanvasOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			UpdateScrollViewerRegions();
		}

		private void CanvasOnMouseWheelUp()
		{
			double delta = _verticalScrollBar.Value - _verticalScrollBar.Minimum;
			double toScroll = Math.Min(delta, TextLine.LineHeight);

			if (toScroll > 0)
			{
				FollowTail = false;
				_verticalScrollBar.Value -= toScroll;
			}
		}

		private void CanvasOnMouseWheelDown()
		{
			double delta = _verticalScrollBar.Maximum - _verticalScrollBar.Value;
			double toScroll = Math.Min(delta, TextLine.LineHeight);
			_verticalScrollBar.Value += toScroll;
		}

		sealed class Canvas
			: FrameworkElement
		{
			private readonly ScrollBar _horizontalScrollBar;
			private readonly ScrollBar _verticalScrollBar;
			private readonly List<TextLine> _visibleTextLines;
			private readonly HashSet<LogLineIndex> _selectedIndices;
			private readonly HashSet<LogLineIndex> _hoveredIndices;

			private int _currentLine;
			private LogFileSection _currentlyVisibleSection;
			private double _yOffset;
			private double _xOffset;

			public LogFileSection CurrentlyVisibleSection
			{
				set { _currentlyVisibleSection = value; }
			}

			private ILogFile _logFile;

			public ILogFile LogFile
			{
				get { return _logFile; }
				set
				{
					_logFile = value;
					_visibleTextLines.Clear();

					_currentLine = 0;
				}
			}

			public void UpdateVisibleSection()
			{
				_currentlyVisibleSection = CalculateVisibleSection();
			}

			public Canvas(ScrollBar horizontalScrollBar, ScrollBar verticalScrollBar)
			{
				_horizontalScrollBar = horizontalScrollBar;
				_horizontalScrollBar.ValueChanged += HorizontalScrollBarOnScroll;

				_verticalScrollBar = verticalScrollBar;
				_verticalScrollBar.ValueChanged += VerticalScrollBarOnValueChanged;

				_selectedIndices = new HashSet<LogLineIndex>();
				_hoveredIndices = new HashSet<LogLineIndex>();
				_visibleTextLines = new List<TextLine>();

				InputBindings.Add(new MouseBinding(new DelegateCommand(OnMouseWheelUp), MouseWheelGesture.WheelUp));
				InputBindings.Add(new MouseBinding(new DelegateCommand(OnMouseWheelDown), MouseWheelGesture.WheelDown));

				SizeChanged += OnSizeChanged;
			}

			private int CurrentLine
			{
				set { _currentLine = value; }
			}

			public List<TextLine> VisibleTextLines
			{
				get { return _visibleTextLines; }
			}

			public IEnumerable<LogLineIndex> SelectedIndices
			{
				get { return _selectedIndices; }
			}

			protected override void OnRender(DrawingContext drawingContext)
			{
				base.OnRender(drawingContext);

				var rect = new Rect(0, 0, ActualWidth, ActualHeight);
				drawingContext.DrawRectangle(Brushes.White, null, rect);

				double y = _yOffset;
				foreach (TextLine textLine in _visibleTextLines)
				{
					textLine.Render(drawingContext, _xOffset, y, ActualWidth);
					y += TextLine.LineHeight;
				}
			}

			private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
			{
				DetermineVerticalOffset();
				_currentlyVisibleSection = CalculateVisibleSection();
				UpdateVisibleLines();
			}

			public void DetermineVerticalOffset()
			{
				double value = _verticalScrollBar.Value;
				var lineBeginning = (int)(Math.Floor(value / TextLine.LineHeight) * TextLine.LineHeight);
				_yOffset = lineBeginning - value;
			}

			public void UpdateVisibleLines()
			{
				_visibleTextLines.Clear();
				if (_logFile == null)
					return;

				var data = new LogLine[_currentlyVisibleSection.Count];
				_logFile.GetSection(_currentlyVisibleSection, data);
				for (int i = 0; i < _currentlyVisibleSection.Count; ++i)
				{
					_visibleTextLines.Add(new TextLine(data[i], _hoveredIndices, _selectedIndices));
				}

				InvalidateVisual();
			}

			private bool SetHovered(LogLineIndex index, Mode mode)
			{
				return Set(_hoveredIndices, index, mode);
			}

			private bool SetSelected(LogLineIndex index, Mode mode)
			{
				return Set(_selectedIndices, index, mode);
			}

			/// <summary>
			///     The section of the log file that is currently visible.
			/// </summary>
			[Pure]
			public LogFileSection CalculateVisibleSection()
			{
				if (_logFile == null)
					return new LogFileSection(0, 0);

				double maxLinesInViewport = (ActualHeight + _yOffset) / TextLine.LineHeight;
				var maxCount = (int)Math.Ceiling(maxLinesInViewport);
				int linesLeft = LogFile.Count - _currentLine;
				int count = Math.Min(linesLeft, maxCount);
				return new LogFileSection(_currentLine, count);
			}

			#region Mouse Events

			protected override void OnMouseMove(MouseEventArgs e)
			{
				base.OnMouseMove(e);
				Point relativePos = e.GetPosition(this);
				UpdateMouseOver(relativePos);
			}

			public event Action MouseWheelDown;
			public event Action MouseWheelUp;

			private void OnMouseWheelDown()
			{
				var fn = MouseWheelDown;
				if (fn != null)
					fn();

				UpdateMouseOver();
			}

			private void OnMouseWheelUp()
			{
				var fn = MouseWheelUp;
				if (fn != null)
					fn();

				UpdateMouseOver();
			}

			protected override void OnMouseLeave(MouseEventArgs e)
			{
				base.OnMouseLeave(e);

				if (_hoveredIndices.Count > 0)
				{
					_hoveredIndices.Clear();
					InvalidateVisual();
				}
			}

			protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
			{
				if (_hoveredIndices.Count > 0)
				{
					var index = _hoveredIndices.First();

					var mode = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
									   ? Mode.Add
									   : Mode.Replace;
					if (SetSelected(index, mode))
						InvalidateVisual();
				}

				base.OnMouseLeftButtonDown(e);
			}

			#endregion Mouse Events

			private void UpdateMouseOver()
			{
				var relativePos = Mouse.GetPosition(this);
				UpdateMouseOver(relativePos);
			}

			private void UpdateMouseOver(Point relativePos)
			{
				var y = relativePos.Y - _yOffset;
				var visibleLineIndex = (int)Math.Floor(y / TextLine.LineHeight);
				if (visibleLineIndex >= 0 && visibleLineIndex < _visibleTextLines.Count)
				{
					var lineIndex = new LogLineIndex(_visibleTextLines[visibleLineIndex].LogLine.LineIndex);
					if (SetHovered(lineIndex, Mode.Replace))
						InvalidateVisual();

					if (Mouse.LeftButton == MouseButtonState.Pressed)
					{
						var mode = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
										   ? Mode.Add
										   : Mode.Replace;
						if (SetSelected(lineIndex, mode))
							InvalidateVisual();
					}
				}
			}

			private void VerticalScrollBarOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
			{
				double pos = args.NewValue;
				var currentLine = (int)Math.Floor(pos / TextLine.LineHeight);

				DetermineVerticalOffset();
				CurrentLine = currentLine;
				CurrentlyVisibleSection = CalculateVisibleSection();
				UpdateVisibleLines();
			}

			private void HorizontalScrollBarOnScroll(object sender, RoutedPropertyChangedEventArgs<double> args)
			{
				// A value 0 zero means that the leftmost character shall be visible.
				// A value of MaxValue means that the rightmost character shall be visible.
				// This we need to offset each line's position by -value
				_xOffset = -args.NewValue;

				InvalidateVisual();
			}
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
			get { return _canvas.VisibleTextLines; }
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			if (!section.InvalidateSection &&
				!section.IsReset)
			{
				var lines = logFile.GetSection(section);
				foreach (var line in lines)
				{
					double width = TextLine.EstimateWidthUpperLimit(line.Message);
					var upperWidth = (int) Math.Ceiling(width);

					// Setting an integer is an atomic operation and thus no
					// special synchronization is required.
					_maxLineWidth = Math.Max(_maxLineWidth, upperWidth);
				}
			}

			Interlocked.Increment(ref _pendingModificationsCount);
		}

		public IEnumerable<LogLineIndex> SelectedIndices { get { return _canvas.SelectedIndices; } }

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

			var fn = FollowTailChanged;
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
					// TODO: Optimize if performance drops below acceptable rates
					_canvas.DetermineVerticalOffset();
					_canvas.CurrentlyVisibleSection = CalculateVisibleSection();
					UpdateScrollViewerRegions();
					ScrollToBottomIfRequired();
					_canvas.UpdateVisibleLines();
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
			return _canvas.CalculateVisibleSection();
		}

		enum Mode
		{
			Replace,
			Add
		}

		private static bool Set(HashSet<LogLineIndex> indices, LogLineIndex index, Mode mode)
		{
			if (mode == Mode.Replace)
			{
				if (indices.Count != 1 ||
					!indices.Contains(index))
				{
					indices.Clear();
					indices.Add(index);
					return true;
				}
			}
			else if (indices.Add(index))
			{
				return true;
			}

			return false;
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
			_canvas.LogFile = newValue;

			if (newValue != null)
			{
				newValue.AddListener(this, TimeSpan.FromMilliseconds(100), 10000);

				UpdateScrollViewerRegions();
				_canvas.DetermineVerticalOffset();
				_canvas.UpdateVisibleSection();
				_canvas.UpdateVisibleLines();
			}
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
			double usableWidth = _canvas.ActualWidth;
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
			double totalHeight = count*TextLine.LineHeight;
			double usableHeight = _canvas.ActualHeight;
			if (totalHeight > usableHeight)
			{
				_verticalScrollBar.Minimum = 0;
				_verticalScrollBar.Maximum = totalHeight - usableHeight;
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
	}
}