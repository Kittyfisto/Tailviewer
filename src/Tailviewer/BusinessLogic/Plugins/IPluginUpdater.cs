using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tailviewer.BusinessLogic.Plugins
{
	public interface IPluginUpdater
	{
		/// <summary>
		///     Looks for newer (supported) versions of all currently installed plugins
		///     on the given repository and downloads them.
		/// </summary>
		/// <param name="repositories"></param>
		/// <returns>A task which eventually returns the number of updated plugins.</returns>
		Task<int> UpdatePluginsAsync(IReadOnlyList<string> repositories);
	}
}