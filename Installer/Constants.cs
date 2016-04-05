using System;
using System.Reflection;

namespace Installer
{
	public static class Constants
	{
		public const string ApplicationTitle = "Tailviewer";
		public static readonly Version ApplicationVersion;
		public static readonly string MainWindowTitle;

		static Constants()
		{
			ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version;
			MainWindowTitle = string.Format("Installing Tailviewer, v{0}", ApplicationVersion);
		}
	}
}