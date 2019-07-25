using System.IO;
using System.Reflection;
using log4net;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class RemovePlugin
		: IApplication<RemovePluginOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public bool RequiresRepository => true;

		public ExitCode Run(IFilesystem filesystem, IInternalPluginRepository repository, RemovePluginOptions options)
		{
			try
			{
				repository.RemovePlugin(options.Id, options.Version);
			}
			catch (CannotRemovePluginException e)
			{
				Log.ErrorFormat(e.Message);
				return ExitCode.GenericFailure;
			}

			return ExitCode.Success;
		}
	}
}
