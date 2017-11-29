using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Hard-coded columns which are provided by all log files.
	/// </summary>
	public static class LogFileColumns
	{
		/// <summary>
		/// </summary>
		public static readonly ILogFileColumn<string> RawContent;

		static LogFileColumns()
		{
			RawContent = new LogFileColumn<string>("raw_content", "Raw Content");
		}
	}
}