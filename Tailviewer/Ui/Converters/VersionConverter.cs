using System;
using System.Globalization;
using System.Windows.Data;

namespace Tailviewer.Ui.Converters
{
	public sealed class VersionConverter
		: IValueConverter
	{
		public bool ShowPatch { get; set; }

		public VersionConverter()
		{
			ShowPatch = true;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var version = value as Version;
			if (version == null)
				return null;

			if (ShowPatch)
				return version.ToString(3);

			return version.ToString(2);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}