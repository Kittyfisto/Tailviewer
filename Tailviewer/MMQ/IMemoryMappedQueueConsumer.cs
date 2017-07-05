using System;

namespace Tailviewer.MMQ
{
	public interface IMemoryMappedQueueConsumer
		: IMemoryMappedQueueAccessor
	{
		/// <summary>
		///     Blocks until:
		///     * A message could be dequeued from the queue
		///     * The queue has been disposed of
		///     * This consumer has been disposed of
		/// </summary>
		/// <returns>the dequeued message</returns>
		/// <exception cref="ObjectDisposedException">This producer has been disposed of.</exception>
		/// <exception cref="InvalidOperationException">The underlying queue has been disposed of.</exception>
		byte[] Dequeue();

		/// <summary>
		///     Blocks until:
		///     * A message could be dequeued from the queue
		///     * The timeout ellapsed
		///     * The queue has been disposed of
		///     * This consumer has been disposed of
		/// </summary>
		/// <param name="message"></param>
		/// <param name="timeout"></param>
		/// <returns>true when a message could be successfully dequeued, false otherwise</returns>
		/// <exception cref="ObjectDisposedException">This producer has been disposed of.</exception>
		/// <exception cref="InvalidOperationException">The underlying queue has been disposed of.</exception>
		bool TryDequeue(out byte[] message, TimeSpan timeout);
	}
}