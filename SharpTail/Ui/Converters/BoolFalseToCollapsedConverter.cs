using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SharpTail.Ui.Converters
{
	public sealed class BoolFalseToCollapsedConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof (Visibility))
				return null;

			if (!(value is bool))
				return false;

			var val = (bool) value;
			if (!val)
			{
				return Visibility.Collapsed;
			}

			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}