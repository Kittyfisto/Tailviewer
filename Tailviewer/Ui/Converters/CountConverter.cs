using System;
using System.Globalization;
using System.Windows.Data;

namespace Tailviewer.Ui.Converters
{
	public sealed class CountConverter
		: IValueConverter
	{
		private const int Million = 1000000;
		private const int Thousand = 1000;

		public string Suffix { get; set; }
		public bool HasPlural { get; set; }

		public CountConverter()
		{
			HasPlural = true;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is int))
				return "-";

			var count = (int) value;
			if (count > Million)
				return Format(culture, count / Million, "M");
			if (count > Thousand)
				return Format(culture, count / Thousand, "k");

			return Format(culture, count, "");
		}

		private string Format(CultureInfo culture, int count, string quantifier)
		{
			if (HasPlural && count != 1)
			{
				return string.Format(culture, "{0}{1} {2}s", count, quantifier, Suffix);
			}

			return string.Format(culture, "{0}{1} {2}", count, quantifier, Suffix);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}