using System;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins.Description;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     An <see cref="IPluginLoader" /> implementation which only offers plugins which have previously been
	///     registered with it.
	/// </summary>
	public sealed class PluginRegistry
		: IPluginLoader
	{
		private readonly Dictionary<Type, List<IPlugin>> _pluginsByInterface;

		public PluginRegistry()
		{
			_pluginsByInterface = new Dictionary<Type, List<IPlugin>>();
		}

		public void Register(params IPlugin[] plugins)
		{
			foreach (var plugin in plugins) Register(plugin);
		}

		public void Register(IPlugin plugin)
		{
			var interfaces = FindInterfaces(plugin.GetType());
			foreach (var @interface in interfaces)
			{
				if (!_pluginsByInterface.TryGetValue(@interface, out var plugins))
				{
					plugins = new List<IPlugin>();
					_pluginsByInterface.Add(@interface, plugins);
				}

				plugins.Add(plugin);
			}
		}

		private IEnumerable<Type> FindInterfaces(Type type)
		{
			return type.GetInterfaces().Where(IsPluginInterface).ToList();
		}

		private static bool IsPluginInterface(Type type)
		{
			if (type == typeof(IPlugin))
				return true;

			return type.GetInterfaces().Any(x => x == typeof(IPlugin));
		}

		#region Implementation of IPluginLoader

		public IEnumerable<IPluginDescription> Plugins => Enumerable.Empty<IPluginDescription>();

		public IPluginStatus GetStatus(PluginId id)
		{
			return new PluginStatus();
		}

		public IReadOnlyDictionary<string, Type> ResolveSerializableTypes()
		{
			return new Dictionary<string, Type>();
		}

		public IReadOnlyList<T> LoadAllOfType<T>() where T : class, IPlugin
		{
			if (!_pluginsByInterface.TryGetValue(typeof(T), out var plugins))
				return new T[0];

			return plugins.OfType<T>().ToList();
		}

		public IReadOnlyList<IPluginWithDescription<T>> LoadAllOfTypeWithDescription<T>() where T : class, IPlugin
		{
			return LoadAllOfType<T>().Select(x => new PluginWithDescription<T>(x, null)).ToList();
		}

		#endregion
	}
}