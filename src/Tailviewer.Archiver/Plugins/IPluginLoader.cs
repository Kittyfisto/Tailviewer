using System;
using System.Collections.Generic;
using Tailviewer.Api;
using Tailviewer.Archiver.Plugins.Description;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	/// 
	/// </summary>
	[Service]
	public interface IPluginLoader
	{
		/// <summary>
		/// The list of installed plugins.
		/// </summary>
		IEnumerable<IPluginDescription> Plugins { get; }

		/// <summary>
		/// Obtains status information for that particular plugin.
		/// Includes potential errors, etc...
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		IPluginStatus GetStatus(PluginId id);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		IReadOnlyDictionary<string, Type> ResolveSerializableTypes();

		/// <summary>
		///     Loads all plugins in the given list that implement the given interface <typeparamref name="T" />.
		///     All plugins that cannot be loaded are ignored (an error is logged, but the caller is not notified).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IReadOnlyList<T> LoadAllOfType<T>() where T : class, IPlugin;

		/// <summary>
		///     Loads all plugins in the given list that implement the given interface <typeparamref name="T" />.
		///     All plugins that cannot be loaded are ignored (an error is logged, but the caller is not notified).
		/// </summary>
		/// <remarks>
		///     Compared to <see cref="LoadAllOfType{T}"/>, this method not only loads and returns the plugins,
		///     but also contains the description of the plugin.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IReadOnlyList<IPluginWithDescription<T>> LoadAllOfTypeWithDescription<T>() where T : class, IPlugin;
	}
}