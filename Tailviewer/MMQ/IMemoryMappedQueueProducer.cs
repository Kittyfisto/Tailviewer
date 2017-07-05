using System;

namespace Tailviewer.MMQ
{
	/// <summary>
	///     The interface to enqueue messages into a machine-wide queue.
	/// </summary>
	public interface IMemoryMappedQueueProducer
		: IDisposable
	{
		/// <summary>
		///     Enqueues the given message at the end of the queue.
		///     Blocks until the message has been fully written, but not necessarily until it
		///     has been consumed.
		/// </summary>
		/// <param name="message"></param>
		/// <exception cref="ArgumentNullException">When <paramref name="message"/> is null</exception>
		/// <exception cref="ObjectDisposedException">This producer has been disposed of.</exception>
		/// <exception cref="InvalidOperationException">The underlying queue has been disposed of.</exception>
		/// <exception cref="TimeoutException">When the message couldn't be fully written within 10 seconds</exception>
		void Enqueue(byte[] message);

		/// <summary>
		///     Enqueues the given message at the end of the queue.
		///     Blocks until the message has been fully written or until the given timeout has ellapsed.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="timeout"></param>
		/// <returns>true when the message has been successfully written, false otherwise</returns>
		/// <exception cref="ArgumentNullException">When <paramref name="message"/> is null</exception>
		/// <exception cref="ObjectDisposedException">This producer has been disposed of.</exception>
		/// <exception cref="InvalidOperationException">The underlying queue has been disposed of.</exception>
		bool TryEnqueue(byte[] message, TimeSpan timeout);
	}
}