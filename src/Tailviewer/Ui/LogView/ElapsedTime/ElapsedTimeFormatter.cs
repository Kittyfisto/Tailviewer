using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView.ElapsedTime
{
	public sealed class ElapsedTimeFormatter
		: AbstractLogEntryValueFormatter
	{
		public const int CharacterWidth = 11;
		private readonly TimeSpan? _value;

		public ElapsedTimeFormatter(TimeSpan? value, TextSettings textSettings)
			: base(textSettings)
		{
			_value = value;
		}

		[Pure]
		public static string ToString(TimeSpan? value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ToString(value.Value, provider);
			}

			return string.Empty;
		}

		public static string ToString(TimeSpan value, IFormatProvider provider)
		{
			if (value >= TimeSpan.FromDays(1))
			{
				return string.Format("{0:%d}d {0:hh\\:mm\\:ss\\.fff}", value);
			}

			return value.ToString(@"hh\:mm\:ss\.fff", provider);
		}

		public override string ToString(IFormatProvider provider)
		{
			return ToString(_value, provider);
		}

		protected override FormattedText CreateFormattedText(string text,
		                                                     CultureInfo culture,
		                                                     TextSettings textSettings)
		{
			return new FormattedText(text,
			                         culture,
			                         FlowDirection.LeftToRight,
			                         textSettings.Typeface,
			                         textSettings.FontSize,
			                         TextBrushes.LineNumberForegroundBrush,
			                         1.25);
		}
	}
}