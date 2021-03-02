using System;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Sources
{
	public sealed class ParsingLogSourceFactory
		: ILogSourceParserPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
			var format = source.GetProperty(Properties.Format);
			var logSourceParserPlugins = _pluginLoader?.LoadAllOfTypeWithDescription<ILogSourceParserPlugin>() ?? Enumerable.Empty<IPluginWithDescription<ILogSourceParserPlugin>>();
			foreach (var plugin in logSourceParserPlugins)
			{
				var parser = TryCreateParser(plugin, source);
				if (parser != null)
				{
					return parser;
				}
			}

			var logEntryParserPlugins = _pluginLoader?.LoadAllOfTypeWithDescription<ILogEntryParserPlugin>() ?? Enumerable.Empty<IPluginWithDescription<ILogEntryParserPlugin>>();
			foreach(var plugin in logEntryParserPlugins)
			{
				var parser = TryCreateParser(plugin, format);
				if (parser != null)
				{
					return new GenericTextLogSource(source, parser);
				}
			}

			return new GenericTextLogSource(source, new GenericTextLogEntryParser());
		}

		#endregion

		ILogSource TryCreateParser(IPluginWithDescription<ILogSourceParserPlugin> pair, ILogSource source)
		{
			try
			{
				var parser = pair.Plugin.CreateParser(_services, source);
				if (parser != null)
					return new NoThrowLogSource(parser, pair.Description.Name);

				return null;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception from plugin {0}: {1}", pair?.Description?.Id, e);
				return null;
			}
		}

		ILogEntryParser TryCreateParser(IPluginWithDescription<ILogEntryParserPlugin> pair, ILogFileFormat format)
		{
			try
			{
				var parser = pair.Plugin.CreateParser(_services, format);
				if (parser != null)
					return new NoThrowLogEntryParser(parser);

				return null;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception from plugin {0}: {1}", pair?.Description?.Id, e);
				return null;
			}
		}
	}
}
