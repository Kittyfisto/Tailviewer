using System.Windows;
using Metrolib;

namespace Installer.Applications.Update
{
	public sealed class UpdateApplication
		: Application
	{
		public static int Run(Arguments args)
		{
			var app = new UpdateApplication();
			var dispatcher = new UiDispatcher(System.Windows.Threading.Dispatcher.CurrentDispatcher);
			var window = new UpdaterWindow(new UpdateWindowViewModel(dispatcher));
			window.Show();
			return app.Run();
		}
	}
}