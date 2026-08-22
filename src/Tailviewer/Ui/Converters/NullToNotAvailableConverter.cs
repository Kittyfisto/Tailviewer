using System;
using System.Globalization;
using System.Windows.Data;

using Tailviewer.Localization;

namespace Tailviewer.Ui.Converters
{
	/// <summary>
	///     A converter which converts null values to N/A.
	/// </summary>
	public sealed class NullToNotAvailableConverter
		: IValueConverter
	{
		/// <inheritdoc />
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return Strings.NotAvailable;

			return value;
		}

		/// <inheritdoc />
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}