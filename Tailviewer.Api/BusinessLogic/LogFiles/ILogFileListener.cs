namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     The interface for a class that is interested in receiving changes made to the data exposed by a
	///     <see cref="ILogFile" />.
	/// </summary>
	public interface ILogFileListener
	{
		/// <summary>
		///     This method is called when a portion of the log file has been modified.
		/// </summary>
		/// <param name="logFile">The log-file that was modified</param>
		/// <param name="section">The section of the log file that was modified</param>
		void OnLogFileModified(ILogFile logFile, LogFileSection section);
	}
}