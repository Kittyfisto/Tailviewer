using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using log4net;
using Tailviewer.PluginRepository.Configuration;

namespace Tailviewer.PluginRepository.Applications
{
	public sealed class WriteConfiguration
		: IApplication<WriteConfigurationOptions>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Implementation of IApplication<in WriteConfigurationOptions>

		public int Run(IFilesystem filesystem, IInternalPluginRepository repository, WriteConfigurationOptions options)
		{
			var filePath = MakeAbsolute(options.Filename);
			Log.DebugFormat("Writing configuration to '{0}'...", filePath);

			XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
			namespaces.Add("", "");
			var serializer = new XmlSerializer(typeof(ServerConfiguration));
			using (var fileStream = File.Create(options.Filename))
			{
				serializer.Serialize(fileStream, new ServerConfiguration(), namespaces);
			}

			Log.InfoFormat("Configuration file written to '{0}'", filePath);

			return 0;
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
