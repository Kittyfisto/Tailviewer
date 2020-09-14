using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.Parsers;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Responsible for loading all available <see cref="ITextLogFileParserPlugin" /> implementations
	///     and delegating to them, in case they implement a particular format.
	/// </summary>
	public sealed class TextLogFileParserPlugin
		: ITextLogFileParserPlugin
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IReadOnlyDictionary<ILogFileFormat, ITextLogFileParserPlugin> _pluginsByFormat;
		private readonly IReadOnlyList<ILogFileFormat> _formats;

		public TextLogFileParserPlugin(IServiceContainer services)
		{
			var pluginLoader = services.Retrieve<IPluginLoader>();
			var plugins = pluginLoader.LoadAllOfType<ITextLogFileParserPlugin>()
			                          .ToList();
			_pluginsByFormat = GroupByFormat(plugins);
			_formats = _pluginsByFormat.Keys.ToList();
		}

		[Pure]
		private static Dictionary<ILogFileFormat, ITextLogFileParserPlugin> GroupByFormat(
			IReadOnlyList<ITextLogFileParserPlugin> plugins)
		{
			var pluginsByFormat = new Dictionary<ILogFileFormat, ITextLogFileParserPlugin>();
			foreach (var plugin in plugins) GroupByFormat(plugin, pluginsByFormat);

			return pluginsByFormat;
		}

		private static void GroupByFormat(ITextLogFileParserPlugin plugin,
		                                  Dictionary<ILogFileFormat, ITextLogFileParserPlugin> pluginsByFormat)
		{
			var supportedFormats = plugin.SupportedFormats;
			try
			{
				if (supportedFormats.Count > 0)
					foreach (var format in supportedFormats)
						if (pluginsByFormat.TryGetValue(format, out var existingPlugin))
							Log.WarnFormat("Plugin '{0}' already implements parsing for format '{1}', skipping plugin '{2}' which does the same, but was loaded later",
							               existingPlugin,
							               format,
							               plugin);
						else
							pluginsByFormat.Add(format, plugin);
				else
					Log.WarnFormat("Plugin '{0}' claims to support 0 formats, it won't be used...", plugin);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Caught unexpected exception while retrieving supported formats of plugin '{0}', it won't be used...",
				               e);
			}
		}

		private ITextLogFileParser CreateDefaultParser(IServiceContainer services)
		{
			var timestampParser = services.TryRetrieve<ITimestampParser>() ?? new TimestampParser();
			return new TextLogFileParser(timestampParser);
		}

		#region Implementation of ITextLogFileParserPlugin

		public IReadOnlyList<ILogFileFormat> SupportedFormats
		{
			get
			{
				return _formats;
			}
		}

		public ITextLogFileParser CreateParser(IServiceContainer services, ILogFileFormat format)
		{
			if (_pluginsByFormat.TryGetValue(format, out var plugin))
				try
				{
					return new NoThrowTextLogFileParser(plugin.CreateParser(services, format));
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Caught unexpected exception while trying to create parser for format '{0}' through plugin '{1}', falling back to default: {2}",
					                format,
					                plugin,
					                e);
					return CreateDefaultParser(services);
				}

			return CreateDefaultParser(services);
		}

		#endregion
	}
}