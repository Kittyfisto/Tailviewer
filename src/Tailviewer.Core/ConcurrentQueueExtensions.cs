using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Tailviewer.Core
{
	/// <summary>
	///     Contains extension methods for the <see cref="ConcurrentQueue{T}" /> class.
	/// </summary>
	public static class ConcurrentQueueExtensions
	{
		/// <summary>
		///     Enqueues many items into the given queue.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queue"></param>
		/// <param name="values"></param>
		public static void EnqueueMany<T>(this ConcurrentQueue<T> queue, IEnumerable<T> values)
		{
			foreach (var value in values) queue.Enqueue(value);
		}
	}
}