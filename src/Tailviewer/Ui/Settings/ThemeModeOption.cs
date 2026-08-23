using Tailviewer.Settings;

namespace Tailviewer.Ui.Settings
{
	/// <summary>
	///     A single selectable theme mode in the settings flyout.
	/// </summary>
	public sealed class ThemeModeOption
	{
		public ThemeModeOption(ThemeMode value, string displayName)
		{
			Value = value;
			DisplayName = displayName;
		}

		public ThemeMode Value { get; }

		public string DisplayName { get; }
	}
}
