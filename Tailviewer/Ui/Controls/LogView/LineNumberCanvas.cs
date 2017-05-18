using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Controls.LogView
{
	public sealed class LineNumberCanvas
		: FrameworkElement
	{
		private readonly List<LineNumber> _lineNumbers;
		private double _yOffset;
		private double _lineNumberWidth;

		public LineNumberCanvas()
		{
			_lineNumbers = new List<LineNumber>();

			ClipToBounds = true;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));

			double y = _yOffset;
			foreach (var number in _lineNumbers)
			{
				number.Render(drawingContext, y, _lineNumberWidth);
				y += TextHelper.LineHeight;
			}
		}

		public IReadOnlyList<LineNumber> LineNumbers => _lineNumbers;

		public void UpdateLineNumbers(ILogFile logFile, LogFileSection visibleSection, double yOffset)
		{
			int lineNumberCharacterCount;
			if (logFile != null)
			{
				lineNumberCharacterCount = (int)Math.Ceiling(Math.Log10(logFile.Count));
			}
			else
			{
				lineNumberCharacterCount = 0;
			}

			// We always reserve space for at least 3 characters.
			_lineNumberWidth = TextHelper.EstimateWidthUpperLimit(Math.Max(lineNumberCharacterCount, 3));
			Width = _lineNumberWidth + TextHelper.LineNumberSpacing;

			_yOffset = yOffset;

			_lineNumbers.Clear();
			for (int i = 0; i < visibleSection.Count; ++i)
			{
				_lineNumbers.Add(new LineNumber(visibleSection.Index + i));
			}
			InvalidateVisual();
		}
	}
}