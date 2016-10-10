using System.Collections.Generic;

namespace Tailviewer
{
	public static class HashSetExtensions
	{
		public static bool HasEqualContent<T>(this HashSet<T> that, HashSet<T> other)
		{
			if (that == null)
			{
				if (other != null)
					return false;

				return true;
			}
			if (other == null)
			{
				return false;
			}

			if (that.Count != other.Count)
				return false;

			foreach (var value in that)
			{
				if (!other.Contains(value))
					return false;
			}

			return true;
		}

		public static bool HasEqualContent<T>(this HashSet<T> that, IEnumerable<T> other)
		{
			if (that == null)
			{
				if (other != null)
					return false;

				return true;
			}
			if (other == null)
			{
				return false;
			}

			int n = 0;
			var it = other.GetEnumerator();
			while (it.MoveNext())
			{
				if (!that.Contains(it.Current))
					return false;

				++n;
			}

			return n == that.Count;
		}

		public static void AddRange<T>(this HashSet<T> that, IEnumerable<T> values)
		{
			foreach (var value in values)
			{
				that.Add(value);
			}
		}
	}
}