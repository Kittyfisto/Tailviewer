using System.Globalization;
using System.IO;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class Export
		: IApplication<ExportOptions>
	{
		public bool RequiresRepository => true;

		public bool ReadOnlyRepository => true;

		public ExitCode Run(IFilesystem filesystem, IInternalPluginRepository repository, ExportOptions options)
		{
			var folder = options.ExportFolder;
			Directory.CreateDirectory(folder);

			using (var file = filesystem.CreateFile(Path.Combine(folder, "import.cmd")))
			using (var writer = new StreamWriter(file))
			{
				writer.WriteLine("@echo off");
				writer.WriteLine("setlocal");
				writer.WriteLine();
				writer.WriteLine("set SCRIPT_PATH=%~dp0");
				writer.WriteLine();
				writer.WriteLine("rem Add users");

				foreach (var user in repository.GetAllUsers())
				{
					writer.WriteLine("repository.exe add-user \"{0}\" \"{1}\" --access-token \"{2}\"", user.Username, user.Email, user.AccessToken);
				}

				writer.WriteLine();
				writer.WriteLine("rem Add plugins");

				foreach (var plugin in repository.GetAllPlugins())
				{
					var content = repository.DownloadPlugin(plugin.Identifier);
					var pluginFileName = $"{plugin.Identifier.Id}.{plugin.Identifier.Version}.tvp";
					var pluginFilePath = Path.Combine(folder, pluginFileName);
					File.WriteAllBytes(pluginFilePath, content);

					writer.WriteLine("repository.exe add-plugin \"%SCRIPT_PATH%\\{0}\" --user \"{1}\" --publish-timestamp {2}", pluginFileName, plugin.Publisher, plugin.PublishTimestamp.ToString(CultureInfo.InvariantCulture));
				}

				writer.WriteLine("endlocal");
			}

			return ExitCode.Success;
		}
	}
}