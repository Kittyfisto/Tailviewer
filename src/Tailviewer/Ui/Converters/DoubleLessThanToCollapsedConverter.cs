using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tailviewer.Ui.Converters
{
	/// <summary>
	///     Converts a double (e.g. an element's <see cref="FrameworkElement.ActualWidth" />) to
	///     <see cref="Visibility.Collapsed" /> when it is smaller than the value supplied as the
	///     converter parameter, and to <see cref="Visibility.Visible" /> otherwise.
	///     A non-positive input is treated as "not measured yet" and therefore yields Visible.
	/// </summary>
	public sealed class DoubleLessThanToCollapsedConverter
		: IValueConverter
	{
		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double threshold;
			if (!double.TryParse(parameter as string, NumberStyles.Float, CultureInfo.InvariantCulture, out threshold))
				return Visibility.Visible;

			var actual = value as double?;
			if (actual == null || actual.Value <= 0)
				return Visibility.Visible;

			return actual.Value < threshold ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
