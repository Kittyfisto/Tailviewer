using CommandLine;

namespace Tailviewer.PluginRegistry.Applications
{
	[Verb("add", HelpText = "Add a tailviewer plugin to the repository")]
	public sealed class AddPluginOptions
	{
		[Value(0, MetaName = "plugin file",
			HelpText = "The file path to the tailviewer plugin (*.tvp) to be added",
			Required = true)]
		public string PluginFileName { get; set; }
	}
}
