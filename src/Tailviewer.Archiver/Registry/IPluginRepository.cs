using System.Collections.Generic;

namespace Tailviewer.Archiver.Registry
{
	/// <summary>
	///     Provides (remote) access to Tailviewer's plugin repository.
	/// </summary>
	/// <remarks>
	///     This interface defines all methods which are remoted via SharpRemote.
	///     Do not make backwards-incompatible changes to this interface of its used
	///     types!
	/// </remarks>
	public interface IPluginRepository
	{
		/// <summary>
		///     Enumerates all plugins in this repository which are implemented against the given list of interfaces.
		/// </summary>
		/// <param name="interfaces"></param>
		/// <returns></returns>
		IReadOnlyList<PluginRegistryId> FindAllPluginsFor(IReadOnlyList<PluginInterface> interfaces);

		/// <summary>
		///     Enumerates all plugins in this repository.
		/// </summary>
		/// <returns></returns>
		IReadOnlyList<PluginRegistryId> FindAllPlugins();

		/// <summary>
		///     Downloads a specific plugin identified by the given name.
		/// </summary>
		/// <param name="pluginId"></param>
		/// <returns></returns>
		byte[] DownloadPlugin(PluginRegistryId pluginId);
	}
}