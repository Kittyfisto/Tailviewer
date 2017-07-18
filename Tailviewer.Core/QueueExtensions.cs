using System.Collections.Generic;

namespace Tailviewer.Core
{
	public static class QueueExtensions
	{
		/// <summary>
		/// Tries to dequeue the next value from this queue.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queue"></param>
		/// <param name="value"></param>
		/// <returns>true when a value could be dequeued, false otherwise</returns>
		public static bool TryDequeue<T>(this Queue<T> queue, out T value)
		{
			if (queue.Count > 0)
			{
				value = queue.Dequeue();
				return true;
			}

			value = default(T);
			return false;
		}
	}
}