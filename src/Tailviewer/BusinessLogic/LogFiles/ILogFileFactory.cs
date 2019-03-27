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
		/// <param name="fileName">The full file path to the file to be opened.</param>
		/// <returns></returns>
		ILogFile Open(string fileName);
	}
}