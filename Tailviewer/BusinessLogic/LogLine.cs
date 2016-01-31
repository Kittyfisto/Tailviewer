using System;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     Represents a single line (terminated by \n or \r\n) of the data source (log file).
	/// </summary>
	public struct LogLine : IEquatable<LogLine>
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly LevelFlags Level;

		/// <summary>
		///     The index of this line in its data source.
		/// </summary>
		public readonly int LineIndex;

		/// <summary>
		///     The index of the log entry this line belongs to.
		/// </summary>
		public readonly int LogEntryIndex;

		public readonly string Message;

		/// <summary>
		///     The timestamp associated with this log-entry.
		/// </summary>
		public readonly DateTime? Timestamp;

		public LogLine(int lineIndex, string message, LevelFlags level)
			: this(lineIndex, lineIndex, message, level)
		{
		}

		public LogLine(int lineIndex, string message, LevelFlags level, DateTime? timestamp)
			: this(lineIndex, lineIndex, message, level, timestamp)
		{}

		public LogLine(int lineIndex, int logEntryIndex, string message, LevelFlags level)
			: this(lineIndex, logEntryIndex, message, level, null)
		{
		}

		public LogLine(int lineIndex, int logEntryIndex, string message, LevelFlags level, DateTime? timestamp)
		{
			LineIndex = lineIndex;
			Message = message;
			Level = level;
			LogEntryIndex = logEntryIndex;
			Timestamp = timestamp;
		}

		public bool Equals(LogLine other)
		{
			return Level == other.Level && LineIndex == other.LineIndex && LogEntryIndex == other.LogEntryIndex &&
			       string.Equals(Message, other.Message) &&
			       Equals(Timestamp, other.Timestamp);
		}

		public override string ToString()
		{
			return string.Format("#{0} (#{1}): {2}", LineIndex, LogEntryIndex, Message);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogLine && Equals((LogLine) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (int) Level;
				hashCode = (hashCode*397) ^ LineIndex;
				hashCode = (hashCode*397) ^ LogEntryIndex;
				hashCode = (hashCode*397) ^ (Message != null ? Message.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ Timestamp.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(LogLine left, LogLine right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogLine left, LogLine right)
		{
			return !left.Equals(right);
		}
	}
}