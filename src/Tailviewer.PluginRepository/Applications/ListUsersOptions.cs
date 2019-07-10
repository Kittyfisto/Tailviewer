using CommandLine;

namespace Tailviewer.PluginRepository.Applications
{
	[Verb("list-users", HelpText = "List the users who are allowed to push new plugins from remote locations")]
	public sealed class ListUsersOptions
	{
	}
}