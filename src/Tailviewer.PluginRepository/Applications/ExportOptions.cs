using CommandLine;

namespace Tailviewer.PluginRepository.Applications
{
	[Verb("export", HelpText = "Exports the entire database into the given file")]
	public sealed class ExportOptions
	{
		[Value(0, MetaName = "Export folder",
			HelpText = "The folder the database should be exported to",
			Required = true)]
		public string ExportFolder { get; set; }
	}
}
