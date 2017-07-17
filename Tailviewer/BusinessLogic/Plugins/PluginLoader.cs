using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using log4net;

namespace Tailviewer.BusinessLogic.Plugins
{
	public sealed class PluginLoader
		: IPluginLoader
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// 
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
			{
				throw new ArgumentException(string.Format("Plugin '{0}' does not define a type named '{1}'",
					description.FilePath,
					assemblyQualifiedName));
			}

			var plugin = (T) Activator.CreateInstance(implementation);
			return plugin;
		}

		[Pure]
		private static string GetFullTypeName(string assemblyQualifiedName)
		{
			var index = assemblyQualifiedName.IndexOf(',');
			if (index == -1)
				return assemblyQualifiedName;

			return assemblyQualifiedName.Substring(0, index);
		}

		public IEnumerable<T> LoadAllOfType<T>(IEnumerable<IPluginDescription> pluginDescriptions)
			where T : class, IPlugin
		{
			var ret = new List<T>();
			foreach (var pluginDescription in pluginDescriptions)
			{
				if (pluginDescription.Plugins.ContainsKey(typeof(T)))
				{
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
				}
			}
			return ret;
		}

		public void Dispose()
		{}
	}
}