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

		/// <summary>
		///     Computes the palette for the given base color and publishes it to
		///     <see cref="Application.Current"/>'s resources. Does nothing when there
		///     is no current application (e.g. unit tests or design time).
		/// </summary>
		/// <param name="primary"></param>
		public static void Apply(Color primary)
		{
			CurrentPrimary = primary;
			TextBrushes.UpdateTheme(primary);

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
		}
	}
}
