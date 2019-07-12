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

	}
}
