using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

using Tailviewer.Localization;

namespace Tailviewer.Ui.Converters
{
	public sealed class SkippedDueToTimestampConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is int))
				return null;

			var count = (int) value;
			bool plural = count != 1;
			var builder = new StringBuilder();

			builder.AppendFormat(Strings.LogEntriesSuffix, count);
			builder.Append(plural ? Strings.LogEntriesAre : Strings.LogEntryIs);
			builder.Append(Strings.NotDisplayedBecause);
			builder.AppendFormat(plural ? Strings.TheyDo : Strings.ItDoes);
			builder.Append(Strings.NoTimestampReason);

			return builder.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}