using System.Collections.Generic;
using CommandLine;

namespace Tailviewer.Archiver.Plugins
{
	[Verb("pack", HelpText = "Create a new tailviewer plugin archive")]
	public sealed class PackOptions
	{
		[Value(0, MetaName = "input file",
			HelpText = "Plugin file which should be packed",
			Required = true)]
		public string PluginFileName { get; set; }

		[Option('o', "output",
			HelpText = "Filename of the resulting plugin archive.")]
		public string ArchiveFileName { get; set; }

		[Option('f', "files",
			SetName = "bylines",
			HelpText = "Additional files to be added to the archive.")]
		public IEnumerable<string> Files { get; set; }
	}
}