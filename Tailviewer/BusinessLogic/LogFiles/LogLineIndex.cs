using System;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public struct LogLineIndex
		: IEquatable<LogLineIndex>
		, IComparable<LogLineIndex>
	{
		public static readonly LogLineIndex Invalid;
		private readonly int _value;

		static LogLineIndex()
		{
			Invalid = new LogLineIndex(-1);
		}

		public LogLineIndex(int value)
		{
			if (value < -1)
				throw new ArgumentOutOfRangeException("value");

			_value = value;
		}

		public bool Equals(LogLineIndex other)
		{
			return _value == other._value;
		}

		public static explicit operator int(LogLineIndex value)
		{
			return value._value;
		}

		public static implicit operator LogLineIndex(int value)
		{
			return new LogLineIndex(value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogLineIndex && Equals((LogLineIndex) obj);
		}

		public override int GetHashCode()
		{
			return _value;
		}

		public int CompareTo(LogLineIndex other)
		{
			return _value.CompareTo(other._value);
		}

		public override string ToString()
		{
			if (this == Invalid)
				return "Invalid";

			return string.Format("#{0}", _value);
		}

		public static bool operator <(LogLineIndex left, LogLineIndex right)
		{
			return left._value < right._value;
		}

		public static bool operator >(LogLineIndex left, LogLineIndex right)
		{
			return left._value > right._value;
		}

		public static bool operator <=(LogLineIndex left, LogLineIndex right)
		{
			return left._value <= right._value;
		}

		public static bool operator >=(LogLineIndex left, LogLineIndex right)
		{
			return left._value >= right._value;
		}

		public static LogLineIndex operator +(LogLineIndex left, int right)
		{
			return new LogLineIndex(left._value + right);
		}

		public static LogLineIndex operator +(int left, LogLineIndex right)
		{
			return new LogLineIndex(left + right._value);
		}

		public static int operator -(LogLineIndex left, LogLineIndex right)
		{
			return left._value - right._value;
		}

		public static bool operator ==(LogLineIndex left, LogLineIndex right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogLineIndex left, LogLineIndex right)
		{
			return !left.Equals(right);
		}

		public static LogLineIndex Min(LogLineIndex left, LogLineIndex rigth)
		{
			return Math.Min(left._value, rigth._value);
		}

		public static LogLineIndex Max(LogLineIndex left, LogLineIndex right)
		{
			return Math.Max(left._value, right._value);
		}
	}
}