using System.Collections.Generic;
using System.Threading.Tasks;
using Tailviewer.Archiver.Repository;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	/// 
	/// </summary>
	[Service]
	public interface IPluginUpdater
	{
		/// <summary>
		///     Looks for newer (supported) versions of all currently installed plugins
		///     on the given repository and downloads them.
		/// </summary>
		/// <param name="repositories"></param>
		/// <returns>A task which eventually returns the number of updated plugins.</returns>
		Task<int> UpdatePluginsAsync(IReadOnlyList<string> repositories);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="repositories"></param>
		/// <returns></returns>
		Task<IReadOnlyList<PublishedPluginDescription>> GetAllPluginsAsync(IReadOnlyList<string> repositories);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="repositories"></param>
		/// <param name="plugin"></param>
		/// <returns></returns>
		Task DownloadPluginAsync(IReadOnlyList<string> repositories, PluginIdentifier plugin);
	}
}