namespace Tailviewer.Settings
{
	/// <summary>
	///     Determines whether the application renders in light or dark colors, or
	///     whether it follows the operating system's current theme.
	/// </summary>
	public enum ThemeMode
	{
		/// <summary>
		///     Always render in light mode.
		/// </summary>
		Light = 0,

		/// <summary>
		///     Always render in dark mode.
		/// </summary>
		Dark = 1,

		/// <summary>
		///     Follow the operating system's theme.
		/// </summary>
		System = 2
	}
}
