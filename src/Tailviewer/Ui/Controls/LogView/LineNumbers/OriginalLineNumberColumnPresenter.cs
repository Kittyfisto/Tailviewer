using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Ui.Controls.LogView.LineNumbers
{
	/// <summary>
	///     A "canvas" which draws <see cref="LogLine.OriginalLineIndex"/> in the same vertical alignment as <see cref="TextCanvas" />
	///     draws the <see cref="LogLine.Message" />.
	/// </summary>
	public sealed class OriginalLineNumberColumnPresenter
		: AbstractLogColumnPresenter<int>
	{
		private double _lineNumberWidth;

		public OriginalLineNumberColumnPresenter()
			: base(LogFileColumns.OriginalLineNumber)
		{}

		public IEnumerable<LineNumberPresenter> LineNumbers => Values.Cast<LineNumberPresenter>();

		protected override void UpdateWidth(ILogFile logFile)
		{
			int lineNumberCharacterCount;
			if (logFile != null)
				lineNumberCharacterCount = (int) Math.Ceiling(Math.Log10(logFile.OriginalCount));
			else
				lineNumberCharacterCount = 0;

			// We always reserve space for at least 3 characters.
			_lineNumberWidth = TextHelper.EstimateWidthUpperLimit(Math.Max(lineNumberCharacterCount, val2: 3));
			Width = _lineNumberWidth + TextHelper.LineNumberSpacing;
		}

		protected override AbstractLogEntryValuePresenter CreatePresenter(int value)
		{
			return new LineNumberPresenter(value);
		}
	}
}