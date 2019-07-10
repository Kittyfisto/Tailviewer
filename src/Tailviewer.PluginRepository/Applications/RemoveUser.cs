using System.Reflection;
using log4net;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class RemoveUser
		: IApplication<RemoveUserOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int Run(PluginRepository repository, RemoveUserOptions options)
		{
			try
			{
				repository.RemoveUser(options.Username);
				Log.InfoFormat("Removed user '{0}'", options.Username);

				return 0;
			}
			catch (CannotRemoveUserException e)
			{
				Log.ErrorFormat(e.Message);
				return -1;
			}
		}
	}
}