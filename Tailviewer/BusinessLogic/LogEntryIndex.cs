using System;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     Represents an index to a specific log entry in a log.
	///     Usually there is a 1 to 1 correlation between log entry and log line, but this doesn't need to be the case.
	/// </summary>
	public struct LogEntryIndex
		: IEquatable<LogEntryIndex>
		  , IComparable<LogEntryIndex>
	{
		public static readonly LogEntryIndex Invalid;
		private readonly int _value;

		static LogEntryIndex()
		{
			Invalid = new LogEntryIndex(-1);
		}

		public LogEntryIndex(int value)
		{
			if (value < -1)
				throw new ArgumentOutOfRangeException("value");

			_value = value;
		}

		public int Value
		{
			get { return _value; }
		}

		public int CompareTo(LogEntryIndex other)
		{
			return _value.CompareTo(other._value);
		}

		public bool Equals(LogEntryIndex other)
		{
			return _value == other._value;
		}

		public static explicit operator int(LogEntryIndex value)
		{
			return value._value;
		}

		public static implicit operator LogEntryIndex(int value)
		{
			return new LogEntryIndex(value);
		}

		public static LogEntryIndex operator ++(LogEntryIndex value)
		{
			return new LogEntryIndex(value._value + 1);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogEntryIndex && Equals((LogEntryIndex) obj);
		}

		public override int GetHashCode()
		{
			return _value;
		}

		public override string ToString()
		{
			if (this == Invalid)
				return "Invalid";

			return string.Format("#{0}", _value);
		}

		public static bool operator <(LogEntryIndex left, LogEntryIndex right)
		{
			return left._value < right._value;
		}

		public static bool operator >(LogEntryIndex left, LogEntryIndex right)
		{
			return left._value > right._value;
		}

		public static bool operator <=(LogEntryIndex left, LogEntryIndex right)
		{
			return left._value <= right._value;
		}

		public static bool operator >=(LogEntryIndex left, LogEntryIndex right)
		{
			return left._value >= right._value;
		}

		public static LogEntryIndex operator +(LogEntryIndex left, int right)
		{
			return new LogEntryIndex(left._value + right);
		}

		public static LogEntryIndex operator +(int left, LogEntryIndex right)
		{
			return new LogEntryIndex(left + right._value);
		}

		public static int operator -(LogEntryIndex left, LogEntryIndex right)
		{
			return left._value - right._value;
		}

		public static bool operator ==(LogEntryIndex left, LogEntryIndex right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogEntryIndex left, LogEntryIndex right)
		{
			return !left.Equals(right);
		}

		public static LogEntryIndex Min(LogEntryIndex left, LogEntryIndex rigth)
		{
			return Math.Min(left._value, rigth._value);
		}

		public static LogEntryIndex Max(LogEntryIndex left, LogEntryIndex right)
		{
			return Math.Max(left._value, right._value);
		}
	}
}