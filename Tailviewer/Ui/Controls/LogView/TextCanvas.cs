using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Metrolib.Controls;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using log4net;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     Responsible for drawing the individual <see cref="LogLine" />s of the <see cref="ILogFile" />.
	/// </summary>
	public sealed class TextCanvas
		: FrameworkElement
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ScrollBar _horizontalScrollBar;
		private readonly HashSet<LogLineIndex> _hoveredIndices;
		private readonly HashSet<LogLineIndex> _selectedIndices;
		private readonly ScrollBar _verticalScrollBar;
		private readonly List<TextLine> _visibleTextLines;

		private int _currentLine;
		private LogFileSection _currentlyVisibleSection;
		private LogLineIndex _lastSelection;
		private ILogFile _logFile;
		private double _xOffset;
		private double _yOffset;
		private string _stringFilter;
		private ILogEntryFilter _filter;
		private bool _colorByLevel;

		public TextCanvas(ScrollBar horizontalScrollBar, ScrollBar verticalScrollBar)
		{
			_horizontalScrollBar = horizontalScrollBar;
			_horizontalScrollBar.ValueChanged += HorizontalScrollBarOnScroll;

			_verticalScrollBar = verticalScrollBar;
			_verticalScrollBar.ValueChanged += VerticalScrollBarOnValueChanged;

			_selectedIndices = new HashSet<LogLineIndex>();
			_hoveredIndices = new HashSet<LogLineIndex>();
			_visibleTextLines = new List<TextLine>();

			InputBindings.Add(new KeyBinding(new DelegateCommand(OnCopyToClipboard), Key.C, ModifierKeys.Control));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMoveDown), Key.Down, ModifierKeys.None));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMoveUp), Key.Up, ModifierKeys.None));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMovePageDown), Key.PageDown, ModifierKeys.None));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMovePageUp), Key.PageUp, ModifierKeys.None));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMoveStart), Key.Home, ModifierKeys.Control));
			InputBindings.Add(new KeyBinding(new DelegateCommand(OnMoveEnd), Key.End, ModifierKeys.Control));
			InputBindings.Add(new MouseBinding(new DelegateCommand(OnMouseWheelUp), MouseWheelGesture.WheelUp));
			InputBindings.Add(new MouseBinding(new DelegateCommand(OnMouseWheelDown), MouseWheelGesture.WheelDown));

			SizeChanged += OnSizeChanged;
			GotFocus += OnGotFocus;
			LostFocus += OnLostFocus;

			Focusable = true;
			ClipToBounds = true;
		}

		public LogFileSection CurrentlyVisibleSection
		{
			get { return _currentlyVisibleSection; }
			set
			{
				if (value == _currentlyVisibleSection)
					return;

				_currentlyVisibleSection = value;
				Action<LogFileSection> fn = VisibleSectionChanged;
				if (fn != null)
					fn(value);
			}
		}

		public ILogFile LogFile
		{
			get { return _logFile; }
			set
			{
				_logFile = value;
				_visibleTextLines.Clear();

				_currentLine = 0;
				_lastSelection = 0;
			}
		}

		public int CurrentLine
		{
			get { return _currentLine; }
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

		public double YOffset
		{
			get { return _yOffset; }
		}

		public string StringFilter
		{
			get { return _stringFilter; }
			set
			{
				_stringFilter = value;
				if (string.IsNullOrWhiteSpace(value))
				{
					Filter = null;
				}
				else
				{
					Filter = BusinessLogic.Filters.Filter.Create(value);
				}
			}
		}

		public ILogEntryFilter Filter
		{
			get { return _filter; }
			set
			{
				_filter = value;
				foreach (var line in _visibleTextLines)
				{
					line.Filter = value;
				}
			}
		}

		public bool ColorByLevel
		{
			get { return _colorByLevel; }
			set
			{
				if (value == _colorByLevel)
					return;

				_colorByLevel = value;
				UpdateVisibleLines();
			}
		}

		public event Action<LogFileSection> VisibleSectionChanged;

		public void UpdateVisibleSection()
		{
			_currentlyVisibleSection = CalculateVisibleSection();
		}

		private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			UpdateVisibleLines();
		}

		private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			UpdateVisibleLines();
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			var rect = new Rect(0, 0, ActualWidth, ActualHeight);
			drawingContext.DrawRectangle(Brushes.White, null, rect);

			double x = _xOffset;
			double y = _yOffset;
			foreach (TextLine textLine in _visibleTextLines)
			{
				textLine.Render(drawingContext, x, y, ActualWidth, ColorByLevel);
				y += TextHelper.LineHeight;
			}
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			OnSizeChanged();
		}

		internal void OnSizeChanged()
		{
			DetermineVerticalOffset();
			_currentlyVisibleSection = CalculateVisibleSection();
			UpdateVisibleLines();
		}

		public void DetermineVerticalOffset()
		{
			double value = _verticalScrollBar.Value;
			var lineBeginning = (int) (Math.Floor(value/TextHelper.LineHeight)*TextHelper.LineHeight);
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
				var line = new TextLine(data[i], _hoveredIndices, _selectedIndices, _colorByLevel)
					{
						Filter = _filter,
						IsFocused = IsFocused
					};
				_visibleTextLines.Add(line);
			}

			Action fn = VisibleLinesChanged;
			if (fn != null)
				fn();

			InvalidateVisual();
		}

		public event Action VisibleLinesChanged;

		private bool SetHovered(LogLineIndex index, SelectMode selectMode)
		{
			return Set(_hoveredIndices, index, selectMode);
		}

		public bool SetSelected(LogLineIndex index, SelectMode selectMode)
		{
			bool changed = Set(_selectedIndices, index, selectMode);
			_lastSelection = index;

			if (changed)
			{
				var fn = OnSelectionChanged;
				if (fn != null)
					fn(_selectedIndices);
			}

			return changed;
		}

		public void SetSelected(IEnumerable<LogLineIndex> indices, SelectMode selectMode)
		{
			if (selectMode == SelectMode.Replace)
			{
				_selectedIndices.Clear();
				if (indices != null)
				{
					foreach (LogLineIndex index in indices)
					{
						_selectedIndices.Add(index);
					}
				}

				var fn = OnSelectionChanged;
				if (fn != null)
					fn(_selectedIndices);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public event Action<HashSet<LogLineIndex>> OnSelectionChanged;

		private static bool Set(HashSet<LogLineIndex> indices, LogLineIndex index, SelectMode selectMode)
		{
			if (selectMode == SelectMode.Replace)
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

		/// <summary>
		///     The section of the log file that is currently visible.
		/// </summary>
		[Pure]
		public LogFileSection CalculateVisibleSection()
		{
			if (_logFile == null)
				return new LogFileSection(0, 0);

			double maxLinesInViewport = (ActualHeight - _yOffset)/TextHelper.LineHeight;
			var maxCount = (int) Math.Ceiling(maxLinesInViewport);
			int linesLeft = LogFile.Count - _currentLine;
			int count = Math.Min(linesLeft, maxCount);
			if (count < 0)
				return new LogFileSection();

			return new LogFileSection(_currentLine, count);
		}

		public void UpdateMouseOver()
		{
			Point relativePos = Mouse.GetPosition(this);
			if (InputHitTest(relativePos) == this)
				UpdateMouseOver(relativePos);
		}

		private void UpdateMouseOver(Point relativePos)
		{
			double y = relativePos.Y - _yOffset;
			var visibleLineIndex = (int) Math.Floor(y/TextHelper.LineHeight);
			if (visibleLineIndex >= 0 && visibleLineIndex < _visibleTextLines.Count)
			{
				var lineIndex = new LogLineIndex(_visibleTextLines[visibleLineIndex].LogLine.LineIndex);
				if (SetHovered(lineIndex, SelectMode.Replace))
					InvalidateVisual();
			}
		}

		private void VerticalScrollBarOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
		{
			double pos = args.NewValue;
			var currentLine = (int) Math.Floor(pos/TextHelper.LineHeight);

			DetermineVerticalOffset();
			_currentLine = currentLine;
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

		private void ChangeSelectionAndBringIntoView(LogLineIndex newIndex)
		{
			if (SetSelected(newIndex, SelectMode.Replace))
			{
				Action<LogLineIndex> fn = RequestBringIntoView;
				if (fn != null)
					fn(newIndex);

				InvalidateVisual();
			}
		}

		internal void OnMovePageUp()
		{
			try
			{
				if (_selectedIndices.Count > 0 && _lastSelection > 0)
				{
					LogLineIndex newIndex;
					int maxDelta = _currentlyVisibleSection.Count;
					if (maxDelta > _lastSelection)
						newIndex = 0;
					else
						newIndex = _lastSelection - maxDelta;

					ChangeSelectionAndBringIntoView(newIndex);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		internal void OnMovePageDown()
		{
			try
			{
				int count = _logFile.Count;
				if (_selectedIndices.Count > 0 && _lastSelection < count - 1)
				{
					LogLineIndex newIndex;
					int maxDelta = _currentlyVisibleSection.Count;
					if (maxDelta + _lastSelection >= count)
						newIndex = count - 1;
					else
						newIndex = _lastSelection + maxDelta;

					ChangeSelectionAndBringIntoView(newIndex);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		internal void OnMoveUp()
		{
			try
			{
				if (_selectedIndices.Count > 0 && _lastSelection > 0)
				{
					int newIndex = _lastSelection - 1;
					ChangeSelectionAndBringIntoView(newIndex);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		internal void OnMoveDown()
		{
			try
			{
				if (_selectedIndices.Count > 0 && _lastSelection < _logFile.Count - 1)
				{
					LogLineIndex newIndex = _lastSelection + 1;
					ChangeSelectionAndBringIntoView(newIndex);
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		internal void OnMoveStart()
		{
			try
			{
				var newIndex = new LogLineIndex(0);
				ChangeSelectionAndBringIntoView(newIndex);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		internal void OnMoveEnd()
		{
			try
			{
				ILogFile logFile = _logFile;
				if (logFile != null)
				{
					var count = logFile.Count;
					if (count > 0)
					{
						var newIndex = new LogLineIndex(count - 1);
						ChangeSelectionAndBringIntoView(newIndex);
					}
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		public new event Action<LogLineIndex> RequestBringIntoView;

		public void OnCopyToClipboard()
		{
			try
			{
				var builder = new StringBuilder();
				ILogFile logFile = _logFile;
				if (logFile != null)
				{
					var sortedIndices = new List<LogLineIndex>(_selectedIndices);
					sortedIndices.Sort();
					for (int i = 0; i < sortedIndices.Count; ++i)
					{
						LogLine line = logFile.GetLine((int) sortedIndices[i]);
						if (i < sortedIndices.Count - 1)
							builder.AppendLine(line.Message);
						else
							builder.Append(line.Message);
					}
				}
				string message = builder.ToString();
				Clipboard.SetText(message);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
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
			Action fn = MouseWheelDown;
			if (fn != null)
				fn();

			UpdateMouseOver();
		}

		private void OnMouseWheelUp()
		{
			Action fn = MouseWheelUp;
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
				LogLineIndex index = _hoveredIndices.First();

				SelectMode selectMode = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
					                        ? SelectMode.Add
					                        : SelectMode.Replace;
				if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				{
					if (SetSelected(_lastSelection, index, selectMode))
						InvalidateVisual();
				}
				else
				{
					if (SetSelected(index, selectMode))
						InvalidateVisual();
				}
			}

			Focus();
			base.OnMouseLeftButtonDown(e);
		}

		private bool SetSelected(LogLineIndex from, LogLineIndex to, SelectMode selectMode)
		{
			bool changed = false;
			if (selectMode == SelectMode.Replace)
			{
				if (_hoveredIndices.Count > 0)
					changed = true;

				_hoveredIndices.Clear();
			}

			LogLineIndex min = LogLineIndex.Min(from, to);
			LogLineIndex max = LogLineIndex.Max(from, to);
			int count = max - min;
			for (int i = 0; i <= count /* we want to select everything including 'to' */; ++i)
			{
				changed |= _selectedIndices.Add(min + i);
			}

			if (changed)
			{
				var fn = OnSelectionChanged;
				if (fn != null)
					fn(_selectedIndices);
			}

			return changed;
		}

		#endregion Mouse Events
	}
}