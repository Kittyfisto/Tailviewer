using System;

namespace Tailviewer.BusinessLogic
{
	public struct LogEntry : IEquatable<LogEntry>
	{
		public readonly LevelFlags Level;
		public readonly string Message;

		public LogEntry(string message, LevelFlags level)
		{
			Message = message;
			Level = level;
		}

		public override string ToString()
		{
			return Message;
		}

		public bool Equals(LogEntry other)
		{
			return string.Equals(Message, other.Message) && Level == other.Level;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogEntry && Equals((LogEntry) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Message != null ? Message.GetHashCode() : 0)*397) ^ (int) Level;
			}
		}

		public static bool operator ==(LogEntry left, LogEntry right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogEntry left, LogEntry right)
		{
			return !left.Equals(right);
		}
	}
}