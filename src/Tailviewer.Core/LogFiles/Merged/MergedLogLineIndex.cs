using System;

namespace Tailviewer.Core.LogFiles.Merged
{
	/// <summary>
	///     A single entry in the <see cref="MergedLogEntryIndex" /> structure.
	/// </summary>
	internal struct MergedLogLineIndex : IEquatable<MergedLogLineIndex>
	{
		public static readonly MergedLogLineIndex Invalid;

		public readonly int OriginalLogEntryIndex;
		public readonly int SourceLineIndex;
		public readonly byte SourceId;
		public readonly DateTime Timestamp;
		public int MergedLogEntryIndex;

		static MergedLogLineIndex()
		{
			Invalid = new MergedLogLineIndex(-1, -1, -1, 0);
		}

		public MergedLogLineIndex(int sourceLineIndex,
		                          int mergedLogEntryIndex,
		                          int originalLogEntryIndex,
		                          byte sourceId)
		{
			SourceLineIndex = sourceLineIndex;
			MergedLogEntryIndex = mergedLogEntryIndex;
			OriginalLogEntryIndex = originalLogEntryIndex;
			Timestamp = DateTime.MinValue;
			SourceId = sourceId;
		}

		public MergedLogLineIndex(int sourceLineIndex,
		                          int mergedLogEntryIndex,
		                          int originalLogEntryIndex,
		                          byte sourceId,
		                          DateTime timestamp)
		{
			SourceLineIndex = sourceLineIndex;
			MergedLogEntryIndex = mergedLogEntryIndex;
			OriginalLogEntryIndex = originalLogEntryIndex;
			Timestamp = timestamp;
			SourceId = sourceId;
		}

		#region Equality members

		public bool Equals(MergedLogLineIndex other)
		{
			return OriginalLogEntryIndex == other.OriginalLogEntryIndex && SourceLineIndex == other.SourceLineIndex &&
			       SourceId == other.SourceId && Timestamp.Equals(other.Timestamp) &&
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
				hashCode = (hashCode * 397) ^ SourceId.GetHashCode();
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

		public override string ToString()
		{
			return
				string.Format("SourceLineIndex: {0}, OriginalLogEntryIndex: {1}, LogFile: {2}, MergedLogEntryIndex: {3}, Timestamp: {4}",
				              SourceLineIndex, OriginalLogEntryIndex, SourceId, MergedLogEntryIndex, Timestamp);
		}
	}
}