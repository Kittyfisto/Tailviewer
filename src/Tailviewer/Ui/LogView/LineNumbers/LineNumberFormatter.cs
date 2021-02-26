using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Tailviewer.Settings;

namespace Tailviewer.Ui.LogView.LineNumbers
{
	public sealed class LineNumberFormatter
		: AbstractLogEntryValueFormatter
	{
		private readonly int _number;

		public LineNumberFormatter(int value)
			: this(value, TextSettings.Default)
		{}

		public LineNumberFormatter(int value, TextSettings textSettings)
			: base(textSettings)
		{
			_number = value;
		}

		public override string ToString(IFormatProvider provider)
		{
			return _number.ToString(provider);
		}

		protected override FormattedText CreateFormattedText(string text,
		                                                     CultureInfo culture,
		                                                     TextSettings textSettings)
		{
			return new FormattedText(ToString(culture),
			                         culture,
			                         FlowDirection.LeftToRight,
			                         textSettings.Typeface,
			                         textSettings.FontSize,
			                         TextBrushes.LineNumberForegroundBrush,
			                         1.25);
		}

		private bool Equals(LineNumberFormatter other)
		{
			return _number == other._number;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is LineNumberFormatter && Equals((LineNumberFormatter) obj);
		}

		public override int GetHashCode()
		{
			return _number;
		}
	}
}