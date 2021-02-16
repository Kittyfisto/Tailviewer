namespace Tailviewer
{
	/// <summary>
	///     The interface for a class that is interested in receiving changes made to the data exposed by a
	///     <see cref="ILogSource" />.
	/// </summary>
	public interface ILogSourceListener
	{
		/// <summary>
		///     This method is called when a portion of the log file has been modified.
		/// </summary>
		/// <param name="logSource">The log-file that was modified</param>
		/// <param name="section">The section of the log file that was modified</param>
		void OnLogFileModified(ILogSource logSource, LogFileSection section);
	}
}