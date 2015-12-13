using System;
using System.Globalization;
using System.Windows.Data;

namespace Tailviewer.Ui.Converters
{
	internal sealed class TimeSpanConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is TimeSpan))
				return null;

			var age = (TimeSpan) value;
			var oneMonth = TimeSpan.FromDays(30);
			var oneWeek = TimeSpan.FromDays(7);
			var oneDay = TimeSpan.FromDays(1);
			var oneHour = TimeSpan.FromHours(1);
			var oneMinute = TimeSpan.FromMinutes(1);
			var oneSecond = TimeSpan.FromSeconds(1);

			if (age > oneMonth)
				return Format(age, oneMonth, "day");
			if (age > oneWeek)
				return Format(age, oneWeek, "week");
			if (age > oneDay)
				return Format(age, oneDay, "day");
			if (age > oneHour)
				return Format(age, oneHour, "hour");
			if (age > oneMinute)
				return Format(age, oneMinute, "minute");

			return Format(age, oneSecond, "second");
		}

		private object Format(TimeSpan value, TimeSpan divider, string caption)
		{
			var number = (int) (value.TotalMilliseconds/divider.TotalMilliseconds);
			if (number == 1)
				return string.Format("{0} {1}", number, caption);

			return string.Format("{0} {1}s", number, caption);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}