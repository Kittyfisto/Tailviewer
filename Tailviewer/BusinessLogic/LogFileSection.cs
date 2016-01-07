using System;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     Basically a handle to a specific portion of the logfile.
	///     Call <see cref="LogFile.GetSection" /> to actually obtain the data for that portion.
	/// </summary>
	public struct LogFileSection : IEquatable<LogFileSection>
	{
		public readonly int Count;
		public readonly LogEntryIndex Index;

		public static readonly LogFileSection Reset;

		static LogFileSection()
		{
			Reset = new LogFileSection(LogEntryIndex.Invalid, 0);
		}

		public LogFileSection(LogEntryIndex index, int count)
		{
			Index = index;
			Count = count;
		}

		public bool IsEndOfSection(LogEntryIndex index)
		{
			return index >= Index + Count;
		}

		public override string ToString()
		{
			if (Index == LogEntryIndex.Invalid && Count == 0)
				return "Reset";

			return string.Format("[{0}, #{1}]", Index, Count);
		}

		public static LogFileSection MinimumBoundingLine(LogFileSection lhs, LogFileSection rhs)
		{
			var minIndex = LogEntryIndex.Min(lhs.Index, rhs.Index);
			var maxIndex = LogEntryIndex.Max(lhs.Index + lhs.Count, rhs.Index + rhs.Count);
			var count = maxIndex - minIndex;

			return new LogFileSection(minIndex, count);
		}

		public bool Equals(LogFileSection other)
		{
			return Index == other.Index && Count == other.Count;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogFileSection && Equals((LogFileSection) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int)Index*397) ^ Count;
			}
		}

		public static bool operator ==(LogFileSection left, LogFileSection right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogFileSection left, LogFileSection right)
		{
			return !left.Equals(right);
		}
	}
}