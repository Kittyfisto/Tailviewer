using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Ui.Controls.LogView.DeltaTimes
{
	/// <summary>
	///     A "canvas" which draws the elapsed time to the previous log entry in the same vertical alignment as <see cref="TextCanvas" />
	///     draws the <see cref="LogLine.Message" />.
	/// </summary>
	public sealed class LogEntryDeltaTimeColumn
		: FrameworkElement
	{
		private readonly List<DeltaTimeEntry> _deltas;
		private readonly double _lineNumberWidth;
		private double _yOffset;

		public LogEntryDeltaTimeColumn()
		{
			_deltas = new List<DeltaTimeEntry>();
			ClipToBounds = true;
			Width = _lineNumberWidth = 50;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			drawingContext.DrawRectangle(Brushes.White, pen: null,
			                             rectangle: new Rect(x: 0, y: 0, width: ActualWidth, height: ActualHeight));

			var y = _yOffset;
			foreach (var number in _deltas)
			{
				number.Render(drawingContext, y, _lineNumberWidth);
				y += TextHelper.LineHeight;
			}
		}

		public void UpdateLines(ILogFile logFile, LogFileSection visibleSection, double yOffset)
		{
			if (Visibility != Visibility.Visible) //< We shouldn't waste CPU cycles when we're hidden from view...
				return;

			_yOffset = yOffset;

			_deltas.Clear();
			if (logFile != null)
			{
				var deltas = new TimeSpan?[visibleSection.Count];
				logFile.GetColumn(visibleSection, LogFileColumns.DeltaTime, deltas);
				foreach (var index in deltas)
					_deltas.Add(new DeltaTimeEntry(index));
			}

			InvalidateVisual();
		}
	}
}