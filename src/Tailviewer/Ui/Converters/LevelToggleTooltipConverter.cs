using System;
using System.Globalization;
using System.Windows.Data;
using Tailviewer.Api;

using Tailviewer.Localization;

namespace Tailviewer.Ui.Converters
{
	public sealed class LevelToggleTooltipConverter
		: IValueConverter
	{
		public LevelFlags Level { get; set; }

		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool isChecked))
				return null;

			var levelName = Level == LevelFlags.All ? Strings.AllWord : Level.ToString();
			return string.Format(Strings.ClickToToggleLevel, isChecked ? Strings.HideWord : Strings.ShowWord, levelName);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
