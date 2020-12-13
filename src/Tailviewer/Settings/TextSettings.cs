using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using log4net;

namespace Tailviewer.Settings
{
	public sealed class TextSettings
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly TextSettings Default = new TextSettings();

		public readonly double FontSize;
		public readonly double LineSpacing;
		public readonly double LineHeight;
		public readonly double LineNumberSpacing;
		public readonly Typeface Typeface;
		public readonly double GlyphWidth;
		public readonly int TabWidth;

		public TextSettings(int fontSize = LogViewerSettings.DefaultFontSize,
		                    int tabWidth = LogViewerSettings.DefaultTabWidth)
		{
			FontSize = fontSize;
			LineSpacing = 3;
			LineHeight = FontSize + LineSpacing;
			LineNumberSpacing = 5;

			FontFamily family = PickFontFamily();
			Typeface = new Typeface(family, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

			Typeface.TryGetGlyphTypeface(out var test);

			ushort glyphIndex = test.CharacterToGlyphMap[' '];
			GlyphWidth = test.AdvanceWidths[glyphIndex]*FontSize;
			TabWidth = tabWidth;
		}

		#region Overrides of Object

		public override int GetHashCode()
		{
			return 42;
		}

		public override bool Equals(object obj)
		{
			var other = obj as TextSettings;
			if (other == null)
				return false;

			return FontSize == other.FontSize &&
			       LineSpacing == other.LineSpacing &&
			       LineHeight == other.LineHeight &&
			       LineNumberSpacing == other.LineNumberSpacing &&
			       Equals(Typeface, other.Typeface);
		}

		#endregion

		/// <summary>
		///     Estimates the width of the given text in DIP units.
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		[Pure]
		public double EstimateWidthUpperLimit(string line)
		{
			return EstimateWidthUpperLimit(line.Length);
		}

		[Pure]
		public double EstimateWidthUpperLimit(int characterCount)
		{
			// TODO: What about surrogate pairs?
			// TODO: What about tabs?
			return characterCount*GlyphWidth;
		}

		private static FontFamily PickFontFamily()
		{
			try
			{
				FontFamily consolas =
					Fonts.SystemFontFamilies.FirstOrDefault(
					                                        x => string.Equals(x.Source, "Consolas",
					                                                           StringComparison.InvariantCultureIgnoreCase));

				if (consolas != null)
					return consolas;
			}
			catch (Exception e)
			{
				// This crash only occured on one system and vanished after about 3 days so
				// I really do not know why the SystemFontFamilies property would throw...
				// See https://github.com/Kittyfisto/Tailviewer/issues/161
				Log.ErrorFormat("Caught exception while trying to find proper font: {0}", e);
			}

			Log.InfoFormat("Consolas is not installed, chosing Inconsolata instead");
			return new FontFamily(new Uri("pack://application:,,,/Tailviewer;Component/Resources/Fonts/"),
			                      "./#Inconsolata");
		}
	}
}