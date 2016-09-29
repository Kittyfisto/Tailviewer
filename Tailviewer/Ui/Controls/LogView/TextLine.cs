using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     Is responsible for representing the contents of a <see cref="LogLine" /> as a <see cref="FormattedText" />,
	///     ready to be rendered.
	/// </summary>
	public sealed class TextLine
	{
		private readonly HashSet<LogLineIndex> _hoveredIndices;
		private readonly LogLine _logLine;
		private readonly List<TextSegment> _segments;
		private readonly HashSet<LogLineIndex> _selectedIndices;
		private ILogEntryFilter _filter;
		private Brush _lastForegroundBrush;
		private bool _colorByLevel;

		public TextLine(LogLine logLine,
		                HashSet<LogLineIndex> hoveredIndices,
		                HashSet<LogLineIndex> selectedIndices,
		                bool colorByLevel)
		{
			if (logLine == null) throw new ArgumentNullException("logLine");
			if (hoveredIndices == null) throw new ArgumentNullException("hoveredIndices");
			if (selectedIndices == null) throw new ArgumentNullException("selectedIndices");

			_logLine = logLine;
			_hoveredIndices = hoveredIndices;
			_selectedIndices = selectedIndices;
			_segments = new List<TextSegment>();
			_colorByLevel = colorByLevel;
		}

		public LogLine LogLine
		{
			get { return _logLine; }
		}

		public bool ColorByLevel
		{
			get { return _colorByLevel; }
			set
			{
				if (value == _colorByLevel)
					return;

				_colorByLevel = value;
				_segments.Clear();
			}
		}

		public bool IsHovered
		{
			get { return _hoveredIndices.Contains(_logLine.LineIndex); }
		}

		public Brush ForegroundBrush
		{
			get
			{
				if (_colorByLevel)
				{
					switch (_logLine.Level)
					{
						case LevelFlags.Fatal:
						case LevelFlags.Error:
							return TextHelper.ErrorForegroundBrush;
					}
				}

				if (IsSelected)
				{
					return TextHelper.SelectedForegroundBrush;
				}
				if (IsHovered)
				{
					return TextHelper.HoveredForegroundBrush;
				}

				return TextHelper.NormalForegroundBrush;
			}
		}

		public Brush BackgroundBrush
		{
			get
			{
				if (IsSelected)
				{
					return TextHelper.SelectedBackgroundBrush;
				}

				if (_colorByLevel)
				{
					if (IsHovered)
					{
						switch (_logLine.Level)
						{
							case LevelFlags.Fatal:
							case LevelFlags.Error:
								return TextHelper.ErrorHighlightBackgroundBrush;

							case LevelFlags.Warning:
								return TextHelper.WarningHighlightBackgroundBrush;

							default:
								return TextHelper.NormalHighlightBackgroundBrush;
						}
					}

					switch (_logLine.Level)
					{
						case LevelFlags.Fatal:
						case LevelFlags.Error:
							return TextHelper.ErrorBackgroundBrush;

						case LevelFlags.Warning:
							return TextHelper.WarningBackgroundBrush;

						default:
							return TextHelper.NormalBackgroundBrush;
					}
				}

				if (IsHovered)
				{
					return TextHelper.NormalHighlightBackgroundBrush;
				}

				return TextHelper.NormalBackgroundBrush;
			}
		}

		public bool IsSelected
		{
			get { return _selectedIndices.Contains(_logLine.LineIndex); }
		}

		public ILogEntryFilter Filter
		{
			get { return _filter; }
			set
			{
				_filter = value;
				_segments.Clear();
			}
		}

		public IEnumerable<TextSegment> Segments
		{
			get
			{
				CreateTextIfNecessary();
				return _segments;
			}
		}

		private void CreateTextIfNecessary()
		{
			Brush regularForegroundBrush = ForegroundBrush;
			if (_segments.Count == 0 || _lastForegroundBrush != regularForegroundBrush)
			{
				_segments.Clear();

				string message = _logLine.Message;
				Brush highlightedBrush = TextHelper.HighlightedForegroundBrush;
				ILogEntryFilter filter = _filter;
				if (filter != null)
				{
					string substring;
					int lastIndex = 0;
					List<FilterMatch> matches = filter.Match(_logLine);
					foreach (FilterMatch match in matches)
					{
						if (match.Index > lastIndex)
						{
							substring = message.Substring(lastIndex, match.Index - lastIndex);
							_segments.Add(new TextSegment(substring, regularForegroundBrush, isRegular: true));
						}

						substring = message.Substring(match.Index, match.Count);
						_segments.Add(new TextSegment(substring, highlightedBrush, isRegular: false));
						lastIndex = match.Index + match.Count;
					}

					if (lastIndex < message.Length - 1)
					{
						substring = message.Substring(lastIndex);
						_segments.Add(new TextSegment(substring, regularForegroundBrush, isRegular: true));
					}
				}
				else
				{
					_segments.Add(new TextSegment(message, regularForegroundBrush, isRegular: true));
				}
				_lastForegroundBrush = regularForegroundBrush;
			}
		}

		public void Render(DrawingContext drawingContext,
		                   double xOffset,
		                   double y,
		                   double actualWidth,
		                   bool colorByLevel)
		{
			CreateTextIfNecessary();

			Brush regularBackgroundBrush = BackgroundBrush;
			Brush highlightedBackgroundBrush = TextHelper.HighlightedBackgroundBrush;

			double x = xOffset;
			for (int i = 0; i < _segments.Count; ++i)
			{
				TextSegment segment = _segments[i];
				Brush brush = segment.IsRegular ? regularBackgroundBrush : highlightedBackgroundBrush;
				if (brush != null)
				{
					var rect = new Rect(x, y,
					                    segment.Width,
					                    TextHelper.LineHeight);
					drawingContext.DrawRectangle(brush, null, rect);
				}

				if (i == _segments.Count - 1)
				{
					if (x < actualWidth && regularBackgroundBrush != null)
					{
						var rect = new Rect(x, y,
						                    actualWidth - x,
						                    TextHelper.LineHeight);
						drawingContext.DrawRectangle(regularBackgroundBrush, null, rect);
					}
				}

				var topLeft = new Point(x, y);
				drawingContext.DrawText(segment.FormattedText, topLeft);

				x += segment.Width;
			}
		}
	}
}