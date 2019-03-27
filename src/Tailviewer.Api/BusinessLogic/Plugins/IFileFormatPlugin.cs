using System.Collections.Generic;
using System.Threading;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.Plugins
{
	/// <summary>
	///     This interface may be implemented to add support for custom log file formats which are not simple
	///     text based files.
	/// </summary>
	public interface IFileFormatPlugin
		: IPlugin
	{
		/// <summary>
		///     The list of file extensions which can be supported by this plugin.
		/// </summary>
		/// <remarks>
		///     May or may not contain the starting point.
		///     Examples: ".exe", "exe", ".foo.bar", "my super custom file.blob"
		/// </remarks>
		/// <remarks>
		///     Tailviewer will always perform a non-case senstive search and test if the complete file name ends with any of the
		///     given extensions. It is therefor allowed to return a complete file name in case the plugin should not support every
		///     file with a given extension, but just a specifically named one.
		/// </remarks>
		IReadOnlyList<string> SupportedExtensions { get; }

		/// <summary>
		///     Creates a new log file to represent the given file.
		/// </summary>
		/// <remarks>
		///     It is expected an implementation of this method DOES NOT BLOCK until the entire file is read into memory
		///     (which would lead to an incredibly horrible user experience). Instead, the given <see cref="ITaskScheduler" />
		///     shall be used to start a new task <see cref="ITaskScheduler.StartPeriodic(System.Action,System.TimeSpan,string)" />
		///     in order to read the file in a background thread, while continuously reporting changes to all of the log file's
		///     listeners via <see cref="ILogFileListener.OnLogFileModified" />.
		/// </remarks>
		/// <remarks>
		///     If this method throws, then it is assumed that an error occured, which is reported to the user.
		///     If this method returns null, then no error is shown the user.
		/// </remarks>
		/// <param name="fileName">The full file path to the file to be opened.</param>
		/// <param name="taskScheduler">A task scheduler that may be used to perform </param>
		/// <returns></returns>
		ILogFile Open(string fileName, ITaskScheduler taskScheduler);
	}
}