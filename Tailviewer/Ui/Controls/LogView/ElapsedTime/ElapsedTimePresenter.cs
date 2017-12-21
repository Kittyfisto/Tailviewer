using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Tailviewer.Ui.Controls.LogView.DeltaTimes;

namespace Tailviewer.Ui.Controls.LogView.ElapsedTime
{
	public sealed class ElapsedTimePresenter
		: AbstractLogEntryValuePresenter
	{
		private readonly TimeSpan? _value;

		public const int CharacterWidth = 11;

		public ElapsedTimePresenter(TimeSpan? value)
		{
			_value = value;
		}

		public override string ToString(IFormatProvider provider)
		{
			if (_value != null)
			{
				return _value.Value.ToString(@"dd\.hh\:mm\:ss", provider);
			}

			return string.Empty;
		}

		protected override FormattedText CreateFormattedText(string text, CultureInfo culture)
		{
			return new FormattedText(text,
			                         culture,
			                         FlowDirection.LeftToRight,
			                         TextHelper.Typeface,
			                         TextHelper.FontSize,
			                         TextHelper.LineNumberForegroundBrush);
		}
	}
}