using System;

namespace Tailviewer.BusinessLogic.Searches
{
	/// <summary>
	///     Represents a section in the list of search matches.
	/// </summary>
	public readonly struct LogMatchSection
	{
		public static readonly LogMatchSection Reset;
		public readonly int Count;
		public readonly LogMatchIndex Index;
		public readonly bool InvalidateSection;

		static LogMatchSection()
		{
			Reset = new LogMatchSection(LogMatchIndex.Invalid, 0);
		}

		public LogMatchSection(LogMatchIndex index, int count, bool invalidateSection = false)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			Index = index;
			Count = count;
			InvalidateSection = invalidateSection;
		}

		public bool IsReset
		{
			get { return this == Reset; }
		}

		public int LastIndex
		{
			get { return Index + Count - 1; }
		}

		public bool Equals(LogMatchSection other)
		{
			return Index == other.Index && Count == other.Count;
		}

		public bool IsEndOfSection(LogMatchIndex index)
		{
			return index >= Index + Count;
		}

		public override string ToString()
		{
			if (Index == LogMatchIndex.Invalid && Count == 0)
				return "Reset";

			if (InvalidateSection)
				return string.Format("Invalidated [{0}, #{1}]", Index, Count);

			return string.Format("Changed [{0}, #{1}]", Index, Count);
		}

		public static LogMatchSection MinimumBoundingLine(LogMatchSection lhs, LogMatchSection rhs)
		{
			LogMatchIndex minIndex = LogMatchIndex.Min(lhs.Index, rhs.Index);
			LogMatchIndex maxIndex = LogMatchIndex.Max(lhs.Index + lhs.Count, rhs.Index + rhs.Count);
			int count = maxIndex - minIndex;

			return new LogMatchSection(minIndex, count);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogMatchSection && Equals((LogMatchSection) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int) Index*397) ^ Count;
			}
		}

		public static bool operator ==(LogMatchSection left, LogMatchSection right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogMatchSection left, LogMatchSection right)
		{
			return !left.Equals(right);
		}
	}
}