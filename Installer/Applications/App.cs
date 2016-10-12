using System.Reflection;
using Installer.Applications.Install;
using Installer.Applications.SilentInstall;
using Installer.Applications.Update;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Installer.Applications
{
	public static class App
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static int Run(string[] args)
		{
			SetupLoggers();

			Log.InfoFormat("Starting setup...");
			Log.InfoFormat("Commandline arguments: {0}", string.Join(" ", args));

			var arguments = Arguments.Parse(args);
			switch (arguments.Mode)
			{
				case Mode.Install:
					return InstallApplication.Run(arguments);

				case Mode.SilentInstall:
					return SilentInstallApplication.Run(arguments);

				case Mode.Update:
					return UpdateApplication.Run(arguments);

				default:
					return -1;
			}
		}

		private static void SetupLoggers()
		{
			var hierarchy = (Hierarchy)LogManager.GetRepository();

			var patternLayout = new PatternLayout
			{
				ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
			};
			patternLayout.ActivateOptions();

			var fileAppender = new RollingFileAppender
			{
				AppendToFile = false,
				File = Constants.InstallationLog,
				Layout = patternLayout,
				MaxSizeRollBackups = 3,
				MaximumFileSize = "1GB",
				RollingStyle = RollingFileAppender.RollingMode.Size,
				StaticLogFileName = false
			};
			fileAppender.ActivateOptions();
			hierarchy.Root.AddAppender(fileAppender);

			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}
	}
}