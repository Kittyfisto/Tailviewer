using System.Collections.Generic;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPluginLoader
	{
		/// <summary>
		/// 
		/// </summary>
		IEnumerable<IPluginDescription> Plugins { get; }

		/// <summary>
		///     Loads all plugins in the given list that implement the given interface <typeparamref name="T" />.
		///     All plugins that cannot be loaded are ignored (an error is logged, but the caller is not notified).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IReadOnlyList<T> LoadAllOfType<T>() where T : class, IPlugin;
	}
}