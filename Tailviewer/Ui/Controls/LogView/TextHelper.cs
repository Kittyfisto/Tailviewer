using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using log4net;

namespace Tailviewer.Ui.Controls.LogView
{
	public static class TextHelper
	{
		public const double FontSize = 12;
		public const double LineSpacing = 3;
		public const double LineHeight = FontSize + LineSpacing;
		public const double LineNumberSpacing = 5;

		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly BrushHelper NormalBackgroundBrush;
		public static readonly Brush DataSourceForegroundBrush;
		public static readonly Brush TraceForegroundBrush;
		public static readonly Brush DebugForegroundBrush;
		public static readonly Brush NormalForegroundBrush;
		public static readonly Brush HoveredForegroundBrush;
		public static readonly Brush NormalHighlightBackgroundBrush;
		public static readonly Brush SelectedForegroundBrush;
		public static readonly Brush SelectedBackgroundBrush;
		public static readonly Brush SelectedUnfocusedBackgroundBrush;
		public static readonly Brush LineNumberForegroundBrush;
		public static readonly Brush HighlightedForegroundBrush;
		public static readonly Brush HighlightedBackgroundBrush;
		public static readonly Brush HighlightedSelectedForegroundBrush;
		public static readonly Brush HighlightedSelectedBackgroundBrush;
		public static readonly Brush WarningBackgroundBrush;
		public static readonly Brush WarningHighlightBackgroundBrush;
		public static readonly Brush ErrorBackgroundBrush;
		public static readonly Brush ErrorHighlightBackgroundBrush;
		public static readonly Brush ErrorForegroundBrush;
		public static readonly Typeface Typeface;
		public static readonly double GlyphWidth;
		public static readonly double TabWidth;

		static TextHelper()
		{
			DataSourceForegroundBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
			DataSourceForegroundBrush.Freeze();

			TraceForegroundBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
			TraceForegroundBrush.Freeze();

			DebugForegroundBrush = TraceForegroundBrush;

			NormalBackgroundBrush = new BrushHelper(null, Color.FromRgb(0xE8, 0xF1, 0xF7));
			NormalForegroundBrush = Brushes.Black;

			NormalHighlightBackgroundBrush = new SolidColorBrush(Color.FromRgb(242, 242, 242));
			NormalHighlightBackgroundBrush.Freeze();

			WarningBackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 195, 0));
			WarningBackgroundBrush.Freeze();

			WarningHighlightBackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 218, 142));
			WarningHighlightBackgroundBrush.Freeze();

			ErrorBackgroundBrush = new SolidColorBrush(Color.FromRgb(232, 17, 35));
			ErrorBackgroundBrush.Freeze();

			ErrorHighlightBackgroundBrush = new SolidColorBrush(Color.FromRgb(250, 108, 118));
			ErrorHighlightBackgroundBrush.Freeze();

			ErrorForegroundBrush = Brushes.White;

			HoveredForegroundBrush = Brushes.Black;
			SelectedBackgroundBrush = new SolidColorBrush(Color.FromRgb(57, 152, 214));
			SelectedBackgroundBrush.Freeze();

			SelectedUnfocusedBackgroundBrush = new SolidColorBrush(Color.FromRgb(215, 215, 215));
			SelectedUnfocusedBackgroundBrush.Freeze();

			HighlightedForegroundBrush = Brushes.Black;
			HighlightedBackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 255, 77));

			HighlightedSelectedForegroundBrush = Brushes.Black;
			HighlightedSelectedBackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 150, 50));

			SelectedForegroundBrush = Brushes.White;
			LineNumberForegroundBrush = new SolidColorBrush(Color.FromRgb(43, 145, 175));
			LineNumberForegroundBrush.Freeze();

			FontFamily family = PickFontFamily();
			Typeface = new Typeface(family, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

			GlyphTypeface test;
			Typeface.TryGetGlyphTypeface(out test);

			ushort glyphIndex = test.CharacterToGlyphMap[' '];
			GlyphWidth = test.AdvanceWidths[glyphIndex]*FontSize;
			TabWidth = new FormattedText("s\t", CultureInfo.CurrentUICulture,
			                             FlowDirection.LeftToRight,
			                             Typeface,
			                             FontSize,
			                             Brushes.Black).Width;
		}

		private static FontFamily PickFontFamily()
		{
			FontFamily consolas =
				Fonts.SystemFontFamilies.FirstOrDefault(
					x => string.Equals(x.Source, "Consolas", StringComparison.InvariantCultureIgnoreCase));

			if (consolas != null)
				return consolas;

			Log.InfoFormat("Consolas is not installed, chosing Inconsolata instead");

			return new FontFamily(new Uri("pack://application:,,,/Tailviewer;Component/Resources/Fonts/"),
			                      "./#Inconsolata");
		}

		/// <summary>
		///     Estimates the width of the given text in DIP units.
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		[Pure]
		public static double EstimateWidthUpperLimit(string line)
		{
			return EstimateWidthUpperLimit(line.Length);
		}

		[Pure]
		public static double EstimateWidthUpperLimit(int characterCount)
		{
			// TODO: What about surrogate pairs?
			// TODO: What about tabs?
			return characterCount*GlyphWidth;
		}
	}
}