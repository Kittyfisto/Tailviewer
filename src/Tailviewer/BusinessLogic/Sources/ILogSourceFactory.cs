using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.Core.Sources;
using Tailviewer.Plugins;

namespace Tailviewer.BusinessLogic.Sources
{
	/// <summary>
	///     Responsible for creating log sources.
	/// </summary>
	public interface ILogSourceFactoryEx
		: ILogSourceFactory
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="configuration"></param>
		/// <param name="pluginDescription"></param>
		/// <returns></returns>
		ILogSource CreateCustom(CustomDataSourceId id,
		                        ICustomDataSourceConfiguration configuration,
		                        out IPluginDescription pluginDescription);
	}
}