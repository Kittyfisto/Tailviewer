using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Tailviewer.MMQ.V1
{
	internal sealed class MemoryMappedQueueProducer
		: MemoryMappedQueueAccessor
		, IMemoryMappedQueueProducer
	{
		private readonly MemoryMappedViewAccessor _accessor;
		private readonly MemoryMappedFile _file;
		private readonly string _queueName;

		private static readonly TimeSpan Timeout;

		static MemoryMappedQueueProducer()
		{
			Timeout = TimeSpan.FromSeconds(10);
		}

		public MemoryMappedQueueProducer(string queueName, MemoryMappedFile file, MemoryMappedViewAccessor accessor)
		{
			_queueName = queueName;
			_file = file;
			_accessor = accessor;
		}

		public void Enqueue(byte[] message)
		{
			if (!TryEnqueue(message, Timeout))
				throw new TimeoutException();
		}

		public bool TryEnqueue(byte[] message, TimeSpan timeout)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			var sw = new Stopwatch();
			sw.Start();
			for(int i = 0; i < int.MaxValue; ++i)
			{
				if (sw.Elapsed >= timeout)
					return false;

				using (var @lock = new QueueLock(_accessor, _queueName))
				{
					if (@lock.Acquire(1))
					{
						if (@lock.AvailableWriteLength >= message.Length)
						{
							@lock.WriteMessage(message);
							return true;
						}
					}
				}

				int sleepTime;
				if (i < 10)
					sleepTime = 0;
				else if (i < 100)
					sleepTime = 1;
				else
					sleepTime = 10;
				Thread.Sleep(sleepTime);
			}

			return false;
		}

		public override void Dispose()
		{
			_accessor.Dispose();
			_file.Dispose();
		}
	}
}