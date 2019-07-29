using System;
using System.IO;

namespace Tailviewer.PluginRepository
{
	public static class Constants
	{
		public static readonly string ApplicationTitle;
		public static readonly Version ApplicationVersion;
		public static readonly DateTime BuildDate;
		public static readonly string ApplicationFolder;
		public static readonly string AppDataLocalFolder;
		public static readonly string ProgramDataFolder;
		public static readonly string ApplicationLogFile;
		public static readonly string ServiceLogFile;
		public static readonly string PluginDatabaseFilePath;

		static Constants()
		{
			ApplicationTitle = "Tailviewer.PluginRepository";
			ApplicationVersion = Core.Constants.ApplicationVersion;
			BuildDate = Core.Constants.BuildDate;
			ApplicationFolder = Core.Constants.ApplicationFolder;
			AppDataLocalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationTitle);
			ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ApplicationTitle);
			ApplicationLogFile = Path.Combine(AppDataLocalFolder, "repository.log");
			ServiceLogFile = Path.Combine(AppDataLocalFolder, "repository-svc.log");
			PluginDatabaseFilePath = Path.Combine(ProgramDataFolder, "Plugins.isdb");
		}
	}
}
