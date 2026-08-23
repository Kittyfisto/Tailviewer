using System.Windows.Media;

namespace Tailviewer.Ui
{
	/// <summary>
	///     The neutral (non-accent) semantic color palette. It provides the light and
	///     dark variants of the well-known neutral <see cref="Color"/> keys that are
	///     published at application scope and consumed through DynamicResource bindings.
	/// </summary>
	public sealed class SemanticPalette
	{
		public Color Surface { get; }
		public Color SurfaceMuted { get; }
		public Color TextPrimary { get; }
		public Color TextSecondary { get; }
		public Color Divider { get; }
		public Color TitleBar { get; }
		public Color OverlayBackground { get; }
		public Color UnfocusedOverlay { get; }
		public Color DisabledForeground { get; }

		private SemanticPalette(Color surface,
		                        Color surfaceMuted,
		                        Color textPrimary,
		                        Color textSecondary,
		                        Color divider,
		                        Color titleBar,
		                        Color overlayBackground,
		                        Color unfocusedOverlay,
		                        Color disabledForeground)
		{
			Surface = surface;
			SurfaceMuted = surfaceMuted;
			TextPrimary = textPrimary;
			TextSecondary = textSecondary;
			Divider = divider;
			TitleBar = titleBar;
			OverlayBackground = overlayBackground;
			UnfocusedOverlay = unfocusedOverlay;
			DisabledForeground = disabledForeground;
		}

		/// <summary>
		///     Computes the neutral palette for the given theme mode.
		/// </summary>
		/// <param name="darkMode"></param>
		public static SemanticPalette Compute(bool darkMode)
		{
			if (darkMode)
			{
				return new SemanticPalette(
					surface: Color.FromRgb(0x1E, 0x1E, 0x1E),
					surfaceMuted: Color.FromRgb(0x2D, 0x2D, 0x30),
					textPrimary: Color.FromRgb(0xDC, 0xDC, 0xDC),
					textSecondary: Color.FromRgb(0x9E, 0x9E, 0x9E),
					divider: Color.FromRgb(0x3F, 0x3F, 0x46),
					titleBar: Color.FromRgb(0x25, 0x25, 0x26),
					overlayBackground: Color.FromArgb(0x99, 0x00, 0x00, 0x00),
					unfocusedOverlay: Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF),
					disabledForeground: Color.FromRgb(0x6B, 0x6B, 0x6B));
			}

			return new SemanticPalette(
				surface: Color.FromRgb(0xFF, 0xFF, 0xFF),
				surfaceMuted: Color.FromRgb(0xD8, 0xD8, 0xD8),
				textPrimary: Color.FromRgb(0x33, 0x33, 0x33),
				textSecondary: Color.FromRgb(0xA0, 0xA0, 0xA0),
				divider: Color.FromRgb(0x80, 0x80, 0x80),
				titleBar: Color.FromRgb(0xEA, 0xED, 0xF2),
				overlayBackground: Color.FromArgb(0x55, 0x00, 0x00, 0x00),
				unfocusedOverlay: Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF),
				disabledForeground: Color.FromRgb(0xB7, 0xB7, 0xB7));
		}
	}
}
