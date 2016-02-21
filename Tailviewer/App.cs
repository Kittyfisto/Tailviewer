using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Tailviewer.BusinessLogic.AutoUpdates;
using Tailviewer.Settings;
using Tailviewer.Ui;
using Tailviewer.Ui.Controls;
using Tailviewer.Ui.ViewModels;
using log4net;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.Filters.QuickFilters;

namespace Tailviewer
{
	public class App
		: Application
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static int Start()
		{
			InstallExceptionHandlers();
			Log.InfoFormat("Starting tailviewer...");
			LogEnvironment();

			ApplicationSettings settings = ApplicationSettings.Create();
			bool neededPatching;
			settings.Restore(out neededPatching);

			if (neededPatching)
			{
				// TODO: Save settings right again to complete the upgrade
				//       (maybe we should preserve an old version)
			}

			using (var dataSources = new DataSources(settings.DataSources))
			using (var updater = new AutoUpdater(settings.AutoUpdate))
			{
				var quickFilters = new QuickFilters(settings.QuickFilters);
				var application = new App();
				Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
				var uiDispatcher = new UiDispatcher(dispatcher);
				dispatcher.UnhandledException += DispatcherOnUnhandledException;

				var window = new MainWindow(settings)
					{
						DataContext = new MainWindowViewModel(settings,
						                                      dataSources,
						                                      quickFilters,
						                                      updater,
						                                      uiDispatcher)
					};

				settings.MainWindow.RestoreTo(window);

				window.Show();
				return application.Run();
			}
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

			Log.InfoFormat("Environment: {0}", builder);
		}

		private static void InstallExceptionHandlers()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
		}

		private static void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
		{
			Exception exception = args.Exception;

			Log.ErrorFormat("Caught unexpected exception on dispatcher: {0}", exception);

			Window window = Current.MainWindow;
			if (window != null)
			{
				var model = window.DataContext as MainWindowViewModel;
				if (model != null && exception != null)
				{
					model.HasError = true;
					model.Exception = exception;
				}
			}

			args.Handled = true;
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