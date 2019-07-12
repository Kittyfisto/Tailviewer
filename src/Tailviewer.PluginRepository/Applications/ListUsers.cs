using System;
using System.IO;
using System.Linq;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class ListUsers
		: IApplication<ListUsersOptions>
	{
		public ExitCode Run(IFilesystem filesystem, IInternalPluginRepository repository, ListUsersOptions options)
		{
			var users = repository.GetAllUsers().ToList();

			if (users.Any())
			{
				Console.WriteLine("There are {0} user(s):", users.Count);
				foreach (var user in users)
				{
					Console.WriteLine("\t{0}, {1}, access token: {2}", user.Username, user.Email, user.AccessToken);
				}
			}
			else
			{
				Console.WriteLine("No users have been added");
			}

			return ExitCode.GenericFailure;
		}
	}
}