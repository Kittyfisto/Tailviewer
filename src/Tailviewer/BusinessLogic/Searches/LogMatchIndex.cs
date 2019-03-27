using System;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Searches
{
	/// <summary>
	///     Represents one entry in the list of matches.
	///     An entry defines a conctinous part of a single line, <see cref="LogLineMatch" />.
	/// </summary>
	public struct LogMatchIndex
	{
		public static readonly LogMatchIndex Invalid;
		private readonly int _value;

		static LogMatchIndex()
		{
			Invalid = new LogMatchIndex(-1);
		}

		public LogMatchIndex(int value)
		{
			if (value < -1)
				throw new ArgumentOutOfRangeException(nameof(value));

			_value = value;
		}

		public bool Equals(LogMatchIndex other)
		{
			return _value == other._value;
		}

		public static explicit operator int(LogMatchIndex value)
		{
			return value._value;
		}

		public static implicit operator LogMatchIndex(int value)
		{
			return new LogMatchIndex(value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogMatchIndex && Equals((LogMatchIndex) obj);
		}

		public override int GetHashCode()
		{
			return _value;
		}

		public int CompareTo(LogMatchIndex other)
		{
			return _value.CompareTo(other._value);
		}

		public override string ToString()
		{
			if (this == Invalid)
				return "Invalid";

			return string.Format("#{0}", _value);
		}

		public static bool operator <(LogMatchIndex left, LogMatchIndex right)
		{
			return left._value < right._value;
		}

		public static bool operator >(LogMatchIndex left, LogMatchIndex right)
		{
			return left._value > right._value;
		}

		public static bool operator <=(LogMatchIndex left, LogMatchIndex right)
		{
			return left._value <= right._value;
		}

		public static bool operator >=(LogMatchIndex left, LogMatchIndex right)
		{
			return left._value >= right._value;
		}

		public static LogMatchIndex operator +(LogMatchIndex left, int right)
		{
			return new LogMatchIndex(left._value + right);
		}

		public static LogMatchIndex operator +(int left, LogMatchIndex right)
		{
			return new LogMatchIndex(left + right._value);
		}

		public static int operator -(LogMatchIndex left, LogMatchIndex right)
		{
			return left._value - right._value;
		}

		public static bool operator ==(LogMatchIndex left, LogMatchIndex right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogMatchIndex left, LogMatchIndex right)
		{
			return !left.Equals(right);
		}

		public static LogMatchIndex Min(LogMatchIndex left, LogMatchIndex rigth)
		{
			return Math.Min(left._value, rigth._value);
		}

		public static LogMatchIndex Max(LogMatchIndex left, LogMatchIndex right)
		{
			return Math.Max(left._value, right._value);
		}
	}
}