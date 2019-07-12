using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using CommandLine;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Tailviewer.PluginRepository.Applications;

namespace Tailviewer.PluginRepository
{
	internal sealed class App
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static int Run(string[] args)
		{
			try
			{
				InstallExceptionHandlers();
				Log.InfoFormat("Starting {0}...", Constants.ApplicationTitle);
				Log.InfoFormat("Commandline arguments: {0}", string.Join(" ", args));
				LogEnvironment();

				var result = Parser.Default.ParseArguments<RunServerOptions,
					AddPluginOptions, ListPluginsOptions, RemovePluginOptions,
					AddUserOptions, ListUsersOptions, RemoveUserOptions,
					ExportOptions, WriteConfigurationOptions
				>(args);

				return result.MapResult(
				                        (RunServerOptions options) => Run<RunServer, RunServerOptions>(options, logTimestamps: true, logToFile: true),
				                        (AddPluginOptions options) => Run<AddPlugin, AddPluginOptions>(options),
				                        (ListPluginsOptions options) => Run<ListPlugins, ListPluginsOptions>(options),
										(RemovePluginOptions options) => Run<RemovePlugin, RemovePluginOptions>(options),
										(AddUserOptions options) => Run<AddUser, AddUserOptions>(options),
										(ListUsersOptions options) => Run<ListUsers, ListUsersOptions>(options),
										(RemoveUserOptions options) => Run<RemoveUser, RemoveUserOptions>(options),
										(ExportOptions options) => Run<Export, ExportOptions>(options),
										(WriteConfigurationOptions options) => Run<WriteConfiguration, WriteConfigurationOptions>(options),
				                        _ => -2);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Exiting due to unexpected exception: {0}", e);
				return -1;
			}
		}

		private static int Run<TApp, TOptions>(TOptions options, bool logTimestamps = false, bool logToFile = false) where TApp : class, IApplication<TOptions>, new()
		{
			SetupConsoleLogger(logTimestamps);

			if (logToFile)
				SetupFileAppender();

			using (var taskScheduler = new DefaultTaskScheduler())
			{
				var filesystem = new Filesystem(taskScheduler);
				using (var repo = PluginRepository.Create())
				{
					var app = new TApp();
					return app.Run(filesystem, repo, options);
				}
			}
		}

		private static void SetupConsoleLogger(bool logTimestamps)
		{
			var hierarchy = (Hierarchy) LogManager.GetRepository();

			hierarchy.Root.AddAppender(new ColoringConsoleAppender(logTimestamps));
			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}

		private static void SetupFileAppender()
		{
			var patternLayout = new PatternLayout
			{
				ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
			};
			patternLayout.ActivateOptions();

			var fileAppender = new RollingFileAppender
			{
				AppendToFile = false,
				File = Constants.ApplicationLogFile,
				Layout = patternLayout,
				MaxSizeRollBackups = 20,
				MaximumFileSize = "1GB",
				RollingStyle = RollingFileAppender.RollingMode.Size,
				StaticLogFileName = false
			};
			fileAppender.ActivateOptions();

			var hierarchy = (Hierarchy) LogManager.GetRepository();
			hierarchy.Root.AddAppender(fileAppender);
		}

		private static void LogEnvironment()
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

		private static void InstallExceptionHandlers()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
		}

		private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
		{
			object exception = args.ExceptionObject;

			Log.ErrorFormat("Caught unhandled exception in AppDomain: {0}", exception);
		}
	}
}
