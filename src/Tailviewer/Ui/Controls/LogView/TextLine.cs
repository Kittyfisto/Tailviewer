using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     Is responsible for representing the contents of a <see cref="LogEntry" /> as a <see cref="FormattedText" />,
	///     ready to be rendered.
	/// </summary>
	public sealed class TextLine
	{
		private readonly HashSet<LogLineIndex> _hoveredIndices;
		private readonly IReadOnlyLogEntry _logEntry;
		private readonly List<TextSegment> _segments;
		private readonly TextSettings _textSettings;
		private readonly TextBrushes _textBrushes;
		private readonly HashSet<LogLineIndex> _selectedIndices;
		private ISearchResults _searchResults;
		private Brush _lastForegroundBrush;
		private bool _colorByLevel;
		private bool _isFocused;

		public TextLine(IReadOnlyLogEntry logEntry,
		                HashSet<LogLineIndex> hoveredIndices,
		                HashSet<LogLineIndex> selectedIndices,
		                bool colorByLevel)
			: this(logEntry, hoveredIndices, selectedIndices, colorByLevel, TextSettings.Default,
			       new TextBrushes(null))
		{}

		public TextLine(IReadOnlyLogEntry logEntry,
		                HashSet<LogLineIndex> hoveredIndices,
		                HashSet<LogLineIndex> selectedIndices,
		                bool colorByLevel,
		                TextSettings textSettings,
		                TextBrushes textBrushes)
		{
			if (logEntry == null) throw new ArgumentNullException(nameof(logEntry));
			if (hoveredIndices == null) throw new ArgumentNullException(nameof(hoveredIndices));
			if (selectedIndices == null) throw new ArgumentNullException(nameof(selectedIndices));
			if (textBrushes == null) throw new ArgumentNullException(nameof(textBrushes));

			_logEntry = logEntry;
			_hoveredIndices = hoveredIndices;
			_selectedIndices = selectedIndices;
			_segments = new List<TextSegment>();
			_colorByLevel = colorByLevel;
			_textSettings = textSettings;
			_textBrushes = textBrushes;
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

		public IReadOnlyLogEntry LogEntry => _logEntry;

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

		public bool IsHovered => _hoveredIndices.Contains(_logEntry.Index);

		public Brush ForegroundBrush
		{
			get
			{
				return _textBrushes.ForegroundBrush(IsSelected, IsFocused, ColorByLevel, _logEntry.LogLevel);
			}
		}

		public Brush BackgroundBrush
		{
			get
			{
				return _textBrushes.BackgroundBrush(IsSelected, IsFocused, ColorByLevel, _logEntry.LogLevel, (int) _logEntry.LogEntryIndex);
			}
		}

		public bool IsSelected => _selectedIndices.Contains(_logEntry.Index);

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

				Brush highlightedBrush = TextBrushes.HighlightedForegroundBrush;
				var searchResults = _searchResults;
				if (searchResults != null)
				{
					try
					{
						// The search results are based on the unformatted message (e.g. before we replace tabs by the appropriate
						// amount of spaces). Therefore we'll have to subdivide the message into chunks based on the search results
						// and then reformat the individual chunks.
						var unformattedMessage = _logEntry.RawContent;
						string substring;
						int lastIndex = 0;
						foreach (LogLineMatch match in searchResults.MatchesByLine[_logEntry.Index])
						{
							if (match.Index > lastIndex)
							{
								substring = unformattedMessage.Substring(lastIndex, match.Index - lastIndex);
								AddSegmentsFrom(FormatMessage(substring), regularForegroundBrush, isRegular: true);
							}

							substring = unformattedMessage.Substring(match.Index, match.Count);
							AddSegmentsFrom(FormatMessage(substring), highlightedBrush, isRegular: false);
							lastIndex = match.Index + match.Count;
						}

						if (lastIndex <= unformattedMessage.Length - 1)
						{
							substring = unformattedMessage.Substring(lastIndex);
							AddSegmentsFrom(FormatMessage(substring), regularForegroundBrush, isRegular: true);
						}
					}
					catch (Exception)
					{
						_segments.Clear();

						string message = FormatMessage(_logEntry.RawContent);
						AddSegmentsFrom(message, regularForegroundBrush, isRegular: true);
					}
				}
				else
				{
					string message = FormatMessage(_logEntry.RawContent);
					AddSegmentsFrom(message, regularForegroundBrush, isRegular: true);
				}
				_lastForegroundBrush = regularForegroundBrush;
			}
		}

		[Pure]
		private string FormatMessage(string logLineMessage)
		{
			var builder = new StringBuilder(logLineMessage ?? string.Empty);
			ReplaceTabsWithSpaces(builder, _textSettings.TabWidth);
			return builder.ToString();
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
				_segments.Add(new TextSegment(segment, brush, isRegular, _textSettings));
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
							? TextBrushes.HighlightedSelectedBackgroundBrush
							: TextBrushes.HighlightedBackgroundBrush;
					}

					if (brush != null)
					{
						var rect = new Rect(x, y,
						                    segment.Width,
						                    _textSettings.LineHeight);
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
				                    _textSettings.LineHeight);
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

		public static void ReplaceTabsWithSpaces(StringBuilder builder, int tabWidth)
		{
			for (int i = 0; i < builder.Length;)
			{
				if (builder[i] == '\t')
				{
					var already = i % tabWidth;
					var remaining = tabWidth - already;
					builder.Remove(i, 1);
					builder.Insert(i, " ", remaining);

					i += remaining;
				}
				else
				{
					++i;
				}
			}
		}
	}
}