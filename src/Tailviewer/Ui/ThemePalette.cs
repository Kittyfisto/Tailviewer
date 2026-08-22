using System;
using System.Windows.Media;

namespace Tailviewer.Ui
{
	/// <summary>
	///     The accent color palette derived from a single theme color.
	/// </summary>
	public sealed class ThemePalette
	{
		public Color Primary { get; }
		public Color PrimaryLight { get; }
		public Color PrimaryLighter { get; }
		public Color PrimaryDark { get; }
		public Color PrimaryForeground { get; }
		public Color PrimarySeparator { get; }

		public Color Secondary { get; }
		public Color SecondaryLight { get; }
		public Color SecondaryLighter { get; }
		public Color SecondaryDark { get; }
		public Color SecondaryForeground { get; }

		private ThemePalette(Color primary)
		{
			Primary = primary;
			PrimaryLight = Lighten(primary, 0.35);
			PrimaryLighter = Lighten(primary, 0.55);
			PrimaryDark = Darken(primary, 0.30);
			PrimaryForeground = Colors.White;
			PrimarySeparator = PrimaryDark;

			Secondary = primary;
			SecondaryLight = PrimaryLight;
			SecondaryLighter = PrimaryLighter;
			SecondaryDark = PrimaryDark;
			SecondaryForeground = Colors.Black;
		}

		/// <summary>
		///     Computes the accent palette for the given base color.
		/// </summary>
		public static ThemePalette Compute(Color primary)
		{
			return new ThemePalette(primary);
		}

		private static Color Lighten(Color color, double factor)
		{
			return Blend(color, Colors.White, factor);
		}

		private static Color Darken(Color color, double factor)
		{
			return Blend(color, Colors.Black, factor);
		}

		private static Color Blend(Color color, Color target, double factor)
		{
			factor = Math.Max(0.0, Math.Min(1.0, factor));
			return Color.FromRgb(
				BlendChannel(color.R, target.R, factor),
				BlendChannel(color.G, target.G, factor),
				BlendChannel(color.B, target.B, factor));
		}

		private static byte BlendChannel(byte source, byte target, double factor)
		{
			return (byte) Math.Round(source + (target - source) * factor);
		}
	}
}
