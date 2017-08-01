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
			var archiveFilename = opts.ArchiveFileName ?? Path.GetFileNameWithoutExtension(opts.InputFileName);
			if (!archiveFilename.EndsWith(PluginArchive.PluginExtension, StringComparison.InvariantCultureIgnoreCase))
				archiveFilename = string.Format("{0}.{1}", archiveFilename, PluginArchive.PluginExtension);

			Console.WriteLine("Creating plugin archive {0}...", archiveFilename);

			using (var packer = PluginPacker.Create(archiveFilename))
			{
				var extension = Path.GetExtension(opts.InputFileName)?.ToLowerInvariant();
				switch (extension)
				{
					case ".sln":
					case ".csproj":
						Console.WriteLine("ERROR: Not implemented yet");
						return -1;

					case ".dll":
						Console.Write("Adding plugin assembly {0}... ", opts.InputFileName);
						packer.AddPluginAssembly(opts.InputFileName);
						Console.WriteLine("OK");
						break;

					default:
						Console.WriteLine("ERROR: Input file must be either a Visual Studio Solution, C# Project or .NET Assembly");
						return -1;
				}

				foreach (var filename in opts.Files)
				{
					Console.Write("Adding file {0}... ", filename);
					AddFile(packer, filename);
					Console.WriteLine("OK");
				}
			}

			Console.WriteLine("Finished!");

			return null;
		}

		private static void AddFile(PluginPacker packer, string filename)
		{
			var entryName = Path.GetFileName(filename);
			using (var stream = File.OpenRead(entryName))
			{
				packer.AddFile(entryName, stream);
			}
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
