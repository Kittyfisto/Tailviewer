using System;
using System.Reflection;

namespace Tailviewer.BusinessLogic.Plugins
{
	public sealed class PluginLoader
		: IPluginLoader
	{
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
			var typeName = description.Plugins[typeof(T)];
			var implementation = assembly.GetType(typeName);
			if (implementation == null)
			{
				throw new ArgumentException(string.Format("Plugin '{0}' does not define a type named '{1}'",
					description.FilePath,
					typeName));
			}

			var plugin = (T) Activator.CreateInstance(implementation);
			return plugin;
		}
	}
}