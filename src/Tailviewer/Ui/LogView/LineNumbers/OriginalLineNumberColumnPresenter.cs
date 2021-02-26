using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Properties;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView.LineNumbers
{
	/// <summary>
	///     A "canvas" which draws <see cref="LogLine.OriginalLineIndex"/> in the same vertical alignment as <see cref="TextCanvas" />
	///     draws the <see cref="LogLine.Message" />.
	/// </summary>
	public sealed class OriginalLineNumberColumnPresenter
		: AbstractLogColumnPresenter<int>
	{
		private double _lineNumberWidth;

		public OriginalLineNumberColumnPresenter(TextSettings textSettings)
			: base(GeneralColumns.OriginalLineNumber, textSettings)
		{}

		public IEnumerable<LineNumberFormatter> LineNumbers => Values.Cast<LineNumberFormatter>();

		protected override void UpdateWidth(ILogSource logSource, TextSettings textSettings)
		{
			int lineNumberCharacterCount;
			if (logSource != null)
			{
				var lineCount = logSource.GetProperty(TextProperties.LineCount);
				if (lineCount > 0)
				{
					lineNumberCharacterCount = (int) Math.Ceiling(Math.Log10(lineCount));
				}
				else
				{
					lineNumberCharacterCount = 0;
				}
			}
			else
			{
				lineNumberCharacterCount = 0;
			}

			// We always reserve space for at least 3 characters.
			_lineNumberWidth = textSettings.EstimateWidthUpperLimit(Math.Max(lineNumberCharacterCount, val2: 3));
			Width = _lineNumberWidth + textSettings.LineNumberSpacing;
		}

		protected override AbstractLogEntryValueFormatter CreateFormatter(int value)
		{
			return new LineNumberFormatter(value, TextSettings);
		}
	}
}