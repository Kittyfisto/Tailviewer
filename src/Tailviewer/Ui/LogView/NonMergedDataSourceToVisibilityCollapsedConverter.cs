using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Ui.LogView
{
	/// <summary>
	///     Converts values which implement the <see cref="IMergedDataSourceViewModel" /> interface to
	///     <see cref="Visibility.Visible" /> and anything else to <see cref="Visibility.Collapsed" />.
	/// </summary>
	public sealed class NonMergedDataSourceToVisibilityCollapsedConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is IMergedDataSourceViewModel)
				return Visibility.Visible;

			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}