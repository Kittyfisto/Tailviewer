using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView.Any
{
	public sealed class AnyFormatter<T>
		: AbstractLogEntryValueFormatter
	{
		private readonly T _value;

		public AnyFormatter(T value, TextSettings textSettings)
			: base(textSettings)
		{
			_value = value;
		}

		#region Overrides of AbstractLogEntryValueFormatter

		public override string ToString(IFormatProvider provider)
		{
			return _value?.ToString();
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
