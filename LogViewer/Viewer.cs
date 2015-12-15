using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Tailviewer.BusinessLogic;

namespace LogViewer
{
	public class Viewer : Control
	{
		public static readonly DependencyProperty LogFileProperty =
			DependencyProperty.Register("LogFile", typeof (ILogFile), typeof (Viewer),
			                            new PropertyMetadata(null, OnLogFileChanged));

		const double LineHeight = 14;

		#region Cached data

		private readonly List<LineData> _lines;
		private int _currentLine;
		private LogFileSection _currentSection;

		#endregion

		private ScrollBar _horizontalScrollBar;
		private ScrollBar _verticalScrollBar;
		private readonly Typeface _typeface;

		static Viewer()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (Viewer), new FrameworkPropertyMetadata(typeof (Viewer)));
		}

		public Viewer()
		{
			_lines = new List<LineData>();
			_typeface = new Typeface("Segoe UI");

			SizeChanged += OnSizeChanged;
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			var previousVisibleSection = _currentSection;
			_currentSection = VisibleSection;
			UpdateScrollViewerRegions();
			UpdateVisibleLines(LogFile, previousVisibleSection, _currentSection);
		}

		public ILogFile LogFile
		{
			get { return (ILogFile) GetValue(LogFileProperty); }
			set { SetValue(LogFileProperty, value); }
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

		/// <summary>
		/// The section of the log file that is currently visible.
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

		private void UpdateVisibleLines()
		{
			UpdateVisibleLines(LogFile);
		}

		private void UpdateVisibleLines(ILogFile logFile)
		{
			_lines.Clear();
			if (logFile == null)
				return;

			var culture = CultureInfo.CurrentUICulture;
			var data = new LogEntry[_currentSection.Count];
			logFile.GetSection(_currentSection, data);
			for (int i = 0; i < _currentSection.Count; ++i)
			{
				_lines.Add(new LineData(i, new FormattedText(data[i].Message,
				                                          culture,
				                                          FlowDirection.LeftToRight,
				                                          _typeface,
				                                          LineHeight,
				                                          Brushes.Black
					                        )));
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
				double totalHeight = count*LineHeight;
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

		private int MaxNumVisibleLines
		{
			get
			{
				return (int)Math.Ceiling(ActualHeight / LineHeight);
			}
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
			var currentLine = (int)Math.Floor(pos/LineHeight);

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
			foreach (var data in _lines)
			{
				var pos = new Point(0, y);
				drawingContext.DrawText(data.Text, pos);
				y += LineHeight;
			}
		}

		private struct LineData
		{
			public readonly int Index;
			public readonly FormattedText Text;

			public LineData(int index, FormattedText text)
			{
				Index = index;
				Text = text;
			}
		}
	}
}