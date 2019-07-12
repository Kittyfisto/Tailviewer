using System;
using System.IO;
using CommandLine;
using Tailviewer.Archiver.Applications;

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
			var result = Parser.Default.ParseArguments<PackOptions, ListOptions, PublishOptions>(args);
			try
			{
				return result.MapResult(
				                        (PackOptions options) => new Pack(options).Run(),
				                        (ListOptions options) => List(options),
				                        (PublishOptions options) => (int)new Publish().Run(options),
				                        _ => -2);
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
		
		private static int List(ListOptions opts)
		{
			return 0;
		}
	}
}
