using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tailviewer.Ui.Controls.LogView
{
	public sealed class DoubleOneToHiddenConverter
		: IValueConverter
	{
		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double actualValue))
				return Visibility.Hidden;

			if (actualValue < 1)
				return Visibility.Visible;

			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}