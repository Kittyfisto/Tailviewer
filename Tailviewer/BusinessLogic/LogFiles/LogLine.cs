using System;
using System.Diagnostics;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Represents a single line (terminated by \n or \r\n) of the data source (log file).
	/// </summary>
	public struct LogLine : IEquatable<LogLine>
	{
		/// <summary>
		/// </summary>
		public readonly LevelFlags Level;

		/// <summary>
		///     The index of this line in its data source.
		/// </summary>
		public readonly int LineIndex;

		/// <summary>
		/// 
		/// </summary>
		public readonly int OriginalLineIndex;

		/// <summary>
		///     The index of the log entry this line belongs to.
		/// </summary>
		public readonly int LogEntryIndex;

		public readonly string Message;

		/// <summary>
		///     The timestamp associated with this log-entry.
		/// </summary>
		public readonly DateTime? Timestamp;

		[DebuggerStepThrough]
		public LogLine(int lineIndex, string message, LevelFlags level)
			: this(lineIndex, lineIndex, message, level)
		{
		}

		[DebuggerStepThrough]
		public LogLine(int lineIndex, string message, LevelFlags level, DateTime? timestamp)
			: this(lineIndex, lineIndex, message, level, timestamp)
		{
		}

		[DebuggerStepThrough]
		public LogLine(int lineIndex, int logEntryIndex, string message, LevelFlags level)
			: this(lineIndex, logEntryIndex, message, level, null)
		{
		}

		[DebuggerStepThrough]
		public LogLine(int lineIndex, int originalLineIndex, int logEntryIndex, string message, LevelFlags level)
			: this(lineIndex, originalLineIndex, logEntryIndex, message, level, null)
		{
		}

		[DebuggerStepThrough]
		public LogLine(int lineIndex, int logEntryIndex, string message, LevelFlags level, DateTime? timestamp)
			: this(lineIndex, lineIndex, logEntryIndex, message, level, timestamp)
		{}

		[DebuggerStepThrough]
		public LogLine(LogLineIndex lineIndex, LogEntryIndex logEntryIndex, string message, LevelFlags level, DateTime? timestamp)
			: this((int)lineIndex, (int)logEntryIndex, message, level, timestamp)
		{}

		[DebuggerStepThrough]
		public LogLine(int lineIndex, int logEntryIndex, LogLine line)
			: this(lineIndex, logEntryIndex, line.Message, line.Level, line.Timestamp)
		{}

		[DebuggerStepThrough]
		public LogLine(LogLineIndex lineIndex, LogEntryIndex logEntryIndex, LogLine line)
			: this((int)lineIndex, (int)logEntryIndex, line.Message, line.Level, line.Timestamp)
		{}

		[DebuggerStepThrough]
		public LogLine(int lineIndex, int originalLineIndex, int logEntryIndex, string message, LevelFlags level, DateTime? timestamp)
		{
			LineIndex = lineIndex;
			OriginalLineIndex = originalLineIndex;
			Message = message;
			Level = level;
			LogEntryIndex = logEntryIndex;
			Timestamp = timestamp;
		}

		public bool Equals(LogLine other)
		{
			if (Level != other.Level)
				return false;

			if (LineIndex != other.LineIndex)
				return false;

			if (LogEntryIndex != other.LogEntryIndex)
				return false;

			if (!string.Equals(Message, other.Message))
				return false;

			if (!Equals(Timestamp, other.Timestamp))
				return false;

			return true;
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
				hashCode = (hashCode*397) ^ (Message?.GetHashCode() ?? 0);
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

		public static LevelFlags DetermineLevelsFromLine(string line)
		{
			LevelFlags unused;
			return DetermineLevelsFromLine(line, out unused);
		}

		public static LevelFlags DetermineLevelsFromLine(string line, out LevelFlags leftMost)
		{
			LevelFlags level = LevelFlags.None;
			leftMost = LevelFlags.None;
			int index = int.MaxValue;

			if (line == null)
				return level;

			var idx = line.IndexOf("FATAL", StringComparison.InvariantCulture);
			if (idx != -1)
			{
				if (idx < index)
				{
					leftMost = LevelFlags.Fatal;
					index = idx;
				}
			}

			idx = line.IndexOf("ERROR", StringComparison.InvariantCulture);
			if (idx != -1)
			{
				level |= LevelFlags.Error;
				if (idx < index)
				{
					leftMost = LevelFlags.Error;
					index = idx;
				}
			}

			idx = line.IndexOf("WARN", StringComparison.InvariantCulture);
			if (idx != -1)
			{
				level |= LevelFlags.Warning;
				if (idx < index)
				{
					leftMost = LevelFlags.Warning;
					index = idx;
				}
			}

			idx = line.IndexOf("INFO", StringComparison.InvariantCulture);
			if (idx != -1)
			{
				level |= LevelFlags.Info;
				if (idx < index)
				{
					leftMost = LevelFlags.Info;
					index = idx;
				}
			}

			idx = line.IndexOf("DEBUG", StringComparison.InvariantCulture);
			if (idx != -1)
			{
				level |= LevelFlags.Debug;
				if (idx < index)
				{
					leftMost = LevelFlags.Debug;
					index = idx;
				}
			}

			return level;
		}

		public static LevelFlags DetermineLevelFromLine(string line)
		{
			LevelFlags leftMost;
			DetermineLevelsFromLine(line, out leftMost);
			return leftMost;
		}
	}
}