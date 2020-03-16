using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

namespace Installer.Applications.Uninstall
{
	public class UninstallApplication
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static int Run(Arguments arguments)
		{
			var installationPath = arguments.InstallationPath;
			Log.InfoFormat("Uninstalling {0} from '{1}'...", Constants.ApplicationTitle, installationPath);

			var installer = new Installer();
			var subFolders = new HashSet<string>();
			foreach (var installedFile in installer.InstallationFileNames)
			{
				var fullInstalledPath = Path.Combine(installationPath, installedFile);
				TryDeleteFile(fullInstalledPath);

				var folder = Path.GetDirectoryName(fullInstalledPath);
				subFolders.Add(folder);
			}

			foreach (var subFolder in subFolders)
			{
				if (subFolder != installationPath)
					TryDeleteDirectory(subFolder);
			}

			Log.InfoFormat("Uninstallation finished");

			return 0;
		}

		private static void TryDeleteDirectory(string fullInstalledPath)
		{
			try
			{
				Log.DebugFormat("Deleting '{0}'...", fullInstalledPath);
				Directory.Delete(fullInstalledPath);
				Log.DebugFormat("Deleted '{0}'", fullInstalledPath);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to delete directory: {0}", e.Message);
			}
		}

		private static void TryDeleteFile(string fullInstalledPath)
		{
			try
			{
				Log.DebugFormat("Deleting '{0}'...", fullInstalledPath);
				File.Delete(fullInstalledPath);
				Log.DebugFormat("Deleted '{0}'", fullInstalledPath);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to delete file: {0}", e.Message);
			}
		}
	}
}
