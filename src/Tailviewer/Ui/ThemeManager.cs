using System;
using System.Windows;
using System.Windows.Media;
using Tailviewer.Settings;
using Tailviewer.Ui.LogView;

namespace Tailviewer.Ui
{
	/// <summary>
	///     Applies the accent theme color to the running application by updating
	///     the well-known color resources consumed through DynamicResource bindings.
	/// </summary>
	public static class ThemeManager
	{
		public static Color CurrentPrimary { get; private set; } = UISettings.DefaultThemeColor;

		public static bool CurrentDarkMode { get; private set; }

		/// <summary>
		///     Raised on the UI thread whenever the accent/neutral theme or its derived
		///     log-level palette has been applied. Consumers rebuild their brushes so
		///     the new (frozen) references are picked up.
		/// </summary>
		public static event Action<bool> ThemeChanged;

		public static ThemeMode CurrentThemeMode { get; private set; } = ThemeMode.Light;

		/// <summary>
		///     The log-level palette derived from the currently effective light/dark
		///     theme.
		/// </summary>
		public static LogLevelPalette CurrentLogLevelPalette { get; private set; } = LogLevelPalette.Light;

		/// <summary>
		///     The provider used to resolve the operating system's theme when
		///     <see cref="ThemeMode.System"/> is selected. Tests may replace this with a fake.
		/// </summary>
		public static ISystemThemeProvider SystemThemeProvider { get; set; }

		/// <summary>
		///     Resolves the effective dark-mode flag for the given theme mode. When
		///     <see cref="ThemeMode.System"/> is selected and no provider is registered,
		///     this falls back to light mode.
		/// </summary>
		public static bool ResolveDarkMode(ThemeMode mode)
		{
			switch (mode)
			{
				case ThemeMode.Dark:
					return true;

				case ThemeMode.Light:
					return false;

				case ThemeMode.System:
					var provider = SystemThemeProvider;
					return provider != null && provider.IsDark();

				default:
					return false;
			}
		}

		/// <summary>
		///     Applies the accent color and theme mode. The log-level palette follows
		///     the resolved light/dark mode, including the current system theme.
		/// </summary>
		public static void Apply(Color primary, ThemeMode mode)
		{
			CurrentThemeMode = mode;
			ApplyCore(primary, ResolveDarkMode(mode));
		}

		/// <summary>
		///     Convenience overload for callers that already resolved the effective
		///     light/dark mode.
		/// </summary>
		public static void Apply(Color primary, bool darkMode)
		{
			ApplyCore(primary, darkMode);
		}

		private static void ApplyCore(Color primary, bool darkMode)
		{
			CurrentPrimary = primary;
			CurrentDarkMode = darkMode;
			CurrentLogLevelPalette = darkMode ? LogLevelPalette.Dark : LogLevelPalette.Light;
			TextBrushes.UpdateTheme(primary);
			TextBrushes.UpdateNeutral(darkMode);
			ThemeChanged?.Invoke(darkMode);

			var resources = Application.Current?.Resources;
			if (resources == null)
				return;

			var palette = ThemePalette.Compute(primary);

			resources["Primary"] = palette.Primary;
			resources["PrimaryLight"] = palette.PrimaryLight;
			resources["PrimaryLighter"] = palette.PrimaryLighter;
			resources["PrimaryDark"] = palette.PrimaryDark;
			resources["PrimaryForeground"] = palette.PrimaryForeground;
			resources["PrimarySeparator"] = palette.PrimarySeparator;

			resources["Secondary"] = palette.Secondary;
			resources["SecondaryLight"] = palette.SecondaryLight;
			resources["SecondaryLighter"] = palette.SecondaryLighter;
			resources["SecondaryDark"] = palette.SecondaryDark;
			resources["SecondaryForeground"] = palette.SecondaryForeground;

			var neutral = SemanticPalette.Compute(darkMode);
			resources["Surface"] = neutral.Surface;
			resources["SurfaceMuted"] = neutral.SurfaceMuted;
			resources["TextPrimary"] = neutral.TextPrimary;
			resources["TextSecondary"] = neutral.TextSecondary;
			resources["Divider"] = neutral.Divider;
			resources["TitleBar"] = neutral.TitleBar;
			resources["OverlayBackground"] = neutral.OverlayBackground;
			resources["UnfocusedOverlay"] = neutral.UnfocusedOverlay;
			resources["DisabledForeground"] = neutral.DisabledForeground;
		}
	}
}
