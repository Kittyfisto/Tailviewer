using System.Collections.Generic;

namespace Tailviewer
{
	public static class EnumerableExtensions
	{
		public static int IndexOf<T>(this IEnumerable<T> that, T value)
		{
			var it = that.GetEnumerator();
			int i = 0;
			while (it.MoveNext())
			{
				if (Equals(it.Current, value))
					return i;

				++i;
			}

			return -1;
		}
	}
}