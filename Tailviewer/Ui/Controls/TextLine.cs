using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Controls
{
	/// <summary>
	///     Is responsible for representing the contents of a <see cref="LogLine" /> as a <see cref="FormattedText" />,
	///     ready to be rendered.
	/// </summary>
	public sealed class TextLine
	{
		public const double FontSize = 12;
		public const double LineSpacing = 4;
		public const double LineHeight = FontSize + LineSpacing;
		private static readonly Brush NormalForegroundBrush;
		private static readonly Brush NormalBackgroundBrush;
		private static readonly Brush HoveredForegroundBrush;
		private static readonly Brush HoveredBackgroundBrush;
		private static readonly Brush SelectedForegroundBrush;
		private static readonly Brush SelectedBackgroundBrush;
		private static readonly Typeface Typeface;
		private static readonly double GlyphWidth;

		private readonly LogLine _logLine;
		private readonly HashSet<LogLineIndex> _hoveredIndices;
		private readonly HashSet<LogLineIndex> _selectedIndices;

		private FormattedText _text;

		static TextLine()
		{
			NormalBackgroundBrush = null;
			NormalForegroundBrush = Brushes.Black;
			HoveredBackgroundBrush = new SolidColorBrush(Color.FromRgb(242, 242, 242));
			HoveredForegroundBrush = Brushes.Black;
			SelectedBackgroundBrush = new SolidColorBrush(Color.FromRgb(57, 152, 214));
			SelectedForegroundBrush = Brushes.White;
			var family = new FontFamily(new Uri("pack://application:,,,/Tailviewer;Component/Resources/"),
			                            "./#Anonymous Pro");
			Typeface = new Typeface(family, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

			GlyphTypeface test;
			Typeface.TryGetGlyphTypeface(out test);

			ushort glyphIndex = test.CharacterToGlyphMap[' '];
			GlyphWidth = test.AdvanceWidths[glyphIndex]*FontSize;
		}

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
		}

		public LogLine LogLine
		{
			get { return _logLine; }
		}

		public bool IsHovered
		{
			get { return _hoveredIndices.Contains(_logLine.LineIndex); }
		}

		public FormattedText Text
		{
			get
			{
				CreateTextIfNecessary();
				return _text;
			}
		}

		private Brush ForegroundBrush
		{
			get
			{
				if (IsSelected)
				{
					return SelectedForegroundBrush;
				}
				if (IsHovered)
				{
					return HoveredForegroundBrush;
				}

				return NormalForegroundBrush;
			}
		}

		private Brush BackgroundBrush
		{
			get
			{
				if (IsSelected)
				{
					return SelectedBackgroundBrush;
				}
				if (IsHovered)
				{
					return HoveredBackgroundBrush;
				}
				return NormalBackgroundBrush;
			}
		}

		public bool IsSelected
		{
			get { return _selectedIndices.Contains(_logLine.LineIndex); }
		}

		private void CreateTextIfNecessary()
		{
			if (_text != null)
				return;

			Brush brush = ForegroundBrush;
			_text = new FormattedText(_logLine.Message,
			                          CultureInfo.CurrentUICulture,
			                          FlowDirection.LeftToRight,
			                          Typeface,
			                          FontSize,
			                          brush);
		}

		public void Render(DrawingContext drawingContext, double x, double y, double actualWidth)
		{
			Brush brush = BackgroundBrush;
			if (brush != null)
			{
				drawingContext.DrawRectangle(brush, null, new Rect(0, y, actualWidth, LineHeight));
			}

			var topLeft = new Point(x, y);
			drawingContext.DrawText(Text, topLeft);
		}

		/// <summary>
		/// Estimates the width of the given text in DIP units.
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		[Pure]
		public static double EstimateWidthUpperLimit(string line)
		{
			// TODO: What about surrogate pairs?
			// TODO: What about tabs?
			return line.Length*GlyphWidth;
		}
	}
}