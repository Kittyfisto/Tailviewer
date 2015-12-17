using System;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     Represents a single line (terminated by \n or \r\n) of the data source (log file).
	/// </summary>
	public struct LogLine : IEquatable<LogLine>
	{
		public readonly LevelFlags Level;
		public readonly uint LineIndex;
		public readonly string Message;

		public LogLine(uint lineIndex, string message, LevelFlags level)
		{
			LineIndex = lineIndex;
			Message = message;
			Level = level;
		}

		public bool Equals(LogLine other)
		{
			return Level == other.Level && LineIndex == other.LineIndex && string.Equals(Message, other.Message);
		}

		public override string ToString()
		{
			return string.Format("#{0}: {1}", LineIndex, Message);
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
				hashCode = (hashCode*397) ^ (int) LineIndex;
				hashCode = (hashCode*397) ^ (Message != null ? Message.GetHashCode() : 0);
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