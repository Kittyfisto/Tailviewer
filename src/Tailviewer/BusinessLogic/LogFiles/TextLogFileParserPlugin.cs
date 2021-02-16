using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.Parsers;
using Tailviewer.Plugins;

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

		private readonly IReadOnlyList<ITextLogFileParserPlugin> _plugins;

		public TextLogFileParserPlugin(IServiceContainer services)
		{
			var pluginLoader = services.Retrieve<IPluginLoader>();
			_plugins = pluginLoader.LoadAllOfType<ITextLogFileParserPlugin>()
			                          .ToList();
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
				return new ILogFileFormat[0];
			}
		}

		public ITextLogFileParser CreateParser(IServiceContainer services, ILogFileFormat format)
		{
			foreach (var plugin in _plugins)
			{
				if (TryCreateParser(plugin, services, format, out var parser))
				{
					return parser;
				}
			}

			return CreateDefaultParser(services);
		}

		private bool TryCreateParser(ITextLogFileParserPlugin plugin, IServiceContainer services, ILogFileFormat format,
		                             out ITextLogFileParser parser)
		{
			try
			{
				// We cannot trust plugins to adhere to the contract set up by the interface.
				// Therefore we wrap the "raw" plugin implementation by a layer which ensures that the plugin cannot screw up too bad
				var rawParser = plugin.CreateParser(services, format);
				parser = new NoThrowTextLogFileParser(rawParser);
				return true;
			}
			catch (Exception e)
			{
				Log.DebugFormat("Caught exception: {0}", e);

				parser = null;
				return false;
			}
		}

		#endregion
	}
}