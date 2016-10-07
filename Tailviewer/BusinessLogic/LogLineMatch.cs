using System;
using System.Text.RegularExpressions;

namespace Tailviewer.BusinessLogic
{
	public struct LogLineMatch : IEquatable<LogLineMatch>
	{
		public readonly int Count;
		public readonly int Index;

		public LogLineMatch(int index, int count)
		{
			Index = index;
			Count = count;
		}

		public LogLineMatch(Match regexMatch)
		{
			Index = regexMatch.Index;
			Count = regexMatch.Length;
		}

		public bool Equals(LogLineMatch other)
		{
			return Index == other.Index && Count == other.Count;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogLineMatch && Equals((LogLineMatch) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Index*397) ^ Count;
			}
		}

		public static bool operator ==(LogLineMatch left, LogLineMatch right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogLineMatch left, LogLineMatch right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format("Match at {0}, #{1}", Index, Count);
		}
	}
}