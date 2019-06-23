using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using Tailviewer.Archiver.Plugins;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class PluginLogFileFactory
		: ILogFileFactory
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IReadOnlyList<IPluginWithDescription<IFileFormatPlugin>> _plugins;
		private readonly ITaskScheduler _taskScheduler;

		public PluginLogFileFactory(ITaskScheduler taskScheduler, IPluginWithDescription<IFileFormatPlugin>[] plugins)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (plugins == null)
				throw new ArgumentNullException(nameof(plugins));

			_plugins = new List<IPluginWithDescription<IFileFormatPlugin>>(plugins);
			_taskScheduler = taskScheduler;
		}

		public PluginLogFileFactory(ITaskScheduler taskScheduler, params IFileFormatPlugin[] plugins)
			: this(taskScheduler, plugins.Select(x => new PluginWithDescription<IFileFormatPlugin>(x, null)))
		{}

		public PluginLogFileFactory(ITaskScheduler taskScheduler, IEnumerable<IPluginWithDescription<IFileFormatPlugin>> plugins)
			: this(taskScheduler, plugins?.ToArray())
		{}

		/// <inheritdoc />
		public ILogFile Open(string filePath, out IPluginDescription pluginDescription)
		{
			var plugin = FindSupportingPlugin(filePath, out pluginDescription);
			if (plugin != null)
			{
				var logFile = OpenWith(filePath, plugin);
				if (logFile != null)
				{
					var pluginName = plugin.GetType().Assembly.FullName;
					return new NoThrowLogFile(logFile, pluginName);
				}
			}

			return new TextLogFile(_taskScheduler, filePath);
		}

		private IFileFormatPlugin FindSupportingPlugin(string filePath, out IPluginDescription pluginDescription)
		{
			var fileName = Path.GetFileName(filePath);
			foreach (var pair in _plugins)
			{
				if (Supports(pair.Plugin, fileName))
				{
					pluginDescription = pair.Description;
					return pair.Plugin;
				}
			}

			pluginDescription = null;
			return null;
		}

		[Pure]
		private static bool Supports(IFileFormatPlugin plugin, string fileName)
		{
			return SupportsByRegex(plugin, fileName) ||
			       SupportsByFileExtension(plugin, fileName);
		}

		private static bool SupportsByRegex(IFileFormatPlugin plugin, string fileName)
		{
			try
			{
				var regexes = (plugin as IFileFormatPlugin2)?.SupportedFileNames ?? Enumerable.Empty<Regex>();
				foreach (var regex in regexes)
				{
					if (regex.IsMatch(fileName))
					{
						Log.DebugFormat("Plugin {0} claims that it supports {1}...", plugin, fileName);
						return true;
					}
				}

				return false;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Plugin {0} threw an unexpected exception: {1}", plugin, e);
				return false;
			}
		}

		private static bool SupportsByFileExtension(IFileFormatPlugin plugin, string fileName)
		{
			try
			{
				var extensions = plugin.SupportedExtensions ?? Enumerable.Empty<string>();
				foreach (var extension in extensions)
				{
					if (fileName.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
					{
						Log.DebugFormat("Plugin {0} claims that it supports {1}...", plugin, fileName);
						return true;
					}
				}

				return false;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Plugin {0} threw an unexpected exception: {1}", plugin, e);
				return false;
			}
		}

		private ILogFile OpenWith(string fileName, IFileFormatPlugin plugin)
		{
			try
			{
				return plugin.Open(fileName, _taskScheduler);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Plugin {0} threw an unexpected exception while trying to open {1}: {2}",
					plugin,
					fileName,
					e);
				return null;
			}
		}
	}
}