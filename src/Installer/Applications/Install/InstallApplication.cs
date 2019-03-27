using System.Windows;
using Metrolib;

namespace Installer.Applications.Install
{
	public sealed class InstallApplication
		: Application
	{
		public static int Run(Arguments args)
		{
			var app = new InstallApplication();
			var dispatcher = new UiDispatcher(System.Windows.Threading.Dispatcher.CurrentDispatcher);
			var window = new MainWindow(new MainWindowViewModel(dispatcher));
			window.Show();
			return app.Run();
		}
	}
}