using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls;
using Tailviewer.Ui.ViewModels;
using DataSources = Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer
{
	public static class App
	{
		[STAThread]
		public static int Main()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
			AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
			ApplicationSettings.Current.Restore();

			using (var dataSources = new DataSources(ApplicationSettings.Current.DataSources))
			{
				var application = new Application();
				var window = new MainWindow
					{
						DataContext = new MainWindowViewModel(dataSources, Dispatcher.CurrentDispatcher)
					};

				ApplicationSettings.Current.MainWindow.RestoreTo(window);

				window.Show();
				return application.Run();
			}
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