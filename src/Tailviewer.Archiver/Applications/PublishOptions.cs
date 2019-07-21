using CommandLine;

namespace Tailviewer.Archiver.Applications
{
	[Verb("publish", HelpText = "Publish a tailviewer plugin to a plugin repository")]
	public sealed class PublishOptions
	{
		[Value(0, MetaName = "file name",
			HelpText = "The filename of the plugin to publish, should end in .tvpr. A wildcard pattern may be used to publish multiple plugins at the same time.",
			Required = true)]
		public string Plugin { get; set; }

		[Option('r', "repository",
			Default = null,
			Required = true,
			HelpText = "The address of the plugin repository, must start with tvpr://")]
		public string Repository { get; set; }

		[Option('a', "access-token",
			Default = null,
			Required = true,
			HelpText = "The access token of the user under which the plugin should be published")]
		public string AccessToken { get; set; }
	}
}
