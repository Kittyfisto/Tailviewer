using System;
using System.Globalization;
using System.Windows.Data;

namespace NewDesign
{
	public sealed class IconConverter
		: IValueConverter
	{
		public object Home { get; set; }
		public object Data { get; set; }
		public object Visualise { get; set; }
		public object Raw { get; set; }
		public object Settings { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is string))
				return null;

			var name = (string) value;
			if (name == "Home")
				return Home;
			if (name == "Data")
				return Data;
			if (name == "Visualise")
				return Visualise;
			if (name == "Raw")
				return Raw;
			if (name == "Settings")
				return Settings;

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}