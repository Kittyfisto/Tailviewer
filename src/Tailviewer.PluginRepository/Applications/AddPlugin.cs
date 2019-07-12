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
				if (!string.IsNullOrEmpty(options.Username))
				{
					if (!repository.TryGetAccessToken(options.Username, out var token))
						throw new CannotAddPluginException($"'{options.Username}' is not a valid username.");

					repository.AddPlugin(options.PluginFileName, token.ToString(), options.PublishTimestamp);
				}
				else if (!string.IsNullOrEmpty(options.AccessToken))
				{
					repository.AddPlugin(options.PluginFileName, options.AccessToken, options.PublishTimestamp);
				}
				else
				{
					throw new CannotAddPluginException("Either a username or an access-token must be specified");
				}
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
