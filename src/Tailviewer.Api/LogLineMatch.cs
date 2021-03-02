using System;
using System.Text.RegularExpressions;

namespace Tailviewer.Api
{
	/// <summary>
	/// 
	/// </summary>
	public struct LogLineMatch : IEquatable<LogLineMatch>
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly int Count;

		/// <summary>
		/// 
		/// </summary>
		public readonly int Index;

		/// <summary>
		/// 
		/// </summary>
		public LogLineMatch(int index, int count)
		{
			Index = index;
			Count = count;
		}

		/// <summary>
		/// 
		/// </summary>
		public LogLineMatch(Match regexMatch)
		{
			Index = regexMatch.Index;
			Count = regexMatch.Length;
		}

		/// <inheritdoc />
		public bool Equals(LogLineMatch other)
		{
			return Index == other.Index && Count == other.Count;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LogLineMatch && Equals((LogLineMatch) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (Index*397) ^ Count;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(LogLineMatch left, LogLineMatch right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(LogLineMatch left, LogLineMatch right)
		{
			return !left.Equals(right);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("Match at {0}, #{1}", Index, Count);
		}
	}
}