using System;
using System.Globalization;
using System.Windows.Data;

namespace Tailviewer.Ui.Converters
{
	public sealed class VersionConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var version = value as Version;
			return version?.Format();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}