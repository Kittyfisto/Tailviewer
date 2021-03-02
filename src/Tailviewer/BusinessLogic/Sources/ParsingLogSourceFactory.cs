using System.Linq;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources.Text;

namespace Tailviewer.BusinessLogic.Sources
{
	public sealed class ParsingLogSourceFactory
		: ILogSourceParserPlugin
	{
		private readonly IServiceContainer _services;
		private readonly IPluginLoader _pluginLoader;

		public ParsingLogSourceFactory(IServiceContainer services)
		{
			_services = services;
			_pluginLoader = services.Retrieve<IPluginLoader>();
		}

		#region Implementation of ILogSourceParserPlugin

		public ILogSource CreateParser(IServiceContainer services, ILogSource source)
		{
			var format = source.GetProperty(GeneralProperties.Format);
			var logSourceParserPlugins = _pluginLoader?.LoadAllOfType<ILogSourceParserPlugin>() ?? Enumerable.Empty<ILogSourceParserPlugin>();
			foreach (var plugin in logSourceParserPlugins)
			{
				var parser = plugin.CreateParser(_services, source);
				if (parser != null)
				{
					return parser;
				}
			}

			var logEntryParserPlugins = _pluginLoader?.LoadAllOfType<ILogEntryParserPlugin>() ?? Enumerable.Empty<ILogEntryParserPlugin>();
			foreach(var plugin in logEntryParserPlugins)
			{
				var parser = plugin.CreateParser(_services, format);
				if (parser != null)
				{
					return new GenericTextLogSource(source, parser);
				}
			}

			return new GenericTextLogSource(source, new GenericTextLogEntryParser());
		}

		#endregion
	}
}
