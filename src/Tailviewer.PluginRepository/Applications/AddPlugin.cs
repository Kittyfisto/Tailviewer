using System.IO;
using System.Reflection;
using log4net;
using Tailviewer.Archiver.Repository;
using Tailviewer.PluginRepository.Exceptions;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class AddPlugin
		: IApplication<AddPluginOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public bool RequiresRepository => true;

		public bool ReadOnlyRepository => false;

		public ExitCode Run(IFilesystem filesystem, IInternalPluginRepository repository, AddPluginOptions options)
		{
			try
			{
				if (!TryLoadPlugin(filesystem, options.PluginFileName,
				                   out var plugin,
				                   out var exitCode))
					return exitCode;

				if (!string.IsNullOrEmpty(options.Username))
				{
					if (!repository.TryGetAccessToken(options.Username, out var token))
					{
						Log.InfoFormat("'{0}' is not a valid username.", options.Username);
						return ExitCode.InvalidUserName;
					}

					repository.PublishPlugin(plugin, token.ToString(), options.PublishTimestamp);
				}
				else if (!string.IsNullOrEmpty(options.AccessToken))
				{
					repository.PublishPlugin(plugin, options.AccessToken, options.PublishTimestamp);
				}
				else
				{
					Log.ErrorFormat("Either a username or an access-token must be specified");
					return ExitCode.GenericFailure;
				}
			}
			catch (InvalidUserTokenException e)
			{
				Log.ErrorFormat(e.Message);
				return ExitCode.InvalidUserToken;
			}

			return ExitCode.Success;
		}

		private static bool TryLoadPlugin(IFilesystem filesystem, string fileName,
		                                  out byte[] plugin,
		                                  out ExitCode exitCode)
		{
			try
			{
				plugin = filesystem.ReadAllBytes(fileName);
				exitCode = ExitCode.Success;
				return true;
			}
			catch (DirectoryNotFoundException e)
			{
				Log.ErrorFormat("Unable to add plugin: {0}", e.Message);
				plugin = null;
				exitCode = ExitCode.DirectoryNotFound;
				return false;
			}
			catch (FileNotFoundException e)
			{
				Log.ErrorFormat("Unable to add plugin: {0}", e.Message);
				plugin = null;
				exitCode = ExitCode.FileNotFound;
				return false;
			}
		}
	}
}
