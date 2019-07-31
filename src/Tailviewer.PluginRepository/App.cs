using System;
using System.IO;
using System.Reflection;
using System.Threading;
using CommandLine;
using log4net;
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
				Logging.InstallExceptionHandlers();
				Log.InfoFormat("Starting {0}...", Constants.ApplicationTitle);
				Log.InfoFormat("Commandline arguments: {0}", string.Join(" ", args));
				Logging.LogEnvironment();

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
			Logging.SetupConsoleLogger(logTimestamps);

			if (logToFile)
				Logging.SetupFileAppender(Constants.ApplicationLogFile);

			using (var taskScheduler = new DefaultTaskScheduler())
			{
				var filesystem = new Filesystem(taskScheduler);
				var app = new TApp();

				if (app.RequiresRepository)
				{
					using (var repo = PluginRepository.Create(app.ReadOnlyRepository))
					{
						return (int)app.Run(filesystem, repo, options);
					}
				}

				return (int)app.Run(filesystem, null, options);
			}
		}
	}
}
