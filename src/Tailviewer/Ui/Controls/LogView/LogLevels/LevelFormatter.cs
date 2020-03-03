using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;

namespace Tailviewer.Ui.Controls.LogView.LogLevels
{
	public sealed class LevelFormatter
		: AbstractLogEntryValueFormatter
	{
		private readonly LevelFlags _value;

		public LevelFormatter(LevelFlags value, TextSettings textSettings)
			: base(textSettings)
		{
			_value = value;
		}

		public static string ToString(LevelFlags value, IFormatProvider provider)
		{
			return value.ToString();
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
			                         TextHelper.LineNumberForegroundBrush,
			                         1.25);
		}

		#endregion
	}
}
