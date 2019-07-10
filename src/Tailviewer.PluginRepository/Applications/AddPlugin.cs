using System.Reflection;
using log4net;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class AddPlugin
		: IApplication<AddPluginOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int Run(PluginRepository repository, AddPluginOptions options)
		{
			try
			{
				repository.AddPlugin(options.PluginFileName, options.AccessToken);
			}
			catch (CannotAddPluginException e)
			{
				Log.ErrorFormat(e.Message);
				return -10;
			}

			return 0;
		}
	}
}
