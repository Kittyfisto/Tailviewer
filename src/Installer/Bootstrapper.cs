using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Installer.Applications;
using log4net;

namespace Installer
{
	public sealed class Bootstrapper
	{
		private static string _subFolder;

		[STAThread]
		public static int Main(string[] args)
		{
			InstallExceptionHandlers();

			EnableEmbeddedDependencyLoading("InstallationFiles");

			try
			{
				return App.Run(args);
			}
			catch (Exception e)
			{
				Console.WriteLine("An error occured: {0}", e);
				return -1;
			}
		}

		/// <summary>
		///     Allows 3rd party assemblies to be resolved from an embedded resources in the given assembly under
		///     %Assembly%\%subfolder%\
		/// </summary>
		/// <param name="subFolder"></param>
		private static void EnableEmbeddedDependencyLoading(string subFolder)
		{
			_subFolder = subFolder;

			AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
		}

		private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
		{
			string name = args.Name;

			var assemblyName = new AssemblyName(name);
			string fileName = assemblyName.Name;

			string resource = string.Format("{0}\\{1}.dll", _subFolder, fileName);
			Assembly curAsm = Assembly.GetExecutingAssembly();

			using (Stream stream = curAsm.GetManifestResourceStream(resource))
			{
				if (stream == null)
					return null;

				byte[] data = ReadFully(stream);
				return Assembly.Load(data);
			}
		}

		private static byte[] ReadFully(Stream input)
		{
			var buffer = new byte[16*1024];
			using (var ms = new MemoryStream())
			{
				int read;
				while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}

		private static void InstallExceptionHandlers()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
		}

		private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
		{
			object exception = args.ExceptionObject;

			LogException(exception);

			MessageBox.Show(string.Format("Oops, something went wrong:\r\n{0}", exception),
			                Constants.MainWindowTitle);
		}

		private static void LogException(object exception)
		{
			ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
			log.ErrorFormat("Caught unexpected exception: {0}", exception);
		}
	}
}