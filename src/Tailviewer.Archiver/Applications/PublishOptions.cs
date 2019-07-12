using CommandLine;

namespace Tailviewer.Archiver.Applications
{
	[Verb("publish", HelpText = "Publish a tailviewer plugin to a plugin repository")]
	public sealed class PublishOptions
	{
		[Value(0, MetaName = "file name",
			HelpText = "The filename of the plugin to publish, should end in .tvpr",
			Required = true)]
		public string Plugin { get; set; }

		[Value(1, MetaName = "server address",
			HelpText = "The address of the plugin repository, currently only the tvpr:// protocol is supported",
			Required = true)]
		public string ServerAddress { get; set; }

		[Option('a', "access-token",
			Default = null,
			Required = true,
			HelpText = "The access token of the user under which the plugin should be published")]
		public string AccessToken { get; set; }
	}
}
