using CommandLine;

namespace Tailviewer.PluginRepository.Applications
{
	[Verb("write-configuration", HelpText = "Writes an example configuration to disk which can be used with the run verb")]
	public sealed class WriteConfigurationOptions
	{
		[Value(0, MetaName = "filename",
			Default = "configuration.xml",
			HelpText = "The filename where the configuration should be stored")]
		public string Filename { get; set; }
	}
}