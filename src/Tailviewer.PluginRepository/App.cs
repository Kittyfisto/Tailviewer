using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using CommandLine;
using log4net;
using Tailviewer.PluginRegistry.Applications;

namespace Tailviewer.PluginRegistry
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

				var result = Parser.Default.ParseArguments<RunServerOptions, AddPluginOptions, ListPluginsOptions>(args);
				return result.MapResult(
				                        (RunServerOptions options) => RunServer.Run(),
				                        (AddPluginOptions options) => AddPlugin.Run(options),
				                        (ListPluginsOptions options) => ListPlugins.Run(options),
				                        _ => -2);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Exiting due to unexpected exception: {0}", e);
				return -1;
			}
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
