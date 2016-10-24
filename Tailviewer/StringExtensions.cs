using System;

namespace Tailviewer
{
	public static class StringExtensions
	{
		public static int IndexOf(this string that, Predicate<char> pred)
		{
			return IndexOf(that, 0, pred);
		}

		public static int IndexOf(this string that, int startIndex, Predicate<char> pred)
		{
			return IndexOf(that, startIndex, that.Length - startIndex, pred);
		}

		/// <summary>
		///     Finds the index of the first <see cref="char" /> for which the given predicate returns true.
		///     If no such character has been found, -1 is returned.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="startIndex"></param>
		/// <param name="length"></param>
		/// <param name="pred"></param>
		/// <returns></returns>
		public static int IndexOf(this string that, int startIndex, int length, Predicate<char> pred)
		{
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException("startIndex");
			if (startIndex >= that.Length)
				throw new ArgumentOutOfRangeException("startIndex");
			if (startIndex + length > that.Length)
				throw new ArgumentOutOfRangeException("length");

			int totalLength = startIndex + length;
			for (int i = startIndex; i < totalLength; ++i)
			{
				char c = that[i];
				if (pred(c))
					return i;
			}

			return -1;
		}
	}
}