using System;
using System.IO;
using System.Reflection;
using log4net;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class AddUser
		: IApplication<AddUserOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int Run(IFilesystem filesystem, IInternalPluginRepository repository, AddUserOptions options)
		{
			try
			{
				var accessToken = repository.AddUser(options.Username, options.Email);
				Log.InfoFormat("Added user {0}, {1}", options.Username, options.Email);

				Log.InfoFormat("The following access token has been generated for this user");
				Log.InfoFormat("DO NOT SHARE THIS TOKEN WITH ANYONE AS IT CAN BE USED TO PUSH PLUGINS TO THIS REPOSITORY");
				Log.InfoFormat("\t{0}", accessToken);

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