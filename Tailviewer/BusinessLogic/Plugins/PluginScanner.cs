using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     Responsible for finding plugins in a directory tree.
	/// </summary>
	public sealed class PluginScanner : IPluginScanner
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly IReadOnlyList<Type> PluginInterfaces;

		static PluginScanner()
		{
			PluginInterfaces = new[] {typeof(IFileFormatPlugin)};
		}

		/// <summary>
		///     Loads all plugins in the given directory path (recursive).
		///     Plugins are .NET assemblies compiled against AnyCPU that implement at least
		///     one of the available <see cref="IPlugin" /> interfaces.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public IReadOnlyList<IPluginDescription> ReflectPlugins(string path)
		{
			var plugins = new List<IPluginDescription>();
			var assemblies = Directory.EnumerateFiles(path, "*.tvp", SearchOption.AllDirectories);
			foreach (var plugin in assemblies)
			{
				IPluginDescription description;
				if (TryLoad(plugin, out description))
					plugins.Add(description);
				// TODO: Maybe we should create a description for plugins we couldn't load, so we display
				//       those in the list of plugins as well?
			}
			return plugins;
		}

		public IPluginDescription ReflectPlugin(string pluginPath)
		{
			var assembly = Assembly.LoadFrom(pluginPath);
			var authorAttribute = assembly.GetCustomAttribute<PluginAuthorAttribute>();
			var websiteAttribute = assembly.GetCustomAttribute<PluginWebsiteAttribute>();
			var descriptionAttribute = assembly.GetCustomAttribute<PluginDescriptionAttribute>();

			if (authorAttribute == null)
				Log.WarnFormat("Plugin '{0}' is missing the PluginAuthor attribute, please consider adding it",
					pluginPath);

			if (websiteAttribute == null)
				Log.WarnFormat("Plugin '{0}' is missing the PluginWebsite attribute, please consider adding it",
					pluginPath);

			if (descriptionAttribute == null)
				Log.WarnFormat("Plugin '{0}' is missing the PluginDescription attribute, please consider adding it",
					pluginPath);

			var plugins = FindPluginImplementations(assembly);
			if (plugins.Count == 0)
			{
				Log.WarnFormat("Plugin '{0}' doesn't implement any of the available plugin interfaces: {1}",
					pluginPath,
					string.Join(", ", PluginInterfaces.Select(x => x.Name)));
			}

			return new PluginDescription
			{
				Author = authorAttribute?.Author,
				Website = websiteAttribute?.Website,
				Description = descriptionAttribute?.Description,
				FilePath = pluginPath,
				Plugins = plugins
			};
		}

		private bool TryLoad(string filePath, out IPluginDescription description)
		{
			try
			{
				description = ReflectPlugin(filePath);
				return true;
			}
			catch (FileLoadException e)
			{
				Log.ErrorFormat("Unable to load plugin '{0}': {1}", filePath, e);
				description = null;
				return false;
			}
			catch (BadImageFormatException e)
			{
				Log.ErrorFormat("Unable to load plugin '{0}' (plugins must be compiled against AnyCPU): {1}", filePath, e);
				description = null;
				return false;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while trying to load plugin '{0}': {1}",
					filePath, e);
				description = null;
				return false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		private IReadOnlyDictionary<Type, string> FindPluginImplementations(Assembly assembly)
		{
			var plugins = new Dictionary<Type, string>();
			foreach (var type in assembly.ExportedTypes)
			foreach (var @interface in PluginInterfaces)
				if (type.GetInterface(@interface.FullName) != null)
					plugins.Add(@interface, type.AssemblyQualifiedName);

			// TODO: Inspect non-public types and log a warning if one implements the IPlugin interface
			return plugins;
		}
	}
}