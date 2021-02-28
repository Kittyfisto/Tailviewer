using System;
using System.Globalization;
using System.Windows.Data;

namespace Tailviewer.Ui.Converters
{
	public sealed class LevelToggleTooltipConverter
		: IValueConverter
	{
		public LevelFlags Level { get; set; }

		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool isChecked))
				return null;

			return string.Format("Click to {0} {1} log entries", isChecked ? "hide" : "show", Level);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
