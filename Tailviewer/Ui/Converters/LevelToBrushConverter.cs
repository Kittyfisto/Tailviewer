using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Ui.Converters
{
	public sealed class LevelToBrushConverter
		: IMultiValueConverter
	{
		private static readonly SolidColorBrush HighlightBackgroundBrush;
		private static readonly SolidColorBrush WarningBackgroundBrush;
		private static readonly SolidColorBrush WarningHighlightBackgroundBrush;
		private static readonly SolidColorBrush ErrorBackgroundBrush;
		private static readonly SolidColorBrush ErrorHighlightBackgroundBrush;

		static LevelToBrushConverter()
		{
			HighlightBackgroundBrush = new SolidColorBrush(Color.FromRgb(242, 242, 242));
			WarningBackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 195, 0));
			WarningHighlightBackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 218, 142));
			ErrorBackgroundBrush = new SolidColorBrush(Color.FromRgb(232, 17, 35));
			ErrorHighlightBackgroundBrush = new SolidColorBrush(Color.FromRgb(250, 108, 118));
		}

		public BrushType Type { get; set; }
		public bool IsHovered { get; set; }
		public bool IsSelected { get; set; }

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			LevelFlags level;
			bool hasColors;
			if (!TryExtract(values, out level, out hasColors))
				return null;

			if (hasColors)
			{
				switch (level)
				{
					case LevelFlags.Fatal:
					case LevelFlags.Error:
						return Error();

					case LevelFlags.Warning:
						return Warning();

					default:
						return Default();
				}
			}

			switch (Type)
			{
				case BrushType.Foreground:
					return Brushes.Black;

				case BrushType.Background:
					return IsHovered
						       ? HighlightBackgroundBrush
						       : Brushes.Transparent;

				default:
					return null;
			}
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}

		private Brush Error()
		{
			switch (Type)
			{
				case BrushType.Foreground:
					return Brushes.White;

				case BrushType.Background:
					return IsHovered
						       ? ErrorHighlightBackgroundBrush
						       : ErrorBackgroundBrush;

				default:
					return null;
			}
		}

		private Brush Warning()
		{
			switch (Type)
			{
				case BrushType.Foreground:
					return Brushes.Black;

				case BrushType.Background:
					return IsHovered
						       ? WarningHighlightBackgroundBrush
						       : WarningBackgroundBrush;

				default:
					return null;
			}
		}

		private Brush Default()
		{
			switch (Type)
			{
				case BrushType.Foreground:
					return Brushes.Black;

				case BrushType.Background:
					return IsHovered
						       ? HighlightBackgroundBrush
						       : Brushes.Transparent;

				default:
					return null;
			}
		}

		private bool TryExtract(object[] values, out LevelFlags level, out bool hasColors)
		{
			if (values != null && values.Length >= 2)
			{
				object lvl = values[0];
				object clr = values[1];
				if (lvl is LevelFlags && clr is bool)
				{
					level = (LevelFlags) lvl;
					hasColors = (bool) clr;
					return true;
				}
			}

			level = LevelFlags.None;
			hasColors = false;
			return false;
		}
	}
}