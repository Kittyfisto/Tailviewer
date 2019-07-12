using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Archiver.Applications
{
	sealed class Pack
		: IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly PackOptions _options;
		private readonly ConsoleLogger _consoleAppender;

		public Pack(PackOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			_options = options;

			// We want to allow any archive executable to pack plugins and therefore we may need to resolve
			// a very different Tailviewer.Api assembly than we were built against.
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

			// Depending on the options, some messages should be visible to the user (i.e. written to the console).
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			_consoleAppender = new ConsoleLogger();
			hierarchy.Root.AddAppender(_consoleAppender);
			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}

		public int Run()
		{
			try
			{
				Log.Info("Creating Tailviewer plugin...");

				using (var pluginStream = new MemoryStream())
				{
					string pluginVersion;
					using (var packer = PluginPacker.Create(pluginStream, leaveOpen: true))
					{
						var extension = Path.GetExtension(_options.InputFileName)?.ToLowerInvariant();
						switch (extension)
						{
							case ".sln":
							case ".csproj":
								Log.Error("Not implemented yet");
								return -1;

							case ".dll":
								Log.InfoFormat("Adding {0}... ", _options.InputFileName);
								packer.AddPluginAssembly(_options.InputFileName);
								Log.Info("OK");
								break;

							default:
								Console.WriteLine("ERROR: Input file must be either a Visual Studio Solution, C# Project or .NET Assembly");
								return -1;
						}

						foreach (var filename in _options.Files)
						{
							Log.InfoFormat("Adding {0}... ", filename);
							var fullFilename = Path.IsPathRooted(filename)
								? filename
								: Path.Combine(Directory.GetCurrentDirectory(), filename);
							AddFile(packer, fullFilename);
							Log.Info("OK");
						}

						if (!string.IsNullOrEmpty(_options.IconFileName))
						{
							using (var stream = File.OpenRead(_options.IconFileName))
							{
								packer.SetIcon(stream);
							}
						}

						if (!string.IsNullOrEmpty(_options.ChangeListFileName))
						{
							packer.SetChanges(_options.ChangeListFileName);
						}

						pluginVersion = packer.Version;
					}

					var archiveFilename = Path.Combine(Directory.GetCurrentDirectory(), string.Format("{0}.{1}.{2}",
						Path.GetFileNameWithoutExtension(_options.InputFileName),
						pluginVersion,
						PluginArchive.PluginExtension));

					pluginStream.Position = 0;
					Log.InfoFormat("Saving plugin => {0}... ", archiveFilename);
					using (var fileStream = File.Create(archiveFilename))
					{
						pluginStream.CopyTo(fileStream);
					}
					Log.Info("OK");
				}

				return 0;
			}
			catch (PackException e)
			{
				Log.Error(e.Message);
				return -1;
			}
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
			var hierarchy = (Hierarchy)LogManager.GetRepository();
			hierarchy.Root.RemoveAppender(_consoleAppender);
			AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
		}
	}
}