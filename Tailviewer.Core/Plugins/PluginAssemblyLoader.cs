using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Core.Plugins
{
	/// <summary>
	///     Responsible for finding and loading plugins in a directory tree.
	///     Is only used in development mode (Tailviewer.exe -d)
	/// </summary>
	public sealed class PluginAssemblyLoader
		: IPluginLoader
			, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly IReadOnlyList<Type> PluginInterfaces;

		static PluginAssemblyLoader()
		{
			PluginInterfaces = new[] {typeof(IFileFormatPlugin)};
		}

		public PluginAssemblyLoader()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="description"></param>
		/// <returns></returns>
		public T Load<T>(IPluginDescription description) where T : class, IPlugin
		{
			if (description == null)
				throw new ArgumentNullException(nameof(description));

			var assembly = Assembly.LoadFrom(description.FilePath);
			var assemblyQualifiedName = description.Plugins[typeof(T)];
			var fullTypeName = GetFullTypeName(assemblyQualifiedName);
			var implementation = assembly.GetType(fullTypeName);
			if (implementation == null)
				throw new ArgumentException(string.Format("Plugin '{0}' does not define a type named '{1}'",
					description.FilePath,
					assemblyQualifiedName));

			var plugin = (T) Activator.CreateInstance(implementation);
			return plugin;
		}

		/// <inheritdoc />
		public IEnumerable<T> LoadAllOfType<T>(IEnumerable<IPluginDescription> pluginDescriptions)
			where T : class, IPlugin
		{
			var ret = new List<T>();
			foreach (var pluginDescription in pluginDescriptions)
				if (pluginDescription.Plugins.ContainsKey(typeof(T)))
					try
					{
						var plugin = Load<T>(pluginDescription);
						ret.Add(plugin);
					}
					catch (Exception e)
					{
						Log.ErrorFormat("Unable to load plugin of interface '{0}' from '{1}': {2}",
							typeof(T),
							pluginDescription,
							e);
					}
			return ret;
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
			try
			{
				var assemblies = Directory.EnumerateFiles(path, "*.tvp", SearchOption.AllDirectories);
				foreach (var plugin in assemblies)
				{
					IPluginDescription description;
					TryLoad(plugin, out description);
					plugins.Add(description);
				}
			}
			catch (DirectoryNotFoundException e)
			{
				Log.WarnFormat("Unable to find plugins in '{0}': {1}", path, e);
			}
			return plugins;
		}

		/// <inheritdoc />
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
				Log.WarnFormat("Plugin '{0}' doesn't implement any of the available plugin interfaces: {1}",
					pluginPath,
					string.Join(", ", PluginInterfaces.Select(x => x.Name)));

			return new PluginDescription
			{
				Author = authorAttribute?.Author,
				Website = websiteAttribute?.Website,
				Description = descriptionAttribute?.Description,
				FilePath = pluginPath,
				Plugins = plugins
			};
		}

		private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			// We will allow the user to load assemblies that are not an exact match to the installed assemblies:
			// This list consists of the Api assembly and all of its dependencies:
			var assemblies = new Dictionary<string, Assembly>
			{
				{"log4net,", typeof(ILog).Assembly},
				{"Metrolib,", typeof(AbstractBootstrapper).Assembly},
				{"System.Threading.Extensions,", typeof(ITaskScheduler).Assembly},
				{"Tailviewer.Api,", typeof(ILogFile).Assembly},
				{"Tailviewer.Core,", typeof(TextLogFile).Assembly}
			};

			// TODO: This feature most certainly needs proper tests. Go write them!

			foreach (var assembly in assemblies)
				if (args.Name.StartsWith(assembly.Key))
				{
					Log.InfoFormat("Assembly '{0}' requests to load '{1}', resolving to '{2}'",
						args.RequestingAssembly,
						args.Name,
						assembly.Value
					);
					return assembly.Value;
				}

			return null;
		}

		[Pure]
		private static string GetFullTypeName(string assemblyQualifiedName)
		{
			var index = assemblyQualifiedName.IndexOf(',');
			if (index == -1)
				return assemblyQualifiedName;

			return assemblyQualifiedName.Substring(0, index);
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
				var error = string.Format("Unable to load plugin '{0}': {1}", filePath, e);
				Log.Error(error);
				description = new PluginDescription
				{
					FilePath = filePath,
					Error = error
				};
				return false;
			}
			catch (BadImageFormatException e)
			{
				var error = string.Format("Unable to load plugin '{0}' (plugins must be compiled against AnyCPU): {1}", filePath,
					e);
				Log.Error(error);
				description = new PluginDescription
				{
					FilePath = filePath,
					Error = error
				};
				return false;
			}
			catch (Exception e)
			{
				var error = string.Format("Caught unexpected exception while trying to load plugin '{0}': {1}",
					filePath, e);
				Log.Error(error);
				description = new PluginDescription
				{
					FilePath = filePath,
					Error = error
				};
				return false;
			}
		}

		/// <summary>
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