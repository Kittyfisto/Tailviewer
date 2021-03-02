using System;
using Tailviewer.Api;

namespace Tailviewer.BusinessLogic.Searches
{
	public struct LogMatch : IEquatable<LogMatch>
	{
		/// <summary>
		///     The index of the <see cref="IReadOnlyLogEntry.RawContent" /> where the match occured.
		/// </summary>
		public readonly LogLineIndex Index;

		/// <summary>
		///     The match, i.e. the portion of the <see cref="IReadOnlyLogEntry.RawContent" /> that matched the search/filter term.
		/// </summary>
		public readonly LogLineMatch Match;

		public LogMatch(LogLineIndex index, LogLineMatch match)
		{
			Index = index;
			Match = match;
		}

		public LogMatch(int index, LogLineMatch match)
		{
			Index = index;
			Match = match;
		}

		public bool Equals(LogMatch other)
		{
			return Index.Equals(other.Index) && Match.Equals(other.Match);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogMatch && Equals((LogMatch) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Index.GetHashCode()*397) ^ Match.GetHashCode();
			}
		}

		public static bool operator ==(LogMatch left, LogMatch right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LogMatch left, LogMatch right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", Index, Match);
		}
	}
}