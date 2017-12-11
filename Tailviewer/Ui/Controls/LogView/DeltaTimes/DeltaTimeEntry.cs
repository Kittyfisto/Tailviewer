using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls.LogView.DeltaTimes
{
	internal struct DeltaTimeEntry
	{
		private readonly TimeSpan? _delta;
		private FormattedText _formattedText;

		public DeltaTimeEntry(TimeSpan? delta)
		{
			_delta = delta;
			_formattedText = null;
		}

		public override string ToString()
		{
			return ToString(CultureInfo.InvariantCulture);
		}

		public string ToString(IFormatProvider provider)
		{
			if (_delta != null)
			{
				return string.Format(provider, "{0}ms", _delta.Value.TotalMilliseconds);
			}

			return string.Empty;
		}

		private FormattedText FormattedText
		{
			get
			{
				if (_formattedText == null)
				{
					var culture = CultureInfo.CurrentUICulture;
					var text = ToString(culture);
					_formattedText = new FormattedText(text,
					                                   culture,
					                                   FlowDirection.LeftToRight,
					                                   TextHelper.Typeface,
					                                   TextHelper.FontSize,
					                                   TextHelper.LineNumberForegroundBrush);
				}
				return _formattedText;
			}
		}

		public void Render(DrawingContext drawingContext, double yOffset, double lineNumberWidth)
		{
			var text = FormattedText;

			// We want to place the line numbers right aligned.
			// Although _text.Width is an option, it is INCREDIBLY slow.
			// It works by creating a polygon that represents the text and
			// then calculating the MBR of said polygon, which is fubar.
			var width = TextHelper.EstimateWidthUpperLimit(text.Text);
			var x = lineNumberWidth - width;

			drawingContext.DrawText(text, new Point(x, yOffset));
		}
	}
}