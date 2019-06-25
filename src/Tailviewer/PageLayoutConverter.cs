using System;
using System.Globalization;
using System.Windows.Data;
using Tailviewer.Core.Analysis;

namespace Tailviewer
{
	public class PageLayoutConverter
		: IValueConverter
	{
		public PageLayout Layout { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var layout = value as PageLayout?;
			if (layout == null)
				return null;

			return layout == Layout;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool))
				return Binding.DoNothing;

			var isChecked = (bool) value;
			if (isChecked)
				return Layout;

			return Binding.DoNothing;
		}
	}
}