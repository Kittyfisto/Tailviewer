using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Tailviewer.Settings;
using Tailviewer.Ui;
using Tailviewer.Ui.Controls;
using Tailviewer.Ui.ViewModels;
using DataSources = Tailviewer.BusinessLogic.DataSources;
using QuickFilters = Tailviewer.BusinessLogic.QuickFilters;

namespace Tailviewer
{
	public static class App
	{
		[STAThread]
		public static int Main()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
			AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

			var settings = new ApplicationSettings();
			settings.Restore();
			using (var dataSources = new DataSources(settings.DataSources))
			{
				var quickFilters = new QuickFilters(settings.QuickFilters);
				var application = new Application();
				var dispatcher = new UiDispatcher(Dispatcher.CurrentDispatcher);
				Dispatcher.CurrentDispatcher.UnhandledException += DispatcherOnUnhandledException;

				var window = new MainWindow(settings)
					{
						DataContext = new MainWindowViewModel(settings, dataSources, quickFilters, dispatcher)
					};

				settings.MainWindow.RestoreTo(window);

				window.Show();
				return application.Run();
			}
		}

		private static void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
		{
			Exception exception = args.Exception;
			var window = Application.Current.MainWindow;
			if (window != null)
			{
				var model = window.DataContext as MainWindowViewModel;
				if (model != null && exception != null)
				{
					model.HasError = true;
					model.ErrorMessage = exception.Message;
				}
			}

			args.Handled = true;
		}

		private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
		{
			MessageBox.Show(string.Format("Oops, something went wrong:\r\n{0}", args.ExceptionObject),
			                Constants.MainWindowTitle);
		}

		private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
		{
			var name = new AssemblyName(args.Name);
			var fileName = name.Name;

			var resource = string.Format("ThirdParty.{0}.dll", fileName);
			Assembly curAsm = Assembly.GetExecutingAssembly();
			using (Stream stream = curAsm.GetManifestResourceStream(resource))
			{
				if (stream == null)
					return null;

				using (var reader = new StreamReader(stream))
				{
					var data = reader.ReadToEnd();
					return Assembly.Load(data);
				}
			}
		}
	}
}