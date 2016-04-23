using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.LogFiles;
using log4net;

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

		private FormattedText _text;
		private Brush _lastForegroundBrush;

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
					return TextHelper.SelectedForegroundBrush;
				}
				if (IsHovered)
				{
					return TextHelper.HoveredForegroundBrush;
				}

				return TextHelper.NormalForegroundBrush;
			}
		}

		private Brush BackgroundBrush
		{
			get
			{
				if (IsSelected)
				{
					return TextHelper.SelectedBackgroundBrush;
				}
				if (IsHovered)
				{
					return TextHelper.HoveredBackgroundBrush;
				}
				return TextHelper.NormalBackgroundBrush;
			}
		}

		public bool IsSelected
		{
			get { return _selectedIndices.Contains(_logLine.LineIndex); }
		}

		private void CreateTextIfNecessary()
		{
			Brush brush = ForegroundBrush;
			if (_text == null || _lastForegroundBrush != brush)
			{
				_text = new FormattedText(_logLine.Message,
										  CultureInfo.CurrentUICulture,
										  FlowDirection.LeftToRight,
										  TextHelper.Typeface,
										  TextHelper.FontSize,
										  brush);
				_lastForegroundBrush = brush;
			}
		}

		public void Render(DrawingContext drawingContext,
			double x,
			double y,
			double actualWidth)
		{
			Brush brush = BackgroundBrush;
			if (brush != null)
			{
				drawingContext.DrawRectangle(brush, null, new Rect(x, y,
				                                                   actualWidth - x,
																   TextHelper.LineHeight));
			}

			var topLeft = new Point(x, y);
			drawingContext.DrawText(Text, topLeft);
		}
	}
}