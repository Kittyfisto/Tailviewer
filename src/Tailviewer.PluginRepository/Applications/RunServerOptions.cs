using CommandLine;

namespace Tailviewer.PluginRepository.Applications
{
	[Verb("run", HelpText = "Run the repository server and serve requests to tailviewer clients")]
	public sealed class RunServerOptions
	{
		[Option("allow-remote-publish",
			Default = null,
			HelpText = "Allow remote publishing via archive.exe. This setting overwrites the value specified in the configuration file.")]
		public bool? AllowRemotePublish { get; set; }
	}
}
