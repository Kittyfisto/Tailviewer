using System;
using System.Reflection;
using System.Windows;
using log4net;

namespace Installer.Applications.SilentInstall
{
	public sealed class SilentInstallApplication
		: Application
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static int Run(Arguments args)
		{
			try
			{
				Log.InfoFormat("Starting silent installation of v{0} to {1}",
				               Constants.ApplicationVersion,
				               args.InstallationPath);

				var installer = new Installer();
				installer.Run(args.InstallationPath);

				Log.InfoFormat("Installation finished");

				return 0;
			}
			catch (Exception e)
			{
				Log.FatalFormat("Unable to complete installation: {0}", e);
				return -1;
			}
		}
	}
}