using System;
using System.IO;
using System.Reflection;

namespace Installer
{
	public static class Constants
	{
		public const string ApplicationTitle = "Tailviewer NEXT";
		public static readonly string ApplicationExecutable = $"{ApplicationTitle}.exe";
		public static readonly Version ApplicationVersion;
		public static readonly string MainWindowTitle;
		public static readonly string InstallationLog;
		public static readonly string DefaultApplicationFolder;
		public static readonly string Publisher;

		static Constants()
		{
			ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version;
			MainWindowTitle = string.Format("Installing Tailviewer NEXT, v{0}", ApplicationVersion);
			InstallationLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			                               "Tailviewer NEXT", "Installation.log");
			Publisher = "brofea";
			DefaultApplicationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), ApplicationTitle);
		}
	}
}