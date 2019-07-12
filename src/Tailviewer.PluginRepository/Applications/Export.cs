using System.Globalization;
using System.IO;
using System.Reflection;
using log4net;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class Export
		: IApplication<ExportOptions>
	{
		public ExitCode Run(IFilesystem filesystem, IInternalPluginRepository repository, ExportOptions options)
		{
			var folder = options.ExportFolder;
			Directory.CreateDirectory(folder);

			using (var file = File.CreateText(Path.Combine(folder, "import.cmd")))
			{
				file.WriteLine("@echo off");
				file.WriteLine("setlocal");
				file.WriteLine();
				file.WriteLine("set SCRIPT_PATH=%~dp0");
				file.WriteLine();
				file.WriteLine("rem Add users");

				foreach (var user in repository.GetAllUsers())
				{
					file.WriteLine("repository.exe add-user \"{0}\" \"{1}\"", user.Username, user.Email);
				}

				file.WriteLine();
				file.WriteLine("rem Add plugins");

				foreach (var plugin in repository.GetAllPlugins())
				{
					var content = repository.DownloadPlugin(plugin.Identifier);
					var pluginFileName = $"{plugin.Identifier.Id}.{plugin.Identifier.Version}.tvp";
					var pluginFilePath = Path.Combine(folder, pluginFileName);
					File.WriteAllBytes(pluginFilePath, content);

					file.WriteLine("repository.exe add-plugin \"%SCRIPT_PATH%\\{0}\" --user \"{1}\" --publish-timestamp {2}", pluginFileName, plugin.Publisher, plugin.PublishTimestamp.ToString(CultureInfo.InvariantCulture));
				}

				file.WriteLine("endlocal");
			}

			return ExitCode.Success;
		}
	}
}