using System;
using System.Reflection;
using System.ServiceProcess;
using log4net;
using Tailviewer.PluginRepository.Applications;
using Tailviewer.PluginRepository.Configuration;

namespace Tailviewer.PluginRepository.Service
{
	public partial class Service1 : ServiceBase
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private Server _server;
		private PluginRepository _repository;

		public Service1()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			Log.DebugFormat("Starting...");

			try
			{
				var configuration = ServerConfiguration.ReadOrCreate(Constants.ApplicationConfigurationFile);

				_repository = PluginRepository.Create();
				var repositoryProxy = new PluginRepositoryProxy(_repository, configuration);
				var ep = IPEndPointExtensions.Parse(configuration.Address);
				_server = new Server(ep, repositoryProxy);

				Log.InfoFormat("Started");
			}
			catch(Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while trying to start: {0}", e);

				throw;
			}
		}

		protected override void OnStop()
		{
			Log.DebugFormat("Starting...");

			try
			{
				_server?.Dispose();
				_repository?.Dispose();

				Log.InfoFormat("Stopped");
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);

				throw;
			}
		}
	}
}
