using System;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Hard-coded columns which are provided by all log files.
	/// </summary>
	public static class LogFileColumns
	{
		/// <summary>
		///     The index of the log entry, from 0 to the number of log entries.
		/// </summary>
		/// <remarks>
		///     Change property to <see cref="LogEntryIndex" /> once <see cref="LogLineIndex" />
		///     has been removed.
		/// </remarks>
		public static readonly ILogFileColumn<LogLineIndex> Index;

		/// <summary>
		///     The index of the log entry another one was created from.
		///     Only differs from <see cref="Index" /> when the log entry has been created
		///     through operations such as filtering, merging, etc...
		/// </summary>
		/// <remarks>
		///     Change property to <see cref="LogEntryIndex" /> once <see cref="LogLineIndex" />
		///     has been removed.
		/// </remarks>
		public static readonly ILogFileColumn<LogLineIndex> OriginalIndex;

		/// <summary>
		/// </summary>
		public static readonly ILogFileColumn<string> RawContent;

		/// <summary>
		///     The amount of time between this and the last log entry.
		/// </summary>
		public static readonly ILogFileColumn<TimeSpan?> DeltaTime;

		/// <summary>
		///     The absolute timestamp of when the log entry was produced.
		/// </summary>
		public static readonly ILogFileColumn<DateTime?> TimeStamp;

		static LogFileColumns()
		{
			Index = new LogFileColumn<LogLineIndex>("index", "Index");
			OriginalIndex = new LogFileColumn<LogLineIndex>("original_index", "Original Index");
			RawContent = new LogFileColumn<string>("raw_content", "Raw Content");
			DeltaTime = new LogFileColumn<TimeSpan?>("delta_time", "Delta Time");
			TimeStamp = new LogFileColumn<DateTime?>("timestamp", "Timestamp");
		}
	}
}