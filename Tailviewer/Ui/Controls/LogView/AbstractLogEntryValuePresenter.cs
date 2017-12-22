using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls.LogView
{
	/// <summary>
	///     Responsible for displaying a single value of a log entry.
	/// </summary>
	public abstract class AbstractLogEntryValuePresenter
	{
		private FormattedText _formattedText;
		private double _width;

		protected AbstractLogEntryValuePresenter()
		{
			_formattedText = null;
		}

		public override string ToString()
		{
			return ToString(CultureInfo.InvariantCulture);
		}

		public abstract string ToString(IFormatProvider provider);

		private FormattedText FormattedText
		{
			get
			{
				if (_formattedText == null)
				{
					var culture = CultureInfo.CurrentUICulture;
					var text = ToString(culture);
					_formattedText = CreateFormattedText(text, culture);

					// We want to place the line numbers right aligned.
					// Although _text.Width is an option, it is INCREDIBLY slow.
					// It works by creating a polygon that represents the text and
					// then calculating the MBR of said polygon, which is fubar.
					_width = TextHelper.EstimateWidthUpperLimit(text);
				}
				return _formattedText;
			}
		}

		protected abstract FormattedText CreateFormattedText(string text, CultureInfo culture);

		public void Render(DrawingContext drawingContext, double yOffset, double lineNumberWidth)
		{
			var text = FormattedText;
			var x = lineNumberWidth - _width;
			drawingContext.DrawText(text, new Point(x, yOffset));
		}
	}
}