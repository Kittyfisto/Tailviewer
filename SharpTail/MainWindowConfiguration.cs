using System.Windows;
using SharpTail.Properties;

namespace SharpTail
{
	public static class MainWindowConfiguration
	{
		public static void Store(WindowConfiguration config)
		{
			Settings.Default.Left = config.Left;
			Settings.Default.Top = config.Top;
			Settings.Default.Width = config.Width;
			Settings.Default.Height = config.Height;
			Settings.Default.IsMaximized = config.State == WindowState.Maximized;
			Settings.Default.IsMinimized = config.State == WindowState.Minimized;
			Settings.Default.Save();
		}

		public static WindowConfiguration Restore()
		{
			var config = new WindowConfiguration
				{
					Left = Settings.Default.Left,
					Top = Settings.Default.Top,
					Width = Settings.Default.Width,
					Height = Settings.Default.Height,
				};
			if (Settings.Default.IsMaximized)
				config.State = WindowState.Maximized;
			else if (Settings.Default.IsMinimized)
				config.State = WindowState.Minimized;
			else
				config.State = WindowState.Normal;

			if (config.Width == 0 || config.Height == 0)
				return null;

			return config;
		}
	}
}