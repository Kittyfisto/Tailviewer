using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Serialization;
using log4net;
using Tailviewer.PluginRepository.Configuration;

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
					Log.DebugFormat("Beginning shutdown due to {0}...", ctrlType);
					BeginShutdownEvent.Set();

					Log.InfoFormat("Shutting down...");
					FinishedShutdownEvent.WaitOne();

					Log.InfoFormat("Done, goodbye!");
					break;

				default:
					Log.InfoFormat("Ignoring {0} event", ctrlType);
					break;
			}

			return true;
		}

		public bool RequiresRepository => true;

		public ExitCode Run(IFilesystem filesystem, IInternalPluginRepository repository, RunServerOptions options)
		{
			try
			{
				ServerConfiguration configuration;
				if (!string.IsNullOrEmpty(options.Configuration))
				{
					Log.InfoFormat("Using configuration file '{0}'", options.Configuration);
					configuration = ReadConfiguration(options.Configuration);
				}
				else
				{
					Log.InfoFormat("No configuration file specified, using hardcoded default values instead");
					configuration = new ServerConfiguration();
				}

				if (options.AllowRemotePublish != null)
					configuration.Publishing.AllowRemotePublish = options.AllowRemotePublish.Value;
				Log.InfoFormat("Remote publishing is {0}", configuration.Publishing.AllowRemotePublish ? "allowed" : "not allowed");

				if (!SetConsoleCtrlHandler(ConsoleCtrlCheck, add: true))
				{
					var error = Marshal.GetLastWin32Error();
					Log.WarnFormat("Could not add console ctrl handler, GetLastError()={0}", error);
				}

				Log.InfoFormat("The repository contains {0} plugin(s)", repository.CountPlugins());
				Log.InfoFormat("Starting server...");

				var repositoryProxy = new PluginRepositoryProxy(repository, configuration);
				if (!TryParseEndPoint(configuration.Address, out var ep))
					return ExitCode.InvalidAddress;

				using (new Server(ep, repositoryProxy))
				{
					Console.WriteLine("Press ctrl+c to exit the application...");

					BeginShutdownEvent.WaitOne();

					return ExitCode.Success;
				}
			}
			finally
			{
				FinishedShutdownEvent.Set();
			}
		}

		private bool TryParseEndPoint(string address, out IPEndPoint endPoint)
		{
			try
			{
				endPoint = IPEndPointExtensions.Parse(address);
				return true;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("{0} is not a valid address", address);
				Log.Debug(e);

				endPoint = null;
				return false;
			}
		}

		private ServerConfiguration ReadConfiguration(string fileName)
		{
			var serializer = new XmlSerializer(typeof(ServerConfiguration));
			serializer.UnknownNode+= UnknownNode;
			serializer.UnknownAttribute+= UnknownAttribute;

			using (var filestream = File.OpenRead(fileName))
			{
				try
				{
					return (ServerConfiguration)serializer.Deserialize(filestream);
				}
				catch (Exception e)
				{
					Log.WarnFormat("Unable to read configuration '{0}': {1}", fileName, e.Message);
					return new ServerConfiguration();
				}
			}
		}

		private static void UnknownNode(object sender, XmlNodeEventArgs e)
		{
			Log.WarnFormat("Unknown Node:{0}\t{1}", e.Name, e.Text);
		}

		private static void UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			System.Xml.XmlAttribute attr = e.Attr;
			Log.WarnFormat("Unknown attribute: {0}={1}", attr.Name, attr.Value);
		}
	}
}
