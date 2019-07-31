using System.IO;
using System.Reflection;
using log4net;
using Tailviewer.PluginRepository.Configuration;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class WriteConfiguration
		: IApplication<WriteConfigurationOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Implementation of IApplication<in WriteConfigurationOptions>

		public bool RequiresRepository => false;

		public bool ReadOnlyRepository => false;

		public ExitCode Run(IFilesystem filesystem, IInternalPluginRepository repository, WriteConfigurationOptions options)
		{
			var filePath = MakeAbsolute(options.Filename);
			Log.DebugFormat("Writing configuration to '{0}'...", filePath);

			new ServerConfiguration().WriteTo(filePath);

			Log.InfoFormat("Configuration file written to '{0}'", filePath);

			return ExitCode.Success;
		}

		#endregion

		private string MakeAbsolute(string fileName)
		{
			if (Path.IsPathRooted(fileName))
				return fileName;

			return Path.Combine(Directory.GetCurrentDirectory(), fileName);
		}
	}
}
