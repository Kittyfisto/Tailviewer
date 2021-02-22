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
		///    Clears this queue of all entries.
		/// </summary>
		/// <param name="that"></param>
		/// <typeparam name="T"></typeparam>
		public static void Clear<T>(this ConcurrentQueue<T> that)
		{
			while(that.TryDequeue(out var _)){}
		}

		/// <summary>
		///     En-queues many items into the given queue.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queue"></param>
		/// <param name="values"></param>
		public static void EnqueueMany<T>(this ConcurrentQueue<T> queue, IEnumerable<T> values)
		{
			foreach (var value in values) queue.Enqueue(value);
		}

		/// <summary>
		///     De-queues and returns all values from the given queue.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queue"></param>
		/// <returns></returns>
		public static IReadOnlyList<T> DequeueAll<T>(this ConcurrentQueue<T> queue)
		{
			List<T> values = null;
			while (queue.TryDequeue(out var value))
			{
				if (values == null)
					values = new List<T>();
				values.Add(value);
			}
			return values ?? EnumerableExtensions<T>.Empty;
		}

		/// <summary>
		///    De-queues modifications from this queue until their combined sum of log entries added is greater or equal to the specified number.
		/// </summary>
		/// <param name="queue"></param>
		/// <param name="maxLogEntries"></param>
		/// <param name="logFileSections"></param>
		/// <returns></returns>
		public static bool TryDequeueUpTo(this ConcurrentQueue<LogFileSection> queue, int maxLogEntries,
		                                  out IReadOnlyList<LogFileSection> logFileSections)
		{
			var tmp = new List<LogFileSection>();
			int count = 0;

			while (queue.TryDequeue(out var modification))
			{
				tmp.Add(modification);
				if (!modification.IsInvalidate)
					count += modification.Count;

				if (count >= maxLogEntries)
					break;
			}

			logFileSections = tmp;
			return tmp.Count > 0;
		}

		/// <summary>
		///    De-queues modifications from this queue until their combined sum of log entries added is greater or equal to the specified number.
		/// </summary>
		/// <param name="queue"></param>
		/// <param name="maxLogEntries"></param>
		/// <param name="logFileSections"></param>
		/// <returns></returns>
		public static bool TryDequeueUpTo(this ConcurrentQueue<KeyValuePair<ILogSource, LogFileSection>> queue, int maxLogEntries,
		                                  out IReadOnlyList<KeyValuePair<ILogSource, LogFileSection>> logFileSections)
		{
			var tmp = new List<KeyValuePair<ILogSource, LogFileSection>>();
			int count = 0;

			while (queue.TryDequeue(out var pair))
			{
				tmp.Add(pair);
				if (!pair.Value.IsInvalidate)
					count += pair.Value.Count;

				if (count >= maxLogEntries)
					break;
			}

			logFileSections = tmp;
			return tmp.Count > 0;
		}
	}
}