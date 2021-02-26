using System;
using System.Globalization;
using System.Windows.Data;

namespace Tailviewer.Ui.Converters
{
	/// <summary>
	///     Ensures that special values such as NaN, positive/negative infinity are converted to 0.
	///     Every other double value is simply forwarded.
	/// </summary>
	public sealed class AdjustingDoubleConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double))
				return null;

			var tmp = (double) value;
			if (double.IsNaN(tmp))
				return 0.0;

			if (double.IsNegativeInfinity(tmp))
				return 0.0;

			if (double.IsPositiveInfinity(tmp))
				return 0.0;

			// Avoid additional boxing and just return the original, already boxed value
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}