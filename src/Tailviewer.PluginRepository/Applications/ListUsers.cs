using System;
using System.Linq;

namespace Tailviewer.PluginRepository.Applications
{
	public static class ListUsers
	{
		public static int Run(ListUsersOptions options)
		{
			using (var repository = new PluginRepository())
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
			}

			return 0;
		}
	}
}