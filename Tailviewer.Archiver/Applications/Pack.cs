using System;
using System.IO;
using System.Reflection;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Archiver.Applications
{
	sealed class Pack
		: IDisposable
	{
		private readonly PackOptions _options;

		public Pack(PackOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			_options = options;

			// We want to allow any archive executable to pack plugins and therefore we may need to resolve
			// a very different Tailviewer.Api assembly than we were built against.
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
		}

		public int Run()
		{
			var archiveFilename = _options.ArchiveFileName ?? Path.GetFileNameWithoutExtension(_options.InputFileName);
			if (!archiveFilename.EndsWith(PluginArchive.PluginExtension, StringComparison.InvariantCultureIgnoreCase))
				archiveFilename = string.Format("{0}.{1}", archiveFilename, PluginArchive.PluginExtension);
			if (!Path.IsPathRooted(archiveFilename))
				archiveFilename = Path.Combine(Directory.GetCurrentDirectory(), archiveFilename);

			Console.WriteLine("Creating Tailviewer plugin...");

			using (var pluginStream = new MemoryStream())
			{
				using (var packer = PluginPacker.Create(pluginStream, leaveOpen: true))
				{
					var extension = Path.GetExtension(_options.InputFileName)?.ToLowerInvariant();
					switch (extension)
					{
						case ".sln":
						case ".csproj":
							Console.WriteLine("ERROR: Not implemented yet");
							return -1;

						case ".dll":
							Console.Write("Adding {0}... ", _options.InputFileName);
							packer.AddPluginAssembly(_options.InputFileName);
							Console.WriteLine("OK");
							break;

						default:
							Console.WriteLine("ERROR: Input file must be either a Visual Studio Solution, C# Project or .NET Assembly");
							return -1;
					}

					foreach (var filename in _options.Files)
					{
						Console.Write("Adding {0}... ", filename);
						var fullFilename = Path.IsPathRooted(filename)
							? filename
							: Path.Combine(Directory.GetCurrentDirectory(), filename);
						AddFile(packer, fullFilename);
						Console.WriteLine("OK");
					}
				}

				pluginStream.Position = 0;
				Console.Write("Saving plugin => {0}... ", archiveFilename);
				using (var fileStream = File.Create(archiveFilename))
				{
					pluginStream.CopyTo(fileStream);
				}
				Console.WriteLine("OK");
			}

			return 0;
		}

		private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var assemblyName = new AssemblyName(args.Name);
			var assemblyDirectory = Path.GetDirectoryName(_options.InputFileName);
			if (string.IsNullOrEmpty(assemblyDirectory))
				assemblyDirectory = Directory.GetCurrentDirectory();

			var assemblyFileName = Path.Combine(assemblyDirectory, string.Format("{0}.dll", assemblyName.Name));
			if (File.Exists(assemblyFileName))
			{
				var assembly = Assembly.LoadFrom(assemblyFileName);
				return assembly;
			}

			return null;
		}

		private static void AddFile(PluginPacker packer, string filename)
		{
			var entryName = Path.GetFileName(filename);
			using (var stream = File.OpenRead(filename))
			{
				packer.AddFile(entryName, stream);
			}
		}

		public void Dispose()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
		}
	}
}