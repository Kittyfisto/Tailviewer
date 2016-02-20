using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

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

			builder.AppendFormat("{0} log entry",count);
			builder.Append(plural ? "s are" : " is");
			builder.Append(" not displayed in the group because ");
			builder.AppendFormat(plural ? "they do" : "it does");
			builder.Append(" not contain a timestamp or because its timestamp was not recognized");

			return builder.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}