using System;
using System.Reflection;
using log4net;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class AddUser
		: IApplication<AddUserOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int Run(PluginRepository repository, AddUserOptions options)
		{
			try
			{
				var accessToken = repository.AddUser(options.Username, options.Email);
				Log.InfoFormat("Added user {0}, {1}", options.Username, options.Email);

				Console.WriteLine("The following access token has been generated for this user");
				Console.WriteLine("DO NOT SHARE THIS TOKEN WITH ANYONE AS IT CAN BE USED TO PUSH PLUGINS TO THIS REPOSITORY");
				Console.WriteLine("\t{0}", accessToken);

				return 0;
			}
			catch (CannotAddUserException e)
			{
				Log.ErrorFormat(e.Message);
				return -1;
			}
		}
	}
}