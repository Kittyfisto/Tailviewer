using System.Collections.Generic;

namespace Tailviewer.Archiver.Repository
{
	/// <summary>
	///     Provides (remote) access to Tailviewer's plugin repository.
	/// </summary>
	/// <remarks>
	///     DO NOT MAKE CHANGES TO THIS CLASS ONCE FINALISED.
	///     Doing so will break the communication between tailviewer client
	///     and the repository.
	/// </remarks>
	public interface IPluginRepository
	{
		/// <summary>
		///     Published the given plugin in this repository.
		/// </summary>
		/// <param name="plugin"></param>
		/// <param name="accessToken"></param>
		void PublishPlugin(byte[] plugin, string accessToken);

		/// <summary>
		///     Enumerates all plugins in this repository which are compatible with the given interfaces.
		/// </summary>
		/// <param name="interfaces"></param>
		/// <returns></returns>
		IReadOnlyList<PluginIdentifier> FindAllPluginsFor(IReadOnlyList<PluginInterface> interfaces);

		/// <summary>
		///     Enumerates newer versions of the given plugins which are compatible with the given interfaces.
		/// </summary>
		/// <param name="plugins"></param>
		/// <param name="interfaces"></param>
		/// <returns></returns>
		IReadOnlyList<PluginIdentifier> FindUpdatesFor(IReadOnlyList<PluginIdentifier> plugins, IReadOnlyList<PluginInterface> interfaces);

		/// <summary>
		///     Enumerates all plugins in this repository.
		/// </summary>
		/// <returns></returns>
		IReadOnlyList<PluginIdentifier> FindAllPlugins();

		/// <summary>
		///     Returns descriptions for all of the given plugins.
		/// </summary>
		/// <remarks>
		///     Only returns a description for those plugins which are part of this repository,
		///     otherwise null is returned in that place of the returned list.
		/// </remarks>
		/// <example>
		///     Suppose plugins a, b and c are queried and only plugin a and c are part of this repository.
		///     The returned list will contain 3 entries: {description of a}, {null} and {description of b}.
		/// </example>
		IReadOnlyList<PublishedPluginDescription> GetDescriptions(IReadOnlyList<PluginIdentifier> plugins);

		/// <summary>
		///     Returns icons for all of the given plugins.
		/// </summary>
		/// <remarks>
		///     Only returns icons for those plugins which are part of this repository,
		///     otherwise null is returned in that place of the returned list.
		/// </remarks>
		/// <example>
		///     Suppose plugins a, b and c are queried and only plugin a and c are part of this repository.
		///     The returned list will contain 3 entries: {icon of a}, {null} and {icon of b}.
		/// </example>
		/// <param name="plugins"></param>
		/// <returns></returns>
		IReadOnlyList<byte[]> GetIcons(IReadOnlyList<PluginIdentifier> plugins);

		/// <summary>
		///     Downloads a specific plugin identified by the given name.
		/// </summary>
		/// <param name="pluginId"></param>
		/// <returns></returns>
		byte[] DownloadPlugin(PluginIdentifier pluginId);
	}
}