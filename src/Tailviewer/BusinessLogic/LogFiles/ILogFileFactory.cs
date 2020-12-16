using System.Collections.Generic;
using Tailviewer.Archiver.Plugins.Description;
using Tailviewer.BusinessLogic.Plugins;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	/// 
	/// </summary>
	public interface ILogFileFactory
	{
		/// <summary>
		///     Creates a new log file to represents the given file.
		/// </summary>
		/// <param name="filePath">The full file path to the file to be opened.</param>
		/// <param name="pluginDescription"></param>
		/// <returns></returns>
		ILogFile Open(string filePath, out IPluginDescription pluginDescription);

		/// <summary>
		/// 
		/// </summary>
		IReadOnlyList<ICustomDataSourcePlugin> CustomDataSources { get; }

		ILogFile CreateCustom(CustomDataSourceId id, ICustomDataSourceConfiguration configuration,
		                      out IPluginDescription pluginDescription);
	}
}