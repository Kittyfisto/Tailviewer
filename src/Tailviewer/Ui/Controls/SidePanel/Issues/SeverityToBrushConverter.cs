using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Tailviewer.BusinessLogic.Plugins.Issues;

namespace Tailviewer.Ui.Controls.SidePanel.Issues
{
	internal sealed class SeverityToBrushConverter
		: IValueConverter
	{
		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Severity))
				return null;

			var severity = (Severity) value;
			switch (severity)
			{
				case Severity.Minor:
					return Brushes.Green;

				case Severity.Major:
					return Brushes.Orange;

				case Severity.Critical:
					return Brushes.Red;

				default:
					return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
