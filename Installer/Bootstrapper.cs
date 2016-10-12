using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Installer.Applications.Install;
using Installer.Applications.SilentInstall;
using Installer.Applications.Update;

namespace Installer
{
	public sealed class Bootstrapper
	{
		private static string _containingAssembly;
		private static string _subFolder;

		[STAThread]
		public static int Main(string[] args)
		{
			InstallExceptionHandlers();

			EnableEmbeddedDependencyLoading("Installer", "InstallationFiles");

			return Run(args);
		}

		private static int Run(string[] args)
		{
			var arguments = Arguments.Parse(args);
			switch (arguments.Mode)
			{
				case Mode.Install:
					return InstallApplication.Run(arguments);

				case Mode.SilentInstall:
					return SilentInstallApplication.Run(arguments);

				case Mode.Update:
					return UpdateApplication.Run(arguments);

				default:
					return -1;
			}
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