using System;
using System.Collections.Generic;

namespace Tailviewer.Archiver
{
	public static class EnumerableExtensions
	{
		public static T MaxBy<T, TY>(this IEnumerable<T> values, Func<T, TY> selector) where TY : IComparable
		{
			using (var it = values.GetEnumerator())
			{
				if (!it.MoveNext())
					return default(T);

				T highest = it.Current;
				TY highestValue = selector(highest);
				while (it.MoveNext())
				{
					TY currentValue = selector(it.Current);
					if (currentValue.CompareTo(highestValue) > 0)
					{
						highest = it.Current;
						highestValue = currentValue;
					}
				}

				return highest;
			}
		}
	}
}