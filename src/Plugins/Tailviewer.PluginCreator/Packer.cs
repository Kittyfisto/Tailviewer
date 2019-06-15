using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Tailviewer.PluginCreator
{
	internal sealed class Packer
	{
		public string Pack(string folder, string assembly, string pluginName)
		{
			DeleteOldPlugins(folder);

			var argumentBuilder = new StringBuilder();
			argumentBuilder.AppendFormat("pack \"{0}\"", assembly);

			string archiverPath =
				Directory.EnumerateFiles(folder, "archive.exe", SearchOption.AllDirectories).ToList()[0];

			using (var process = new Process())
			{
				process.StartInfo = new ProcessStartInfo(archiverPath)
				{
					Arguments = argumentBuilder.ToString(),
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					UseShellExecute = false,
					WorkingDirectory = folder
				};

				if (!process.Start())
					throw new NotImplementedException();

				var output = process.StandardOutput.ReadToEnd();
				var exitCode = process.ExitCode;
				if (exitCode != 0)
				{
					Console.WriteLine(output);
					throw new Exception($"archiver returned {exitCode}");
				}
			}

			var pluginFiles = Directory.EnumerateFiles(folder, "*.tvp", SearchOption.AllDirectories).ToList();
			if (pluginFiles.Count == 0)
				throw new NotImplementedException();
			if (pluginFiles.Count > 1)
				throw new NotImplementedException();

			return pluginFiles[0];

		}

		private void DeleteOldPlugins(string folder)
		{
			var pluginFiles = Directory.EnumerateFiles(folder, "*.tvp", SearchOption.AllDirectories);
			foreach(var file in pluginFiles)
				File.Delete(file);
		}
	}
}