using System.Reflection;
using System.Windows;
using Metrolib;
using log4net;

namespace Installer.Applications.Update
{
	public sealed class UpdateApplication
		: Application
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static int Run(Arguments args)
		{
			Log.InfoFormat("Starting update of v{0} to {1}",
						   Constants.ApplicationVersion,
						   args.InstallationPath);

			var app = new UpdateApplication();
			var dispatcher = new UiDispatcher(System.Windows.Threading.Dispatcher.CurrentDispatcher);
			var window = new UpdaterWindow(new UpdateWindowViewModel(dispatcher, args.InstallationPath));
			window.Show();
			return app.Run();
		}
	}
}