using System.IO;
using System.Reflection;
using log4net;
using Tailviewer.Archiver;
using Tailviewer.Archiver.Repository;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class AddPlugin
		: IApplication<AddPluginOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int Run(IFilesystem filesystem, IInternalPluginRepository repository, AddPluginOptions options)
		{
			try
			{
				var plugin = LoadPlugin(filesystem, options.PluginFileName);

				if (!string.IsNullOrEmpty(options.Username))
				{
					if (!repository.TryGetAccessToken(options.Username, out var token))
						throw new CannotAddPluginException($"'{options.Username}' is not a valid username.");

					repository.PublishPlugin(plugin, token.ToString(), options.PublishTimestamp);
				}
				else if (!string.IsNullOrEmpty(options.AccessToken))
				{
					repository.PublishPlugin(plugin, options.AccessToken, options.PublishTimestamp);
				}
				else
				{
					throw new CannotAddPluginException("Either a username or an access-token must be specified");
				}
			}
			catch (InvalidUserTokenException e)
			{
				Log.ErrorFormat(e.Message);
				return (int) ExitCode.InvalidUserToken;
			}
			catch (CannotAddPluginException e)
			{
				Log.ErrorFormat(e.Message);
				return -10;
			}

			return 0;
		}

		private static byte[] LoadPlugin(IFilesystem filesystem, string fileName)
		{
			try
			{
				return filesystem.ReadAllBytes(fileName);
			}
			catch (DirectoryNotFoundException e)
			{
				throw new CannotAddPluginException($"Unable to add plugin: {e.Message}", e);
			}
			catch (FileNotFoundException e)
			{
				throw new CannotAddPluginException($"Unable to add plugin: {e.Message}", e);
			}
		}
	}
}
