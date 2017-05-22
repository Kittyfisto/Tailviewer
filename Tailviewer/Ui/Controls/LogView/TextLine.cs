using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;

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
		private ISearchResults _searchResults;
		private Brush _lastForegroundBrush;
		private bool _colorByLevel;
		private bool _isFocused;

		public TextLine(LogLine logLine,
		                HashSet<LogLineIndex> hoveredIndices,
		                HashSet<LogLineIndex> selectedIndices,
		                bool colorByLevel)
		{
			if (logLine == null) throw new ArgumentNullException(nameof(logLine));
			if (hoveredIndices == null) throw new ArgumentNullException(nameof(hoveredIndices));
			if (selectedIndices == null) throw new ArgumentNullException(nameof(selectedIndices));

			_logLine = logLine;
			_hoveredIndices = hoveredIndices;
			_selectedIndices = selectedIndices;
			_segments = new List<TextSegment>();
			_colorByLevel = colorByLevel;
			_isFocused = true;
		}

		public bool IsFocused
		{
			get { return _isFocused; }
			set
			{
				if (value == _isFocused)
					return;

				_isFocused = value;
				_segments.Clear();
			}
		}

		public LogLine LogLine => _logLine;

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

		public bool IsHovered => _hoveredIndices.Contains(_logLine.LineIndex);

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
					if (IsFocused)
						return TextHelper.SelectedForegroundBrush;

					return TextHelper.NormalForegroundBrush;
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
					if (IsFocused)
						return TextHelper.SelectedBackgroundBrush;

					return TextHelper.SelectedUnfocusedBackgroundBrush;
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
							return TextHelper.NormalBackgroundBrush.GetBrushFor(_logLine.LogEntryIndex);
					}
				}

				if (IsHovered)
				{
					return TextHelper.NormalHighlightBackgroundBrush;
				}

				return TextHelper.NormalBackgroundBrush.GetBrushFor(_logLine.LogEntryIndex);
			}
		}

		public bool IsSelected => _selectedIndices.Contains(_logLine.LineIndex);

		public IEnumerable<TextSegment> Segments
		{
			get
			{
				CreateTextIfNecessary();
				return _segments;
			}
		}

		public ISearchResults SearchResults
		{
			set
			{
				_searchResults = value;
				_segments.Clear();
			}
		}

		private void CreateTextIfNecessary()
		{
			Brush regularForegroundBrush = ForegroundBrush;
			if (_segments.Count == 0 || _lastForegroundBrush != regularForegroundBrush)
			{
				_segments.Clear();

				string message = _logLine.Message ?? string.Empty;
				Brush highlightedBrush = TextHelper.HighlightedForegroundBrush;
				var searchResults = _searchResults;
				if (searchResults != null)
				{
					try
					{
						string substring;
						int lastIndex = 0;
						foreach (LogLineMatch match in searchResults.MatchesByLine[_logLine.LineIndex])
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

						if (lastIndex <= message.Length - 1)
						{
							substring = message.Substring(lastIndex);
							_segments.Add(new TextSegment(substring, regularForegroundBrush, isRegular: true));
						}
					}
					catch (Exception)
					{
						_segments.Add(new TextSegment(message, regularForegroundBrush, isRegular: true));
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

			double x = xOffset;
			for (int i = 0; i < _segments.Count; ++i)
			{
				Brush brush;
				TextSegment segment = _segments[i];
				if (segment.IsRegular)
				{
					brush = regularBackgroundBrush;
				}
				else
				{
					brush = IsSelected
						? TextHelper.HighlightedSelectedBackgroundBrush
						: TextHelper.HighlightedBackgroundBrush;
				}

				if (brush != null)
				{
					var rect = new Rect(x, y,
					                    segment.Width,
					                    TextHelper.LineHeight);
					drawingContext.DrawRectangle(brush, null, rect);
				}

				var topLeft = new Point(x, y);
				drawingContext.DrawText(segment.FormattedText, topLeft);

				x += segment.Width;

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
			}
		}
	}
}