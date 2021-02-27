using System;
using System.Collections.Generic;
using log4net;

namespace Tailviewer.Collections
{
	public static class EnumerableExtensions
	{
		public static int IndexOf<T>(this IEnumerable<T> that, T value)
		{
			var it = that.GetEnumerator();
			var i = 0;
			while (it.MoveNext())
			{
				if (Equals(it.Current, value))
					return i;

				++i;
			}

			return -1;
		}

		/// <summary>
		///     Disposes every object in the given enumeration.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="that"></param>
		/// <param name="log"></param>
		public static void DisposeAll<T>(this IEnumerable<T> that, ILog log) where T : IDisposable
		{
			foreach (var obj in that)
				try
				{
					obj.Dispose();
				}
				catch (Exception e)
				{
					log.ErrorFormat("Caught unexpected exception: {0}", e);
				}
		}
	}
}