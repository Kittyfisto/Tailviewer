using System.Collections.Generic;
using CommandLine;

namespace Tailviewer.Archiver.Applications
{
	[Verb("pack", HelpText = "Create a new tailviewer plugin archive")]
	public sealed class PackOptions
	{
		[Value(0, MetaName = "input file",
			HelpText = "Visual Studio Solution (*.sln), C# Project (*.csproj) or .NET Assembly (*.dll) from which the plugin should be created",
			Required = true)]
		public string InputFileName { get; set; }

		[Option('c', "configuration",
			Default = "Release",
			HelpText = "The Visual Studio configuration (such as Release or Debug) which should be used when determining the output files to be packed. By default, Release is used. Only needs to be specified when a visual studio solution or c# project is used as input.")]
		public string Configuration { get; set; }

		[Option('f', "files",
			SetName = "bylines",
			HelpText = "Additional files to be added to the archive.")]
		public IEnumerable<string> Files { get; set; }

		[Option('i', "icon",
			HelpText = "Additonal ")]
		public string IconFileName { get; set; }
	}
}