using System;
using System.Globalization;
using System.Windows.Data;

using Tailviewer.Localization;

namespace Tailviewer.Ui.Converters
{
	public sealed class FollowTailTooltipConverter
		: IValueConverter
	{
		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool isChecked))
				return null;

			if (isChecked)
				return Strings.ClickToStopFollowingTail;

			return Strings.ClickToFollowTail;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
