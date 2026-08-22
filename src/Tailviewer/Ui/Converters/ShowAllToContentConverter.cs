using System;
using System.Globalization;
using System.Windows.Data;

using Tailviewer.Localization;

namespace Tailviewer.Ui.Converters
{
	public sealed class ShowAllToContentConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var showAll = value as bool?;
			if (showAll == true)
				return Strings.All;
			if (showAll == false)
				return Strings.None;
			return Strings.Mixed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}