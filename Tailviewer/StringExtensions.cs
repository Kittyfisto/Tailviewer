using System;
using System.Diagnostics.Contracts;

namespace Tailviewer
{
	public static class StringExtensions
	{
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

		public static string TrimNewlineEnd(this string that)
		{
			bool unused;
			return TrimNewlineEnd(that, out unused);
		}

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

		public static int IndexOf(this string that, Predicate<char> pred)
		{
			return IndexOf(that, pred, 0);
		}

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

		[Pure]
		public static bool ContainsAt(this string that, string value, int startIndex)
		{
			var sub = that.Substring(startIndex);
			return sub.StartsWith(value);
		}
	}
}