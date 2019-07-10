using System;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using log4net;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class RunServer
		: IApplication<RunServerOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ManualResetEvent BeginShutdownEvent = new ManualResetEvent(false);
		private static readonly ManualResetEvent FinishedShutdownEvent = new ManualResetEvent(false);

		[DllImport("Kernel32", SetLastError=true)]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool add);

		public delegate bool HandlerRoutine(CtrlTypes CtrlType);

		public enum CtrlTypes 
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2, 
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		private static bool ConsoleCtrlCheck(CtrlTypes ctrlType) 
		{
			switch (ctrlType)
			{
				case CtrlTypes.CTRL_C_EVENT:
				case CtrlTypes.CTRL_CLOSE_EVENT:
				case CtrlTypes.CTRL_LOGOFF_EVENT:
				case CtrlTypes.CTRL_SHUTDOWN_EVENT:
					Log.InfoFormat("Beginning shutdown due to {0}...", ctrlType);
					BeginShutdownEvent.Set();

					Log.InfoFormat("Waiting for shutdown...");
					FinishedShutdownEvent.WaitOne();

					Log.InfoFormat("Done, goodbye!");
					break;

				default:
					Log.InfoFormat("Ignoring {0} event", ctrlType);
					break;
			}

			return true;
		}

		public int Run(PluginRepository repository, RunServerOptions options)
		{
			try
			{
				if (!SetConsoleCtrlHandler(ConsoleCtrlCheck, add: true))
				{
					var error = Marshal.GetLastWin32Error();
					Log.InfoFormat("Could not add console ctrl handler, GetLastError()={0}", error);
				}

				Log.InfoFormat("The repository contains {0} plugin(s)", repository.CountPlugins());

				using (new Server(new IPEndPoint(IPAddress.Any, 1234), repository))
				{
					Console.WriteLine("Press ctrl+c to exit the application...");

					BeginShutdownEvent.WaitOne();

					return 0;
				}
			}
			finally
			{
				FinishedShutdownEvent.Set();
			}
		}
	}
}
