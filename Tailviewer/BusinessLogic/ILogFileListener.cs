namespace Tailviewer.BusinessLogic
{
	public interface ILogFileListener
	{
		/// <summary>
		/// This method is called when a portion of the log file has been modified.
		/// </summary>
		/// <param name="section"></param>
		void OnLogFileModified(LogFileSection section);
	}
}