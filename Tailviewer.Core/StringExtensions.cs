using System;
using System.Diagnostics.Contracts;

namespace Tailviewer.Core
{
	/// <summary>
	/// Extension methods for the <see cref="string"/> class.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Tests if this string ends with \n.
		/// </summary>
		/// <param name="that"></param>
		/// <returns></returns>
		public static bool EndsWithNewline(this string that)
		{
			if (that == null)
				return false;

			var length = that.Length;
			if (length == 0)
				return false;

			if (that[length - 1] == '\n')
				return true;

			return false;
		}

		/// <summary>
		/// Tests if this string ends with \r\n.
		/// </summary>
		/// <param name="that"></param>
		/// <returns></returns>
		public static bool EndsWithCarriageFeedLineReturn(this string that)
		{
			if (that == null)
				return false;

			var length = that.Length;
			if (length < 2)
				return false;

			if (that[length - 2] == '\r' &&
				that[length - 1] == '\n')
				return true;

			return false;
		}

		/// <summary>
		/// Returns a new string which has any trailing newlines removed.
		/// </summary>
		/// <param name="that"></param>
		/// <returns></returns>
		public static string TrimNewlineEnd(this string that)
		{
			bool unused;
			return TrimNewlineEnd(that, out unused);
		}

		/// <summary>
		/// Returns a new string which has any trailing newlines removed.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="trimmed"></param>
		/// <returns></returns>
		public static string TrimNewlineEnd(this string that, out bool trimmed)
		{
			if (that.EndsWithCarriageFeedLineReturn())
			{
				trimmed = true;
				return that.Substring(0, that.Length - 2);
			}

			if (that.EndsWithNewline())
			{
				trimmed = true;
				return that.Substring(0, that.Length - 1);
			}

			trimmed = false;
			return that;
		}

		/// <summary>
		/// Finds the first index that satisfies the given predicate.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="pred"></param>
		/// <returns></returns>
		public static int IndexOf(this string that, Predicate<char> pred)
		{
			return IndexOf(that, pred, 0);
		}

		/// <summary>
		/// Finds the first index that satisfies the given predicate.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="pred"></param>
		/// <param name="startIndex"></param>
		/// <returns></returns>
		public static int IndexOf(this string that, Predicate<char> pred, int startIndex)
		{
			return IndexOf(that, pred, startIndex, that.Length - startIndex);
		}

		/// <summary>
		///     Finds the index of the first <see cref="char" /> for which the given predicate returns true.
		///     If no such character has been found, -1 is returned.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="pred"></param>
		/// <param name="startIndex"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static int IndexOf(this string that, Predicate<char> pred, int startIndex, int length)
		{
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (startIndex >= that.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (startIndex + length > that.Length)
				throw new ArgumentOutOfRangeException(nameof(length));

			int totalLength = startIndex + length;
			for (int i = startIndex; i < totalLength; ++i)
			{
				char c = that[i];
				if (pred(c))
					return i;
			}

			return -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="that"></param>
		/// <param name="value"></param>
		/// <param name="startIndex"></param>
		/// <returns></returns>
		[Pure]
		public static bool ContainsAt(this string that, string value, int startIndex)
		{
			var sub = that.Substring(startIndex);
			return sub.StartsWith(value);
		}
	}
}