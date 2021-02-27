using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	///     Responsible for formatting a single value of a log entry.
	/// </summary>
	public abstract class AbstractLogEntryValueFormatter
		: ILogEntryValueFormatter
	{
		private readonly TextSettings _textSettings;
		private FormattedText _formattedText;
		private double _width;

		protected AbstractLogEntryValueFormatter(TextSettings textSettings)
		{
			_textSettings = textSettings;
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
					_formattedText = CreateFormattedText(text, culture, _textSettings);

					// We want to place the line numbers right aligned.
					// Although _text.Width is an option, it is INCREDIBLY slow.
					// It works by creating a polygon that represents the text and
					// then calculating the MBR of said polygon, which is fubar.
					_width = _textSettings.EstimateWidthUpperLimit(text);
				}
				return _formattedText;
			}
		}

		protected abstract FormattedText CreateFormattedText(string text, CultureInfo culture, TextSettings textSettings);

		public void Render(DrawingContext drawingContext, double yOffset, double lineNumberWidth)
		{
			var text = FormattedText;
			var x = lineNumberWidth - _width;
			drawingContext.DrawText(text, new Point(x, yOffset));
		}
	}
}