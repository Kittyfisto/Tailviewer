using System;
using System.IO;
using System.Reflection;

namespace Tailviewer.Archiver
{
	public static class Bootstrapper
	{
		private static string _containingAssembly;
		private static string _subFolder;

		/// <summary>
		/// </summary>
		/// <remarks>
		///     Dependencies won't be loaded until a method depending on them is first called.
		///     Therefore we have to ensure that this method does NOT have ANY external dependencies.
		/// </remarks>
		/// <param name="args"></param>
		/// <returns></returns>
		private static int Main(string[] args)
		{
			try
			{
				EnableEmbeddedDependencyLoading("Tailviewer.Archiver", "Resources");

				return Program.Run(args);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
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
			var name = args.Name;

			var requiredAssemblyName = new AssemblyName(name);
			var fileName = requiredAssemblyName.Name;

			var resource = string.Format("{0}.{1}.{2}.dll", _containingAssembly, _subFolder, fileName);
			var curAsm = Assembly.GetExecutingAssembly();

			using (var stream = curAsm.GetManifestResourceStream(resource))
			{
				if (stream == null)
					return null;

				byte[] data = ReadFully(stream);
				var assembly =  Assembly.Load(data);
				var actualAssemblyName = assembly.GetName();
				if (!IsEqual(actualAssemblyName, requiredAssemblyName))
				{
					return null;
				}

				return assembly;
			}
		}

		private static bool IsEqual(AssemblyName actualAssemblyName, AssemblyName assemblyName)
		{
			if (!Equals(actualAssemblyName.FullName, assemblyName.FullName))
			{
				return false;
			}

			return true;
		}

		private static byte[] ReadFully(Stream input)
		{
			var buffer = new byte[16 * 1024];
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
	}
}