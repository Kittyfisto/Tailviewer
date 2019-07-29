using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
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
			var configurationFile = GetConfigurationFilePath();
			var configuration = TryReadConfiguration(configurationFile);

			_repository = PluginRepository.Create();
			var repositoryProxy = new PluginRepositoryProxy(_repository, configuration);
			var ep = IPEndPointExtensions.Parse(configuration.Address);
			_server = new Server(ep, repositoryProxy);
		}

		private static ServerConfiguration TryReadConfiguration(string configurationFile)
		{
			try
			{
				return ServerConfiguration.Read(configurationFile);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to read configuration from '{0}':\r\n{1}", configurationFile, e);
				return new ServerConfiguration();
			}
		}

		protected override void OnStop()
		{
			_server?.Dispose();
			_repository?.Dispose();
		}

		private static string GetConfigurationFilePath()
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			return Path.Combine(Path.GetDirectoryName(path), "configuration.xml");
		}
	}
}
