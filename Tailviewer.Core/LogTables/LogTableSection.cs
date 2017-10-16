using System;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogTables
{
	/// <summary>
	///     Marks a section of a log table.
	///     Contrary to <see cref="LogFileSection" />, a log table section always marks entire entries and not individual
	///     lines.
	/// </summary>
	public struct LogTableSection
		: IEquatable<LogTableSection>
	{
		/// <summary>
		///     The first index of the section represented by this object.
		/// </summary>
		public readonly LogEntryIndex Index;

		/// <summary>
		///     The number of entries represented by this object.
		/// </summary>
		public readonly int Count;

		/// <summary>
		///     Initializes this section.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		public LogTableSection(LogEntryIndex index, int count)
		{
			Index = index;
			Count = count;
		}

		/// <inheritdoc />
		public bool Equals(LogTableSection other)
		{
			return Count == other.Count && Index.Equals(other.Index);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("[{0}, #{1}]", Index, Count);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is LogTableSection && Equals((LogTableSection) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (Count * 397) ^ Index.GetHashCode();
			}
		}

		/// <summary>
		///     Compares the two sections for equality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogTableSection left, LogTableSection right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Compares the two sections for inequality.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogTableSection left, LogTableSection right)
		{
			return !left.Equals(right);
		}
	}
}