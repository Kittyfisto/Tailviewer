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
		public static readonly string ApplicationDataFolder;
		public static readonly string ApplicationLogFile;
		public static readonly string ApplicationConfigurationFile;
		public static readonly string ServiceLogFile;
		public static readonly string PluginDatabaseFilePath;

		static Constants()
		{
			ApplicationTitle = "Tailviewer.PluginRepository";
			ApplicationVersion = Core.Constants.ApplicationVersion;
			BuildDate = Core.Constants.BuildDate;
			ApplicationFolder = Core.Constants.ApplicationFolder;
			ApplicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ApplicationTitle);
			ApplicationLogFile = Path.Combine(ApplicationDataFolder, "repository.log");
			ApplicationConfigurationFile = Path.Combine(ApplicationDataFolder, "configuration.xml");
			ServiceLogFile = Path.Combine(ApplicationDataFolder, "repository-svc.log");
			PluginDatabaseFilePath = Path.Combine(ApplicationDataFolder, "database.isdb");
		}
	}
}
