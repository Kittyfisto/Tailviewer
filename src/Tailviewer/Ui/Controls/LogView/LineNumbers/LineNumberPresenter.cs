using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Tailviewer.Ui.Controls.LogView.LineNumbers
{
	public sealed class LineNumberPresenter
		: AbstractLogEntryValuePresenter
	{
		private readonly int _number;

		public LineNumberPresenter(int value)
		{
			_number = value;
		}

		public override string ToString(IFormatProvider provider)
		{
			return _number.ToString(provider);
		}

		protected override FormattedText CreateFormattedText(string text, CultureInfo culture)
		{
			return new FormattedText(ToString(culture),
			                         culture,
			                         FlowDirection.LeftToRight,
			                         TextHelper.Typeface,
			                         TextHelper.FontSize,
			                         TextHelper.LineNumberForegroundBrush,
			                         1.25);
		}

		private bool Equals(LineNumberPresenter other)
		{
			return _number == other._number;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is LineNumberPresenter && Equals((LineNumberPresenter) obj);
		}

		public override int GetHashCode()
		{
			return _number;
		}
	}
}