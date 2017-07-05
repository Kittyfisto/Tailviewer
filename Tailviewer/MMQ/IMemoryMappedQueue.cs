using System;

namespace Tailviewer.MMQ
{
	/// <summary>
	///     Represents a queue backed by memory that may be shared between different processes
	///     on the same machine.
	///     The queue exists until disposed of. Once disposed, <see cref="IMemoryMappedQueueProducer" />
	///     and <see cref="IMemoryMappedQueueConsumer" /> may no longer work and most methods throw an
	///     <see cref="InvalidOperationException" />.
	/// </summary>
	public interface IMemoryMappedQueue
		: IDisposable
	{
		
	}
}