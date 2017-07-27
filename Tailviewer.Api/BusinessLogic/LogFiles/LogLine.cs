using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;

namespace Tailviewer.BusinessLogic.LogFiles
{
	/// <summary>
	///     Represents a single line (terminated by \n or \r\n) of the data source (log file).
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LogLine : IEquatable<LogLine>
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

		/// <summary>
		///     The message belonging to this log line.
		/// </summary>
		/// <remarks>
		///     Will never contain more than one line.
		/// </remarks>
		public readonly string Message;

		/// <summary>
		///     The timestamp associated with this log-entry.
		/// </summary>
		public readonly DateTime? Timestamp;

		/// <summary>
		///     A bitmask which describes the filters that caused this log line to be visible.
		/// </summary>
		/// <remarks>
		///     As a result, only up to 32 filters may be used at one time.
		/// </remarks>
		public readonly int MatchedFilters;

		/// <summary>
		///     Initializes this log line.
		/// </summary>
		/// <remarks>
		///     ONLY <paramref name="message" /> will be displayed to the user.
		///     All other parameters are meta information for filtering and merging multiple data sources, but
		///     ARE NOT DISPLAYED ON THEIR OWN.
		/// </remarks>
		/// <param name="lineIndex"></param>
		/// <param name="message">The message as it will be displayed to the user</param>
		/// <param name="level"></param>
		[DebuggerStepThrough]
		public LogLine(int lineIndex, string message, LevelFlags level)
			: this(lineIndex, lineIndex, message, level)
		{
		}

		/// <summary>
		///     Initializes this log line.
		/// </summary>
		/// <remarks>
		///     ONLY <paramref name="message" /> will be displayed to the user.
		///     All other parameters are meta information for filtering and merging multiple data sources, but
		///     ARE NOT DISPLAYED ON THEIR OWN.
		/// </remarks>
		/// <param name="lineIndex"></param>
		/// <param name="message">The message as it will be displayed to the user</param>
		/// <param name="level"></param>
		/// <param name="timestamp"></param>
		[DebuggerStepThrough]
		public LogLine(int lineIndex, string message, LevelFlags level, DateTime? timestamp)
			: this(lineIndex, lineIndex, message, level, timestamp)
		{
		}

		/// <summary>
		///     Initializes this log line.
		/// </summary>
		/// <remarks>
		///     ONLY <paramref name="message" /> will be displayed to the user.
		///     All other parameters are meta information for filtering and merging multiple data sources, but
		///     ARE NOT DISPLAYED ON THEIR OWN.
		/// </remarks>
		/// <param name="lineIndex"></param>
		/// <param name="logEntryIndex"></param>
		/// <param name="message">The message as it will be displayed to the user</param>
		/// <param name="level"></param>
		[DebuggerStepThrough]
		public LogLine(int lineIndex, int logEntryIndex, string message, LevelFlags level)
			: this(lineIndex, logEntryIndex, message, level, null)
		{
		}

		/// <summary>
		///     Initializes this log line.
		/// </summary>
		/// <remarks>
		///     ONLY <paramref name="message" /> will be displayed to the user.
		///     All other parameters are meta information for filtering and merging multiple data sources, but
		///     ARE NOT DISPLAYED ON THEIR OWN.
		/// </remarks>
		/// <param name="lineIndex"></param>
		/// <param name="originalLineIndex"></param>
		/// <param name="logEntryIndex"></param>
		/// <param name="message">The message as it will be displayed to the user</param>
		/// <param name="level"></param>
		[DebuggerStepThrough]
		public LogLine(int lineIndex, int originalLineIndex, int logEntryIndex, string message, LevelFlags level)
			: this(lineIndex, originalLineIndex, logEntryIndex, message, level, null)
		{
		}

		/// <summary>
		///     Initializes this log line.
		/// </summary>
		/// <remarks>
		///     ONLY <paramref name="message" /> will be displayed to the user.
		///     All other parameters are meta information for filtering and merging multiple data sources, but
		///     ARE NOT DISPLAYED ON THEIR OWN.
		/// </remarks>
		/// <param name="lineIndex"></param>
		/// <param name="logEntryIndex"></param>
		/// <param name="message">The message as it will be displayed to the user</param>
		/// <param name="level"></param>
		/// <param name="timestamp"></param>
		[DebuggerStepThrough]
		public LogLine(int lineIndex, int logEntryIndex, string message, LevelFlags level, DateTime? timestamp)
			: this(lineIndex, lineIndex, logEntryIndex, message, level, timestamp)
		{}

		/// <summary>
		///     Initializes this log line.
		/// </summary>
		/// <remarks>
		///     ONLY <paramref name="message" /> will be displayed to the user.
		///     All other parameters are meta information for filtering and merging multiple data sources, but
		///     ARE NOT DISPLAYED ON THEIR OWN.
		/// </remarks>
		/// <param name="lineIndex"></param>
		/// <param name="logEntryIndex"></param>
		/// <param name="message">The message as it will be displayed to the user</param>
		/// <param name="level"></param>
		/// <param name="timestamp"></param>
		[DebuggerStepThrough]
		public LogLine(LogLineIndex lineIndex, LogEntryIndex logEntryIndex, string message, LevelFlags level, DateTime? timestamp)
			: this((int)lineIndex, (int)logEntryIndex, message, level, timestamp)
		{}

		/// <summary>
		///     Initializes this log line.
		/// </summary>
		/// <param name="lineIndex"></param>
		/// <param name="logEntryIndex"></param>
		/// <param name="line"></param>
		[DebuggerStepThrough]
		public LogLine(int lineIndex, int logEntryIndex, LogLine line)
			: this(lineIndex, logEntryIndex, line.Message, line.Level, line.Timestamp)
		{}

		/// <summary>
		///     Initializes this log line.
		/// </summary>
		/// <param name="lineIndex"></param>
		/// <param name="logEntryIndex"></param>
		/// <param name="line"></param>
		[DebuggerStepThrough]
		public LogLine(LogLineIndex lineIndex, LogEntryIndex logEntryIndex, LogLine line)
			: this((int)lineIndex, (int)logEntryIndex, line.Message, line.Level, line.Timestamp)
		{}

		/// <summary>
		///     Initializes this log line.
		/// </summary>
		/// <remarks>
		///     ONLY <paramref name="message" /> will be displayed to the user.
		///     All other parameters are meta information for filtering and merging multiple data sources, but
		///     ARE NOT DISPLAYED ON THEIR OWN.
		/// </remarks>
		/// <param name="lineIndex"></param>
		/// <param name="originalLineIndex"></param>
		/// <param name="logEntryIndex"></param>
		/// <param name="message">The message as it will be displayed to the user</param>
		/// <param name="level"></param>
		/// <param name="timestamp"></param>
		[DebuggerStepThrough]
		public LogLine(int lineIndex, int originalLineIndex, int logEntryIndex, string message, LevelFlags level, DateTime? timestamp)
			: this(lineIndex, originalLineIndex, logEntryIndex, message, level, timestamp, 0)
		{}

		/// <summary>
		///     Initializes this log line.
		/// </summary>
		/// <remarks>
		///     ONLY <paramref name="message" /> will be displayed to the user.
		///     All other parameters are meta information for filtering and merging multiple data sources, but
		///     ARE NOT DISPLAYED ON THEIR OWN.
		/// </remarks>
		/// <param name="lineIndex"></param>
		/// <param name="originalLineIndex"></param>
		/// <param name="logEntryIndex"></param>
		/// <param name="message">The message as it will be displayed to the user</param>
		/// <param name="level"></param>
		/// <param name="timestamp"></param>
		/// <param name="matchedFilters"></param>
		[DebuggerStepThrough]
		public LogLine(int lineIndex, int originalLineIndex, int logEntryIndex, string message, LevelFlags level, DateTime? timestamp, int matchedFilters)
		{
			LineIndex = lineIndex;
			OriginalLineIndex = originalLineIndex;
			Message = message;
			Level = level;
			LogEntryIndex = logEntryIndex;
			Timestamp = timestamp;
			MatchedFilters = matchedFilters;
		}

		/// <summary>
		///     Compares this log line against the given one.
		/// </summary>
		/// <param name="other">The other log line to compare against this one</param>
		/// <returns>True when this and the given log line have the same values in all publicly visible properties</returns>
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

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("#{0} (#{1}): {2}", LineIndex, LogEntryIndex, Message);
		}

		/// <summary>
		///     Compares this log line against the given one.
		/// </summary>
		/// <param name="obj">The other log line to compare against this one</param>
		/// <returns>True when this and the given log line have the same values in all publicly visible properties</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogLine && Equals((LogLine) obj);
		}

		/// <inheritdoc />
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

		/// <summary>
		///     Compares the two given log lines for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogLine left, LogLine right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two given log lines for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
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