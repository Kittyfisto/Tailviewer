using CommandLine;

namespace Tailviewer.PluginRepository.Applications
{
	[Verb("remove-plugin", HelpText = "Remove a plugin from this repository")]
	public sealed class RemovePluginOptions
	{
		[Value(0, MetaName = "plugin id",
			HelpText = "The id of the plugin to remove, for example Tailviewer.Analysis.Count",
			Required = true)]
		public string Id { get; set; }

		[Option('v', "version",
			Default = null,
			HelpText = "The specific version of the plugin to remove, for example 0.1.2",
			Required = true)]
		public string Version { get; set; }
	}
}
