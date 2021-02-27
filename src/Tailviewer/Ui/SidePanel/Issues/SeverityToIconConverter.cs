using System;
using System.Globalization;
using System.Windows.Data;
using Metrolib;

namespace Tailviewer.Ui.SidePanel.Issues
{
	internal sealed class SeverityToIconConverter
		: IValueConverter
	{
		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Icons.Alert;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}