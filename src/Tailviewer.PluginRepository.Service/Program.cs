using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Tailviewer.PluginRepository.Service
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			Logging.SetupFileAppender(Constants.ServiceLogFile);
			Logging.InstallExceptionHandlers();

			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new Service1()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
