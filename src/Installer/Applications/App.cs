using System;
using System.Reflection;
using Installer.Applications.Install;
using Installer.Applications.SilentInstall;
using Installer.Applications.Uninstall;
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

			try
			{
				var arguments = Arguments.Parse(args);
				switch (arguments.Mode)
				{
					case Mode.Install:
						return InstallApplication.Run(arguments);

					case Mode.SilentInstall:
						return SilentInstallApplication.Run(arguments);

					case Mode.Update:
						return UpdateApplication.Run(arguments);

					case Mode.Uninstall:
						return UninstallApplication.Run(arguments);

					default:
						throw new Exception($"Unknown mode: {arguments.Mode}");
				}
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Error: {0}", e.Message);
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

			var consoleLayout = new PatternLayout
			{
				ConversionPattern = "%-5level: %message%newline"
			};
			consoleLayout.ActivateOptions();
			var consoleAppender = new ConsoleAppender
			{
				Layout = consoleLayout
			};
			consoleAppender.ActivateOptions();
			hierarchy.Root.AddAppender(consoleAppender);

			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}
	}
}