using CommandLine;

namespace Tailviewer.PluginRepository.Applications
{
	[Verb("run", HelpText = "Run the repository server and serve requests to tailviewer clients")]
	public sealed class RunServerOptions
	{
		[Option('c', "configuration",
			Default = null,
			HelpText = "The filename of the configuration file to use")]
		public string Configuration { get; set; }

		[Option("allow-remote-publish",
			Default = null,
			HelpText = "Allow remote publishing via archive.exe. When both a configuration file and this option is specified, then this setting wil overwrite any value of the configuration file.")]
		public bool? AllowRemotePublish { get; set; }
	}
}
