using System;

namespace Tailviewer.Core.LogFiles.Merged
{
	/// <summary>
	///     A single entry in the <see cref="MergedLogEntryIndex" /> structure.
	/// </summary>
	internal struct MergedLogLineIndex : IEquatable<MergedLogLineIndex>
	{
		public readonly int OriginalLogEntryIndex;
		public readonly int SourceLineIndex;
		public readonly byte LogFileIndex;
		public readonly DateTime Timestamp;
		public int MergedLogEntryIndex;

		#region Equality members

		public bool Equals(MergedLogLineIndex other)
		{
			return OriginalLogEntryIndex == other.OriginalLogEntryIndex && SourceLineIndex == other.SourceLineIndex &&
			       LogFileIndex == other.LogFileIndex && Timestamp.Equals(other.Timestamp) &&
			       MergedLogEntryIndex == other.MergedLogEntryIndex;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			return obj is MergedLogLineIndex && Equals((MergedLogLineIndex) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = OriginalLogEntryIndex;
				hashCode = (hashCode * 397) ^ SourceLineIndex;
				hashCode = (hashCode * 397) ^ LogFileIndex.GetHashCode();
				hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
				hashCode = (hashCode * 397) ^ MergedLogEntryIndex;
				return hashCode;
			}
		}

		public static bool operator ==(MergedLogLineIndex left, MergedLogLineIndex right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MergedLogLineIndex left, MergedLogLineIndex right)
		{
			return !left.Equals(right);
		}

		#endregion

		public MergedLogLineIndex(int sourceLineIndex,
		                          int mergedLogEntryIndex,
		                          int originalLogEntryIndex,
		                          byte logFileIndex)
		{
			SourceLineIndex = sourceLineIndex;
			MergedLogEntryIndex = mergedLogEntryIndex;
			OriginalLogEntryIndex = originalLogEntryIndex;
			Timestamp = DateTime.MinValue;
			LogFileIndex = logFileIndex;
		}

		public MergedLogLineIndex(int sourceLineIndex,
		                          int mergedLogEntryIndex,
		                          int originalLogEntryIndex,
		                          byte logFileIndex,
		                          DateTime timestamp)
		{
			SourceLineIndex = sourceLineIndex;
			MergedLogEntryIndex = mergedLogEntryIndex;
			OriginalLogEntryIndex = originalLogEntryIndex;
			Timestamp = timestamp;
			LogFileIndex = logFileIndex;
		}

		public override string ToString()
		{
			return
				string.Format("SourceLineIndex: {0}, OriginalLogEntryIndex: {1}, LogFile: {2}, MergedLogEntryIndex: {3}, Timestamp: {4}",
				              SourceLineIndex, OriginalLogEntryIndex, LogFileIndex, MergedLogEntryIndex, Timestamp);
		}
	}
}