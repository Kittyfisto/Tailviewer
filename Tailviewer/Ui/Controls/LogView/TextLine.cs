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
		private readonly LogLine _logLine;
		private readonly HashSet<LogLineIndex> _hoveredIndices;
		private readonly HashSet<LogLineIndex> _selectedIndices;

		private Brush _lastForegroundBrush;
		private readonly List<TextSegment> _segments;
		private ILogEntryFilter _filter;

		public TextLine(LogLine logLine,
			HashSet<LogLineIndex> hoveredIndices,
			HashSet<LogLineIndex> selectedIndices)
		{
			if (logLine == null) throw new ArgumentNullException("logLine");
			if (hoveredIndices == null) throw new ArgumentNullException("hoveredIndices");
			if (selectedIndices == null) throw new ArgumentNullException("selectedIndices");

			_logLine = logLine;
			_hoveredIndices = hoveredIndices;
			_selectedIndices = selectedIndices;
			_segments = new List<TextSegment>();
		}

		public LogLine LogLine
		{
			get { return _logLine; }
		}

		public bool IsHovered
		{
			get { return _hoveredIndices.Contains(_logLine.LineIndex); }
		}

		public Brush ForegroundBrush
		{
			get
			{
				switch (_logLine.Level)
				{
					case LevelFlags.Fatal:
					case LevelFlags.Error:
						return TextHelper.ErrorForegroundBrush;

					default:
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
		}

		public Brush BackgroundBrush
		{
			get
			{
				if (IsSelected)
				{
					return TextHelper.SelectedBackgroundBrush;
				}
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
			var regularForegroundBrush = ForegroundBrush;
			if (_segments.Count == 0 || _lastForegroundBrush != regularForegroundBrush)
			{
				_segments.Clear();

				var message = _logLine.Message;
				var highlightedBrush = TextHelper.HighlightedForegroundBrush;
				var filter = _filter;
				if (filter != null)
				{
					string substring;
					int lastIndex = 0;
					var matches = filter.Match(_logLine);
					foreach (var match in matches)
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
			double actualWidth)
		{
			CreateTextIfNecessary();

			Brush regularBackgroundBrush = BackgroundBrush;
			Brush highlightedBackgroundBrush = TextHelper.HighlightedBackgroundBrush;

			double x = xOffset;
			for (int i = 0; i < _segments.Count; ++i)
			{
				var segment = _segments[i];
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