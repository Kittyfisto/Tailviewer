using CommandLine;

namespace Tailviewer.PluginRepository.Applications
{
	[Verb("add-plugin", HelpText = "Add a tailviewer plugin to the repository")]
	public sealed class AddPluginOptions
	{
		[Value(0, MetaName = "plugin file",
			HelpText = "The file path to the tailviewer plugin (*.tvp) to be added",
			Required = true)]
		public string PluginFileName { get; set; }

		[Option('u', "user",
			Default = null,
			HelpText = "The username under which to publish the plugin")]
		public string Username { get; set; }

		[Option('a', "access-token",
			Default = null,
			HelpText = "The access token of the user under which the plugin should be published")]
		public string AccessToken { get; set; }
	}
}
