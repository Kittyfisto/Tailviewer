using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Tailviewer.Core;

namespace Tailviewer.PluginRepository
{
	/// <summary>
	/// 
	/// </summary>
	public static class Logging
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static void SetupFileAppender(string fileName)
		{
			var patternLayout = new PatternLayout
			{
				ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
			};
			patternLayout.ActivateOptions();

			var fileAppender = new RollingFileAppender
			{
				AppendToFile = false,
				File = fileName,
				Layout = patternLayout,
				MaxSizeRollBackups = 20,
				MaximumFileSize = "1GB",
				RollingStyle = RollingFileAppender.RollingMode.Size,
				StaticLogFileName = false
			};
			fileAppender.ActivateOptions();

			var hierarchy = (Hierarchy)LogManager.GetRepository();
			hierarchy.Root.AddAppender(fileAppender);
			hierarchy.Configured = true;
		}

		public static void InstallExceptionHandlers()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
		}

		private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
		{
			object exception = args.ExceptionObject;

			Log.ErrorFormat("Caught unhandled exception in AppDomain: {0}", exception);
		}

		public static void SetupConsoleLogger(bool logTimestamps)
		{
			var hierarchy = (Hierarchy)LogManager.GetRepository();

			hierarchy.Root.AddAppender(new ColoringConsoleAppender(logTimestamps));
			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}

		public static void LogEnvironment()
		{
			var builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendFormat("{0}: v{1}, {2}",
				Constants.ApplicationTitle,
				FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location)
					.ProductVersion,
				Environment.Is64BitProcess ? "64bit" : "32bit");
			builder.AppendLine();

			builder.AppendFormat("Build date: {0}", Constants.BuildDate);
			builder.AppendLine();

			builder.AppendFormat(".NET Environment: {0}", Environment.Version);
			builder.AppendLine();

			builder.AppendFormat("Operating System: {0}, {1}\r\n",
				Environment.OSVersion,
				Environment.Is64BitOperatingSystem ? "64bit" : "32bit");
			builder.AppendFormat("Current directory: {0}", Directory.GetCurrentDirectory());

			Log.InfoFormat("Environment: {0}", builder);
		}
	}
}