using Tailviewer.Archiver.Plugins.Description;

namespace Tailviewer.BusinessLogic.Sources
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
		ILogSource Open(string filePath, out IPluginDescription pluginDescription);
	}
}