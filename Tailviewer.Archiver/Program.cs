using System;
using System.IO;
using CommandLine;
using Tailviewer.Archiver.Applications;
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
					(PackOptions options) => new Pack(options).Run(),
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
