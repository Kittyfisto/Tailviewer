using System;
using System.Reflection;
using log4net;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public static class RemoveUser
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static int Run(RemoveUserOptions options)
		{
			try
			{
				using (var repo = new PluginRepository())
				{
					repo.RemoveUser(options.Username);
					Log.InfoFormat("Removed user '{0}'", options.Username);

					return 0;
				}
			}
			catch (CannotRemoveUserException e)
			{
				Log.ErrorFormat(e.Message);
				return -1;
			}
		}
	}
}