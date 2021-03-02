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
			installer.Uninstall(installationPath);

			Log.InfoFormat("Uninstallation finished");

			return 0;
		}
	}
}
