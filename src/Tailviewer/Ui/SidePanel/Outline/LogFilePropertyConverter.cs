using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Tailviewer.Core;
using Tailviewer.Ui.Converters;

namespace Tailviewer.Ui.SidePanel.Outline
{
	public sealed class LogFilePropertyConverter
		: IValueConverter
	{
		private readonly NullToNotAvailableConverter _notAvailable = new NullToNotAvailableConverter();

		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return _notAvailable.Convert(null, targetType, parameter, culture);

			if (value is ILogFileFormat format)
			{
				var description = format.Description;
				if (string.IsNullOrWhiteSpace(description))
					return format.Name;

				return description;
			}

			if (value is Encoding encoding)
			{
				return encoding.WebName;
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return _notAvailable.Convert(null, targetType, parameter, culture);

			return value;
		}

		#endregion
	}
}