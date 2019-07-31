using CommandLine;

namespace Tailviewer.PluginRepository.Applications
{
	[Verb("add-user", HelpText = "Add a user to the repository that is allowed to push new plugins from remote locations")]
	public sealed class AddUserOptions
	{
		[Value(0, MetaName = "user name",
			HelpText = "The username",
			Required = true)]
		public string Username { get; set; }

		[Value(1, MetaName = "email",
			HelpText = "The email of the user",
			Required = true)]
		public string Email { get; set; }

		[Option('a', "access-token",
			Default = null,
			HelpText = "The access token (in the form of a GUID) of the user. If none is specified, then a new access token is generated.")]
		public string AccessToken { get; set; }
	}
}
