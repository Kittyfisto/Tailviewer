using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Metrolib;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.Ui.Controls;
using Tailviewer.Ui.ViewModels;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.Analysis;
using Tailviewer.Count.BusinessLogic;
using Tailviewer.Count.Ui;
using Tailviewer.Events.BusinessLogic;
using Tailviewer.QuickInfo.BusinessLogic;
using Tailviewer.QuickInfo.Ui;
using Tailviewer.Ui.Controls.MainPanel.Analyse.Widgets.Help;
using ApplicationSettings = Tailviewer.Settings.ApplicationSettings;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer
{
	public class App
		: Application
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public App()
		{
			Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Metrolib;component/Themes/Generic.xaml") });
		}

		public static int Start(SingleApplicationHelper.IMutex mutex, string[] args)
		{
			try
			{
				return StartInternal(mutex, args);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return -1;
			}
		}

		private static int StartInternal(SingleApplicationHelper.IMutex mutex, string[] args)
		{
			InstallExceptionHandlers();
			Log.InfoFormat("Starting tailviewer...");
			Log.InfoFormat("Commandline arguments: {0}", string.Join(" ", args));
			LogEnvironment();

			ApplicationSettings settings = ApplicationSettings.Create();
			settings.Restore(out var neededPatching);

			if (neededPatching)
			{
				// TODO: Save settings right again to complete the upgrade
				//       (maybe we should preserve an old version)
			}

			var actionCenter = new ActionCenter();
			using (var taskScheduler = new DefaultTaskScheduler())
			using (var serialTaskScheduler = new SerialTaskScheduler())
			using (var pluginScanner = new PluginArchiveLoader(Constants.PluginPath))
			{
				var pluginCache = CreatePluginSystem(pluginScanner);

				var fileFormatPlugins = pluginCache.LoadAllOfType<IFileFormatPlugin>();
				var filesystem = new Filesystem(serialTaskScheduler);
				var plugins = LoadPlugins();

				var logFileFactory = new PluginLogFileFactory(taskScheduler, fileFormatPlugins);
				using (var dataSources = new DataSources(logFileFactory, taskScheduler, settings.DataSources))
				using (var updater = new AutoUpdater(actionCenter, settings.AutoUpdate))
				using (var logAnalyserEngine = new LogAnalyserEngine(taskScheduler))
				using (var analysisStorage = new AnalysisStorage(taskScheduler, filesystem, logAnalyserEngine, CreateTypeFactory(plugins)))
				{
					foreach (var plugin in plugins)
					{
						logAnalyserEngine.RegisterFactory(plugin);
					}

					var arguments = ArgumentParser.TryParse(args);
					if (arguments.FileToOpen != null)
					{
						if (File.Exists(arguments.FileToOpen))
						{
							// Not only do we want to add this file to the list of data sources,
							// but we also want to select it so the user can view it immediately, regardless
							// of what was selected previously.
							var dataSource = dataSources.AddDataSource(arguments.FileToOpen);
							settings.DataSources.SelectedItem = dataSource.Id;
						}
						else
						{
							Log.ErrorFormat("File '{0}' does not exist, won't open it!", arguments.FileToOpen);
						}
					}

					if (settings.AutoUpdate.CheckForUpdates)
					{
						// Our initial check for updates is not due to a user action
						// and therefore we don't need to show a notification when the
						// application is up-to-date.
						updater.CheckForUpdates(addNotificationWhenUpToDate: false);
					}

					var quickFilters = new QuickFilters(settings.QuickFilters);
					actionCenter.Add(Build.Current);
					actionCenter.Add(Change.Merge(Changelog.MostRecentPatches));
					var application = new App();
					Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
					var uiDispatcher = new UiDispatcher(dispatcher);
					dispatcher.UnhandledException += actionCenter.ReportUnhandledException;
					TaskScheduler.UnobservedTaskException += actionCenter.ReportUnhandledException;

					var window = new MainWindow(settings)
					{
						DataContext = new MainWindowViewModel(settings,
						                                      dataSources,
						                                      quickFilters,
						                                      actionCenter,
						                                      updater,
						                                      taskScheduler,
						                                      analysisStorage,
						                                      uiDispatcher,
						                                      pluginCache)
					};

					settings.MainWindow.RestoreTo(window);

					window.Show();
					mutex.SetListener(window);

					return application.Run();
				}
			}
		}

		private static IPluginLoader CreatePluginSystem(PluginArchiveLoader pluginScanner)
		{
			// Currently, we deploy some well known "plugins" via the installer and they're
			// not available as *.tvp files just yet (which means the PluginArchiveLoader won't find them).
			// Therefore we register those at a PluginRegistry.
			var wellKnownPlugins = LoadWellKnownPlugins();

			// Even though we're dealing with the limitation above, the rest of the application should not need
			// to care, which is why we make both of those types of plugin accessible from one loader
			var pluginLoader = new AggregatedPluginLoader(pluginScanner, wellKnownPlugins);

			// Last but not least, the PluginArchiveLoader doesn't cache anything which means
			// that multiple Load requests would result in the same plugin being loaded many times.
			// we don't want that (unnecessary work, waste of CPU time, etc..), so that's why there's a cache.
			var pluginCache = new PluginCache(pluginLoader);
			return pluginCache;
		}

		private static IPluginLoader LoadWellKnownPlugins()
		{
			var registry = new PluginRegistry();
			//registry.Register(new EventsLogAnalyserPlugin());
			registry.Register(new HelpWidgetPlugin());
			registry.Register(new QuickInfoAnalyserPlugin(), new QuickInfoWidgetPlugin());
			registry.Register(new LogEntryCountAnalyserPlugin(), new LogEntryCountWidgetPlugin());
			return registry;
		}

		private static ITypeFactory CreateTypeFactory(IEnumerable<ILogAnalyserPlugin> plugins)
		{
			var factory = new TypeFactory();
			foreach (var plugin in plugins)
			{
				foreach (var pair in plugin.SerializableTypes)
				{
					try
					{
						factory.Add(pair.Key, pair.Value);
					}
					catch (Exception e)
					{
						Log.ErrorFormat("Caught unexpected exception: {0}", e);
					}
				}
			}
			factory.Add(typeof(ActiveAnalysisConfiguration));
			factory.Add(typeof(AnalyserTemplate));
			factory.Add(typeof(PageTemplate));
			factory.Add(typeof(WidgetTemplate));
			return factory;
		}

		private static IReadOnlyList<ILogAnalyserPlugin> LoadPlugins()
		{
			return new ILogAnalyserPlugin[]
			{
				new LogEntryCountAnalyserPlugin(),
				new QuickInfoAnalyserPlugin(),
				new EventsLogAnalyserPlugin()
			};
		}

		private static void LogEnvironment()
		{
			var builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendFormat("Tailviewer: v{0}, {1}\r\n",
			                     FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion,
			                     Environment.Is64BitProcess ? "64bit" : "32bit");
			builder.AppendFormat(".NET Environment: {0}\r\n", Environment.Version);
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

			MessageBox.Show(string.Format("Oops, something went wrong:\r\n{0}", exception),
			                Constants.MainWindowTitle);
		}
	}
}