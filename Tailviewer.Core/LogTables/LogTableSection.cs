using System;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogTables
{
	/// <summary>
	///     Marks a section of a log table.
	///     Contrary to <see cref="LogFileSection" />, a log table section always marks entire entries and not individual lines.
	/// </summary>
	public struct LogTableSection
		: IEquatable<LogTableSection>
	{
		public readonly int Count;
		public readonly LogEntryIndex Index;

		public LogTableSection(LogEntryIndex index, int count)
		{
			Index = index;
			Count = count;
		}

		public bool Equals(LogTableSection other)
		{
			return Count == other.Count && Index.Equals(other.Index);
		}

		public override string ToString()
		{
			return string.Format("[{0}, #{1}]", Index, Count);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogTableSection && Equals((LogTableSection) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Count*397) ^ Index.GetHashCode();
			}
		}

		public static bool operator ==(LogTableSection left, LogTableSection right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogTableSection left, LogTableSection right)
		{
			return !left.Equals(right);
		}
	}
}