using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tailviewer.PluginCreator
{
	internal sealed class Nuget
	{
		public IReadOnlyList<string> Download(string folder, string tailviewerVersion)
		{
			const string framework = "net45";

			var argumentBuilder = new StringBuilder();
			argumentBuilder.AppendFormat("install Tailviewer.Api -Version {0} -Framework {1}", tailviewerVersion, framework);

			var packageFolder = Path.Combine(folder, "packages");
			ClearFolder(packageFolder);

			using (var process = new Process())
			{
				process.StartInfo = new ProcessStartInfo("nuget.exe")
				{
					Arguments = argumentBuilder.ToString(),
					WorkingDirectory = packageFolder,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					UseShellExecute = false
				};
				if (!process.Start())
					throw new NotImplementedException();

				var output = process.StandardOutput.ReadToEnd();
				process.WaitForExit();

				var exitCode = process.ExitCode;
				if (exitCode != 0)
				{
					Console.WriteLine(output);
					throw new Exception($"nuget.exe returned {exitCode}");
				}

				var assemblies = Directory.EnumerateFiles(packageFolder, "*.dll", SearchOption.AllDirectories);
				return assemblies.Where(x => x.Contains(framework)).ToList();
			}
		}

		private static void ClearFolder(string packageFolder)
		{
			if (Directory.Exists(packageFolder))
			{
				var files = Directory.EnumerateFiles(packageFolder, "*", SearchOption.AllDirectories);
				foreach (var file in files)
					File.Delete(file);
				Directory.Delete(packageFolder, true);
				// Deleting is asynchronous, we HAVE to block until it's been deleted...
				while(Directory.Exists(packageFolder))
					Thread.Sleep(TimeSpan.FromMilliseconds(10));
			}

			Directory.CreateDirectory(packageFolder);
		}
	}
}