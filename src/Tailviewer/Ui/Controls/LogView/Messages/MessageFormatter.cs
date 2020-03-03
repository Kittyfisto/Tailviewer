using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView.Messages
{
	public sealed class MessageFormatter
		: AbstractLogEntryValueFormatter
	{
		private readonly string _value;

		public MessageFormatter(string value, TextSettings textSettings)
			: base(textSettings)
		{
			_value = value;
		}

		#region Overrides of AbstractLogEntryValueFormatter

		public override string ToString(IFormatProvider provider)
		{
			return _value;
		}

		protected override FormattedText CreateFormattedText(string text, CultureInfo culture, TextSettings textSettings)
		{
			return new FormattedText(text,
			                         culture,
			                         FlowDirection.LeftToRight,
			                         textSettings.Typeface,
			                         textSettings.FontSize,
			                         TextHelper.LineNumberForegroundBrush,
			                         1.25);
		}

		#endregion
	}
}
