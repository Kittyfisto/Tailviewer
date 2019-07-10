using CommandLine;

namespace Tailviewer.PluginRepository.Applications
{
	[Verb("remove-user", HelpText = "Remove a previously added user from the repository")]
	public sealed class RemoveUserOptions
	{
		[Value(0, MetaName = "user name",
			HelpText = "The user name of the user to remove from the repository",
			Required = true)]
		public string Username { get; set; }
	}
}