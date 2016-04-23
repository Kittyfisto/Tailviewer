using System;
using System.Diagnostics.Contracts;
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

		public static readonly Brush NormalForegroundBrush;
		public static readonly Brush NormalBackgroundBrush;
		public static readonly Brush HoveredForegroundBrush;
		public static readonly Brush HoveredBackgroundBrush;
		public static readonly Brush SelectedForegroundBrush;
		public static readonly Brush SelectedBackgroundBrush;
		public static readonly Brush LineNumberForegroundBrush;
		public static readonly Typeface Typeface;
		public static readonly double GlyphWidth;

		static TextHelper()
		{
			NormalBackgroundBrush = null;
			NormalForegroundBrush = Brushes.Black;
			HoveredBackgroundBrush = new SolidColorBrush(Color.FromRgb(242, 242, 242));
			HoveredForegroundBrush = Brushes.Black;
			SelectedBackgroundBrush = new SolidColorBrush(Color.FromRgb(57, 152, 214));
			SelectedForegroundBrush = Brushes.White;
			LineNumberForegroundBrush = new SolidColorBrush(Color.FromRgb(43, 145, 175));

			FontFamily family = PickFontFamily();
			Typeface = new Typeface(family, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

			GlyphTypeface test;
			Typeface.TryGetGlyphTypeface(out test);

			ushort glyphIndex = test.CharacterToGlyphMap[' '];
			GlyphWidth = test.AdvanceWidths[glyphIndex]*FontSize;
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