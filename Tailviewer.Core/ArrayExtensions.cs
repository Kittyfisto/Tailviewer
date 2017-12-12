using System;

namespace Tailviewer.Core
{
	/// <summary>
	///     Extensions to the array class.
	/// </summary>
	public static class ArrayExtensions
	{
		/// <summary>
		///     Fills this array with the given value from the given index.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="that"></param>
		/// <param name="value"></param>
		public static void Fill<T>(this T[] that, T value)
		{
			if (that == null)
				throw new ArgumentNullException(nameof(that));

			for (var i = 0; i < that.Length; ++i)
				that[i] = value;
		}

		/// <summary>
		///     Fills this array with the given value from the given index.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="that"></param>
		/// <param name="value"></param>
		/// <param name="start"></param>
		/// <param name="count"></param>
		public static void Fill<T>(this T[] that, T value, int start, int count)
		{
			if (that == null)
				throw new ArgumentNullException(nameof(that));

			for (var i = 0; i < count; ++i)
				that[start + i] = value;
		}
	}
}