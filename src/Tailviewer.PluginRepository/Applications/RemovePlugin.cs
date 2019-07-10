using System.Reflection;
using log4net;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class RemovePlugin
		: IApplication<RemovePluginOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int Run(PluginRepository repository, RemovePluginOptions options)
		{
			try
			{
				repository.RemovePlugin(options.Id, options.Version);
			}
			catch (CannotRemovePluginException e)
			{
				Log.ErrorFormat(e.Message);
				return -10;
			}

			return 0;
		}
	}
}
