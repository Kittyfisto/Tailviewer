using System;
using System.Windows;
using Microsoft.Win32;

namespace Tailviewer.Ui
{
	/// <summary>
	///     Provides the operating system's current application theme (light or dark).
	/// </summary>
	public interface ISystemThemeProvider
	{
		/// <summary>
		///     Returns true when the operating system is configured to use a dark
		///     application theme, false otherwise.
		/// </summary>
		bool IsDark();

		/// <summary>
		///     Raised on the UI thread when the operating system's theme has changed.
		/// </summary>
		event Action Changed;
	}

	/// <summary>
	///     Reads the Windows "AppsUseLightTheme" user preference and raises
	///     <see cref="Changed"/> when the system theme changes.
	/// </summary>
	public sealed class WindowsSystemThemeProvider
		: ISystemThemeProvider
		, IDisposable
	{
		private const string PersonalizeKeyPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
		private const string AppsUseLightThemeValueName = "AppsUseLightTheme";

		public WindowsSystemThemeProvider()
		{
			SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
		}

		public event Action Changed;

		public bool IsDark()
		{
			var value = Registry.GetValue(PersonalizeKeyPath, AppsUseLightThemeValueName, null);
			if (value is int appsUseLightTheme)
				return appsUseLightTheme == 0;

			return false;
		}

		public void Dispose()
		{
			SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
		}

		private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			if (e.Category != UserPreferenceCategory.General)
				return;

			var dispatcher = Application.Current?.Dispatcher;
			if (dispatcher == null)
				return;

			dispatcher.BeginInvoke(new Action(() => Changed?.Invoke()));
		}
	}
}
