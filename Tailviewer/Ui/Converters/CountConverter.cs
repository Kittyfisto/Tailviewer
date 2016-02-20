using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Windows.Data;

namespace Tailviewer.Ui.Converters
{
	/// <summary>
	/// Responsible for converting (big) numbers to readable text, omitting uninteresting decimals, e.g.
	/// 12.344.112 becomes 12M.
	/// </summary>
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

		[Pure]
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is int))
				return "-";

			var count = (int) value;
			if (count > Million)
				return Format(culture, count / Million, count, "M");
			if (count > Thousand)
				return Format(culture, count / Thousand, count, "k");

			return Format(culture, count, count, "");
		}

		[Pure]
		private string Format(CultureInfo culture, int count, int totalCount, string quantifier)
		{
			if (!string.IsNullOrEmpty(Suffix))
			{
				if (HasPlural && totalCount != 1)
				{
					return string.Format(culture, "{0}{1} {2}s", count, quantifier, Suffix);
				}

				return string.Format(culture, "{0}{1} {2}", count, quantifier, Suffix);
			}

			return string.Format(culture, "{0}{1}", count, quantifier);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}