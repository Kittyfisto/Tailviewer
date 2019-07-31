using System.IO;
using System.Reflection;
using log4net;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class RemoveUser
		: IApplication<RemoveUserOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public bool RequiresRepository => true;

		public bool ReadOnlyRepository => false;

		public ExitCode Run(IFilesystem filesystem, IInternalPluginRepository repository, RemoveUserOptions options)
		{
			try
			{
				repository.RemoveUser(options.Username);
				Log.InfoFormat("Removed user '{0}'", options.Username);

				return ExitCode.Success;
			}
			catch (CannotRemoveUserException e)
			{
				Log.ErrorFormat(e.Message);
				return ExitCode.GenericFailure;
			}
		}
	}
}