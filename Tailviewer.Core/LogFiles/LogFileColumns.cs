using System;
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

		/// <summary>
		/// The amount of time between this and the last log entry.
		/// </summary>
		public static readonly ILogFileColumn<TimeSpan?> DeltaTime;

		/// <summary>
		/// The absolute timestamp of when the log entry was produced.
		/// </summary>
		public static readonly ILogFileColumn<DateTime?> TimeStamp;

		static LogFileColumns()
		{
			RawContent = new LogFileColumn<string>("raw_content", "Raw Content");
			DeltaTime = new LogFileColumn<TimeSpan?>("delta_time", "Delta Time");
			TimeStamp = new LogFileColumn<DateTime?>("timestamp", "Timestamp");
		}
	}
}