using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Tailviewer.MMQ.V1
{
	internal sealed class MemoryMappedQueueConsumer
		: MemoryMappedQueueAccessor
		, IMemoryMappedQueueConsumer
	{
		private static readonly TimeSpan DefaultTimeout;

		private readonly MemoryMappedViewAccessor _accessor;
		private readonly string _queueName;
		private readonly MemoryMappedFile _file;

		static MemoryMappedQueueConsumer()
		{
			DefaultTimeout = TimeSpan.FromSeconds(10);
		}

		public MemoryMappedQueueConsumer(string queueName, MemoryMappedFile file, MemoryMappedViewAccessor accessor)
		{
			_queueName = queueName;
			_file = file;
			_accessor = accessor;
		}

		public byte[] Dequeue()
		{
			return Dequeue(DefaultTimeout);
		}

		public byte[] Dequeue(TimeSpan timeout)
		{
			byte[] message;
			if (!TryDequeue(out message, timeout))
				throw new TimeoutException();

			return message;
		}

		public bool TryDequeue(out byte[] message)
		{
			return TryDequeue(out message, DefaultTimeout);
		}

		public bool TryDequeue(out byte[] message, TimeSpan timeout)
		{
			var sw = new Stopwatch();
			sw.Start();
			for (int i = 0; i < int.MaxValue; ++i)
			{
				if (sw.Elapsed >= timeout)
					break;

				using (var @lock = new QueueLock(_accessor, _queueName))
				{
					if (@lock.Acquire(1))
					{
						if (@lock.AvailableReadLength > 0)
						{
							message = @lock.ReadMessage();
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

			message = null;
			return false;
		}

		public override void Dispose()
		{
			_accessor.Dispose();
			_file.Dispose();
		}
	}
}