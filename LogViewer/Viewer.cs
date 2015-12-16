using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Tailviewer.BusinessLogic;

namespace LogViewer
{
	public class Viewer : Control
	{
		public static readonly DependencyProperty LogFileProperty =
			DependencyProperty.Register("LogFile", typeof (ILogFile), typeof (Viewer),
			                            new PropertyMetadata(null, OnLogFileChanged));

		#region Cached data

		private readonly List<TextLine> _lines;
		private int _currentLine;
		private LogFileSection _currentSection;
		private TextLine _hoveredLine;
		private TextLine _selectedLine;

		#endregion

		private ScrollBar _horizontalScrollBar;
		private ScrollBar _verticalScrollBar;

		static Viewer()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (Viewer), new FrameworkPropertyMetadata(typeof (Viewer)));
		}

		public Viewer()
		{
			_lines = new List<TextLine>();

			SizeChanged += OnSizeChanged;
		}

		public ILogFile LogFile
		{
			get { return (ILogFile) GetValue(LogFileProperty); }
			set { SetValue(LogFileProperty, value); }
		}

		/// <summary>
		///     The section of the log file that is currently visible.
		/// </summary>
		public LogFileSection VisibleSection
		{
			get
			{
				if (LogFile == null)
					return new LogFileSection(0, 0);

				int maxCount = MaxNumVisibleLines;
				int linesLeft = LogFile.Count - _currentLine;
				int count = Math.Min(linesLeft, maxCount);
				return new LogFileSection(_currentLine, count);
			}
		}

		private int MaxNumVisibleLines
		{
			get { return (int) Math.Ceiling(ActualHeight/TextLine.LineHeight); }
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			LogFileSection previousVisibleSection = _currentSection;
			_currentSection = VisibleSection;
			UpdateScrollViewerRegions();
			UpdateVisibleLines(LogFile, previousVisibleSection, _currentSection);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			Point relativePos = e.GetPosition(this);
			var lineIndex = (int) Math.Floor(relativePos.Y/TextLine.LineHeight);
			if (lineIndex >= 0 && lineIndex < _lines.Count)
			{
				TextLine hoveredLine = _lines[lineIndex];
				if (hoveredLine != _hoveredLine)
				{
					hoveredLine.IsHovered = true;
					if (_hoveredLine != null)
					{
						_hoveredLine.IsHovered = false;
					}
					_hoveredLine = hoveredLine;

					if (Mouse.LeftButton == MouseButtonState.Pressed)
						TrySelectLine(_hoveredLine);

					InvalidateVisual();
				}
			}
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (_hoveredLine != null)
			{
				_hoveredLine.IsHovered = false;
				_hoveredLine = null;
				InvalidateVisual();
			}
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (TrySelectLine(_hoveredLine))
			{
				e.Handled = true;
				InvalidateVisual();
			}

			base.OnMouseLeftButtonDown(e);
		}

		private bool TrySelectLine(TextLine line)
		{
			if (line != null)
			{
				if (_selectedLine != null)
					_selectedLine.IsSelected = false;

				_selectedLine = line;
				_selectedLine.IsSelected = true;

				return true;
			}

			return false;
		}

		private static void OnLogFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Viewer) d).OnLogFileChanged(e.OldValue as ILogFile, e.NewValue as ILogFile);
		}

		private void OnLogFileChanged(ILogFile oldValue, ILogFile newValue)
		{
			_lines.Clear();
			if (newValue != null)
			{
				int numRows = Math.Min(MaxNumVisibleLines, newValue.Count);
				_currentSection = new LogFileSection(0, numRows);

				UpdateScrollViewerRegions();
				UpdateVisibleLines(newValue);
			}
		}

		private void UpdateVisibleLines()
		{
			UpdateVisibleLines(LogFile);
		}

		private void UpdateVisibleLines(ILogFile logFile)
		{
			_lines.Clear();
			if (logFile == null)
				return;

			var data = new LogEntry[_currentSection.Count];
			logFile.GetSection(_currentSection, data);
			for (int i = 0; i < _currentSection.Count; ++i)
			{
				_lines.Add(new TextLine(data[i]));
			}

			InvalidateVisual();
		}

		private void UpdateScrollViewerRegions()
		{
			if (LogFile == null)
			{
			}
			else
			{
				int count = LogFile.Count;
				double totalHeight = count*TextLine.LineHeight;
				double maximum = Math.Max(totalHeight - ActualHeight, 0);
				_verticalScrollBar.Minimum = 0;
				_verticalScrollBar.Maximum = maximum;
				_verticalScrollBar.ViewportSize = ActualHeight;
			}
		}

		private void UpdateVisibleLines(ILogFile logFile, LogFileSection oldSection, LogFileSection newSection)
		{
			UpdateVisibleLines(logFile);
		}

		public override void OnApplyTemplate()
		{
			_horizontalScrollBar = (ScrollBar) GetTemplateChild("PART_HorizontalScrollBar");
			_horizontalScrollBar.Scroll += HorizontalScrollBarOnScroll;

			_verticalScrollBar = (ScrollBar) GetTemplateChild("PART_VerticalScrollBar");
			_verticalScrollBar.Scroll += VerticalScrollBarOnScroll;

			UpdateScrollViewerRegions();

			base.OnApplyTemplate();
		}

		private void VerticalScrollBarOnScroll(object sender, ScrollEventArgs args)
		{
			double pos = args.NewValue;
			var currentLine = (int) Math.Floor(pos/TextLine.LineHeight);

			_currentLine = currentLine;
			_currentSection = VisibleSection;
			UpdateVisibleLines();
		}

		private void HorizontalScrollBarOnScroll(object sender, ScrollEventArgs args)
		{
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			double y = 0;
			foreach (TextLine data in _lines)
			{
				data.Render(drawingContext, y, ActualWidth);
				y += TextLine.LineHeight;
			}
		}
	}
}