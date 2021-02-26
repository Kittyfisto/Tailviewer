using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView.Timestamps
{
	public sealed class TimestampFormatter
		: AbstractLogEntryValueFormatter
	{
		private readonly DateTime? _value;

		public TimestampFormatter(DateTime? value, TextSettings textSettings)
			: base(textSettings)
		{
			_value = value;
		}

		public static string ToString(DateTime? value, IFormatProvider provider)
		{
			return value?.ToString(provider);
		}

		#region Overrides of AbstractLogEntryValueFormatter

		public override string ToString(IFormatProvider provider)
		{
			return ToString(_value, provider);
		}

		protected override FormattedText CreateFormattedText(string text, CultureInfo culture, TextSettings textSettings)
		{
			return new FormattedText(text,
			                         culture,
			                         FlowDirection.LeftToRight,
			                         textSettings.Typeface,
			                         textSettings.FontSize,
			                         TextBrushes.LineNumberForegroundBrush,
			                         1.25);
		}

		#endregion
	}
}
