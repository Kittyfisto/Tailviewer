using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core;

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

						case LevelFlags.Debug:
							if (IsSelected)
								if (IsFocused)
									return TextHelper.SelectedForegroundBrush;

							return TextHelper.DebugForegroundBrush;

						case LevelFlags.Trace:
							if (IsSelected)
								if (IsFocused)
									return TextHelper.SelectedForegroundBrush;

							return TextHelper.TraceForegroundBrush;
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

		public IReadOnlyList<TextSegment> Segments
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
								AddSegmentsFrom(substring, regularForegroundBrush, isRegular: true);
							}

							substring = message.Substring(match.Index, match.Count);
							AddSegmentsFrom(substring, highlightedBrush, isRegular: false);
							lastIndex = match.Index + match.Count;
						}

						if (lastIndex <= message.Length - 1)
						{
							substring = message.Substring(lastIndex);
							AddSegmentsFrom(substring, regularForegroundBrush, isRegular: true);
						}
					}
					catch (Exception)
					{
						_segments.Clear();
						AddSegmentsFrom(message, regularForegroundBrush, isRegular: true);
					}
				}
				else
				{
					AddSegmentsFrom(message, regularForegroundBrush, isRegular: true);
				}
				_lastForegroundBrush = regularForegroundBrush;
			}
		}

		private void AddSegmentsFrom(string message, Brush brush, bool isRegular)
		{
			const int maxCharactersPerSegment = 512;
			int segmentCount = (int) Math.Ceiling(1.0 * message.Length / maxCharactersPerSegment);
			for (int i = 0; i < segmentCount; ++i)
			{
				var start = i * maxCharactersPerSegment;
				var remaining = message.Length - start;
				var length = Math.Min(remaining, maxCharactersPerSegment);
				var segment = message.Substring(i * maxCharactersPerSegment, length);
				_segments.Add(new TextSegment(segment, brush, isRegular));
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
				TextSegment segment = _segments[i];
				if (IsVisible(actualWidth, x, segment.Width))
				{
					Brush brush;
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
				}
				x += segment.Width;
			}

			if (x < actualWidth && regularBackgroundBrush != null)
			{
				var rect = new Rect(x, y,
				                    actualWidth - x,
				                    TextHelper.LineHeight);
				drawingContext.DrawRectangle(regularBackgroundBrush, null, rect);
			}
		}

		[Pure]
		public static bool IsVisible(double actualWidth, double x, double segmentWidth)
		{
			const int visibleXMin = 0;
			var visibleXMax = actualWidth;

			var xMin = x;
			var xMax = xMin + segmentWidth;

			var isVisible = !(xMax < visibleXMin || xMin > visibleXMax);
			return isVisible;
		}
	}
}