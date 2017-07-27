using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Core.LogFiles
{
	public sealed class PluginLogFileFactory
		: ILogFileFactory
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IReadOnlyList<IFileFormatPlugin> _plugins;
		private readonly ITaskScheduler _taskScheduler;

		public PluginLogFileFactory(ITaskScheduler taskScheduler, params IFileFormatPlugin[] plugins)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (plugins == null)
				throw new ArgumentNullException(nameof(plugins));

			_plugins = new List<IFileFormatPlugin>(plugins);
			_taskScheduler = taskScheduler;
		}

		public PluginLogFileFactory(ITaskScheduler taskScheduler, IEnumerable<IFileFormatPlugin> plugins)
			: this(taskScheduler, plugins?.ToArray())
		{}

		/// <inheritdoc />
		public ILogFile Open(string fileName)
		{
			var plugin = FindSupportingPlugin(fileName);
			if (plugin != null)
			{
				var logFile = OpenWith(fileName, plugin);
				if (logFile != null)
				{
					var pluginName = plugin.GetType().Assembly.FullName;
					return new NoThrowLogFile(logFile, pluginName);
				}
			}

			return new TextLogFile(_taskScheduler, fileName);
		}

		private IFileFormatPlugin FindSupportingPlugin(string fileName)
		{
			foreach (var plugin in _plugins)
			{
				if (Supports(plugin, fileName))
				{
					return plugin;
				}
			}

			return null;
		}

		[Pure]
		private static bool Supports(IFileFormatPlugin plugin, string fileName)
		{
			try
			{
				var extensions = plugin.SupportedExtensions;
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