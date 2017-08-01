using System;
using System.IO;
using CommandLine;
using Tailviewer.Archiver.Plugins;

namespace Tailviewer.Archiver
{
	/// <summary>
	///     Entry point to the application.
	/// </summary>
	/// <remarks>
	///     Usage: archive.exe pack MyPlugin.dll
	/// </remarks>
	static class Program
	{
		public static int Run(string[] args)
		{
			var result = Parser.Default.ParseArguments<PackOptions, ListOptions>(args);
			try
			{
				result.MapResult(
					(PackOptions options) => Pack(options),
					(ListOptions options) => List(options),
					_ => MakeError());

				return 0;
			}
			catch (IOException e)
			{
				Console.WriteLine("ERROR: {0}", e.Message);
				return -1;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return -1;
			}
		}

		private static object Pack(PackOptions opts)
		{
			var archiveFilename = opts.ArchiveFileName ?? Path.GetFileNameWithoutExtension(opts.PluginFileName);
			var extension = ".tva";
			if (!archiveFilename.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
				archiveFilename += extension;

			Console.WriteLine("Creating plugin archive {0}...", archiveFilename);

			using (var packer = PluginPacker.Create(archiveFilename))
			{
				Console.Write("Adding plugin assembly {0}... ", opts.PluginFileName);
				packer.AddPluginAssembly(opts.PluginFileName);
				Console.WriteLine("OK");

				foreach (var filename in opts.Files)
				{
					Console.Write("Adding file {0}... ", filename);
					var entryName = Path.GetFileName(filename);
					if (filename.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
					{
						packer.AddAssembly(entryName, filename);
					}
					else
					{
						using (var stream = File.OpenRead(entryName))
						{
							packer.AddFile(entryName, stream);
						}
					}
					Console.WriteLine("OK");
				}
			}

			Console.WriteLine("Finished!");

			return null;
		}

		private static object List(ListOptions opts)
		{
			return null;
		}

		private static Tuple<string, string> MakeError()
		{
			return Tuple.Create("\0", "\0");
		}
	}
}
