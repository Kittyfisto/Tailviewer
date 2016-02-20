using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tailviewer.Ui.Converters
{
	public sealed class ZeroToHiddenConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is int))
				return null;

			var count = (int) value;
			return count == 0
				       ? Visibility.Hidden
				       : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}