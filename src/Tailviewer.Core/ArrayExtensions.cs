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

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="that"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool Equals<T>(T[] that, T[] other) where T : struct, IEquatable<T>
		{
			if (StartsWith(that, other))
			{
				if (that.Length != other.Length)
					return false;

				return true;
			}

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="that"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool StartsWith<T>(T[] that, T[] other) where T : struct, IEquatable<T>
		{
			if (ReferenceEquals(that, other))
				return true;

			if (ReferenceEquals(that, null) || ReferenceEquals(other, null))
				return false;

			if (that.Length < other.Length)
				return false;

			for (int i = 0; i < other.Length; ++i)
			{
				if (!that[i].Equals(other[i]))
					return false;
			}

			return true;
		}
	}
}