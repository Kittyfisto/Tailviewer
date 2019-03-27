using System;
using Tailviewer.BusinessLogic;

namespace TablePlayground
{
	/// <summary>
	///     A collection of columns which every log file provides.
	///     It is possible that log files don't provide any data in a column, however.
	/// </summary>
	public sealed class DefaultLogColumns
	{
		public static readonly ILogColumn<DateTime> Timestamp;
		public static readonly ILogColumn<TimeSpan> ElapsedDelta;
		public static readonly ILogColumn<LevelFlags> Level;
		public static readonly ILogColumn<int> LineNumber;
		public static readonly ILogColumn<LogLineIndex> LineIndex;
		public static readonly ILogColumn<int> OriginalLineNumber;
		public static readonly ILogColumn<LogLineIndex> OriginalLineIndex;
		public static readonly ILogColumn<LogLineSourceId> Source;
		public static readonly ILogColumn<string> RawLine;
		public static readonly ILogColumn<string> Logger;
		public static readonly ILogColumn<string> Thread;
		public static readonly ILogColumn<string> Message;

		static DefaultLogColumns()
		{
			Timestamp = new LogColumn<DateTime>();
			ElapsedDelta = new LogColumn<TimeSpan>();
			Level = new LogColumn<LevelFlags>();
			LineNumber = new LogColumn<int>();
			LineIndex = new LogColumn<LogLineIndex>();
			OriginalLineNumber = new LogColumn<int>();
			OriginalLineIndex = new LogColumn<LogLineIndex>();
			Source = new LogColumn<LogLineSourceId>();
			RawLine = new LogColumn<string>();
			Logger = new LogColumn<string>();
			Thread = new LogColumn<string>();
			Message = new LogColumn<string>();
		}
	}
}