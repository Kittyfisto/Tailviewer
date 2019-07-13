using CommandLine;

namespace Tailviewer.Archiver.Applications
{
	[Verb("git-get-changes", HelpText = "Extract a changelist from git commit messages and write them to a changes.xml file. Git must be part of the PATH variable for this to work.")]
	public class GitGetChangesOptions
	{
		[Option('r', "repository",
			Default = null,
			Required = true,
			HelpText = "The git repository from which changes should be extracted")]
		public string Repository { get; set; }

		[Option('s', "since-last-tag",
			Default = false,
			Required = false,
			HelpText = "Get only changes which were made in between the last tag and HEAD")]
		public bool SinceLastTag { get; set; }

		[Option('f', "filter",
			Default = null,
			Required = false,
			HelpText = "Only include commit messages in the output which contain the given filter phrase. If none is specified, all commits are included.")]
		public string Filter { get; set; }

		[Option('o', "output",
			Default = "master",
			Required = true,
			HelpText = "The output file to which the changelist should be written. This file can be used with the pack verb to include these changes in a plugin's changelist.")]
		public string Output { get; set; }
	}
}
