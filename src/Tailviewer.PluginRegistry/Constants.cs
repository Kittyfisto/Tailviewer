using System;
using System.IO;

namespace Tailviewer.PluginRegistry
{
	public static class Constants
	{
		public static readonly string ApplicationTitle;
		public static readonly Version ApplicationVersion;
		public static readonly DateTime BuildDate;
		public static readonly string ApplicationFolder;
		public static readonly string AppDataLocalFolder;
		public static readonly string ApplicationLogFile;
		public static readonly string PluginDatabaseFilePath;

		static Constants()
		{
			ApplicationTitle = "Tailviewer.PluginRegistry";
			ApplicationVersion = Core.Constants.ApplicationVersion;
			BuildDate = Core.Constants.BuildDate;
			ApplicationFolder = Core.Constants.ApplicationFolder;
			AppDataLocalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationTitle);
			ApplicationLogFile = Path.Combine(AppDataLocalFolder, $"{ApplicationTitle}.log");
			PluginDatabaseFilePath = Path.Combine(AppDataLocalFolder, "Plugins.isdb");
		}
	}
}
