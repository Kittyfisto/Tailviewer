using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Metrolib;

namespace Installer
{
	public sealed class Bootstrapper
	{
		private static string _containingAssembly;
		private static string _subFolder;

		[STAThread]
		public static int Main()
		{
			InstallExceptionHandlers();

			AssemblyName name = Assembly.GetExecutingAssembly().GetName();
			EnableEmbeddedDependencyLoading(name.Name, "InstallationFiles");

			return Run();
		}

		private static int Run()
		{
			var app = new Application();
			var dispatcher = new UiDispatcher(Dispatcher.CurrentDispatcher);
			var window = new MainWindow(new MainWindowViewModel(dispatcher));
			window.Show();
			return app.Run();
		}

		/// <summary>
		///     Allows 3rd party assemblies to be resolved from an embedded resources in the given assembly under
		///     %Assembly%\%subfolder%\
		/// </summary>
		/// <param name="containingAssembly"></param>
		/// <param name="subFolder"></param>
		private static void EnableEmbeddedDependencyLoading(string containingAssembly, string subFolder)
		{
			_containingAssembly = containingAssembly;
			_subFolder = subFolder;

			AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
		}

		private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
		{
			string name = args.Name;

			var assemblyName = new AssemblyName(name);
			string fileName = assemblyName.Name;

			string resource = string.Format("{0}.{1}.{2}.dll", _containingAssembly, _subFolder, fileName);
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

			MessageBox.Show(string.Format("Oops, something went wrong:\r\n{0}", exception),
			                Constants.MainWindowTitle);
		}
	}
}