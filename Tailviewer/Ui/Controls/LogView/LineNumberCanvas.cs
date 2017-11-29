using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     A "canvas" which draws <see cref="LogLine.OriginalLineIndex"/> in the same vertical alignment as <see cref="TextCanvas" />
	///     draws the <see cref="LogLine.Message" />.
	/// </summary>
	public sealed class LineNumberCanvas
		: FrameworkElement
	{
		private readonly List<LineNumber> _lineNumbers;
		private double _lineNumberWidth;
		private double _yOffset;

		public LineNumberCanvas()
		{
			_lineNumbers = new List<LineNumber>();

			ClipToBounds = true;
		}

		public IReadOnlyList<LineNumber> LineNumbers => _lineNumbers;

		protected override void OnRender(DrawingContext drawingContext)
		{
			drawingContext.DrawRectangle(Brushes.White, pen: null,
				rectangle: new Rect(x: 0, y: 0, width: ActualWidth, height: ActualHeight));

			var y = _yOffset;
			foreach (var number in _lineNumbers)
			{
				number.Render(drawingContext, y, _lineNumberWidth);
				y += TextHelper.LineHeight;
			}
		}

		public void UpdateLineNumbers(ILogFile logFile, LogFileSection visibleSection, double yOffset)
		{
			int lineNumberCharacterCount;
			if (logFile != null)
				lineNumberCharacterCount = (int) Math.Ceiling(Math.Log10(logFile.OriginalCount));
			else
				lineNumberCharacterCount = 0;

			// We always reserve space for at least 3 characters.
			_lineNumberWidth = TextHelper.EstimateWidthUpperLimit(Math.Max(lineNumberCharacterCount, val2: 3));
			Width = _lineNumberWidth + TextHelper.LineNumberSpacing;

			_yOffset = yOffset;

			_lineNumbers.Clear();
			if (logFile != null)
			{
				var indices = new LogLineIndex[visibleSection.Count];
				logFile.GetOriginalIndicesFrom(visibleSection, indices);
				foreach (var index in indices)
					_lineNumbers.Add(new LineNumber(index));
			}

			InvalidateVisual();
		}
	}
}