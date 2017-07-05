using System;
using System.IO.MemoryMappedFiles;

namespace Tailviewer.MMQ.V1
{
	internal sealed class MemoryMappedQueueFactory
		: IMemoryMappedQueueFactory
	{
		private readonly string _queueName;
		private readonly MemoryMappedFile _file;
		private readonly int _start;
		private readonly int _length;

		public MemoryMappedQueueFactory(string queueName, MemoryMappedFile file, int start, int length)
		{
			_queueName = queueName;
			_file = file;
			_start = start;
			_length = length;
		}

		public IMemoryMappedQueueProducer CreateProducer()
		{
			MemoryMappedViewAccessor accessor = null;
			try
			{
				accessor = _file.CreateViewAccessor(_start, _length);
				return new MemoryMappedQueueProducer(_queueName, accessor);
			}
			catch (Exception)
			{
				accessor?.Dispose();
				throw;
			}
		}

		public IMemoryMappedQueueConsumer CreateConsumer()
		{
			MemoryMappedViewAccessor accessor = null;
			try
			{
				accessor = _file.CreateViewAccessor(_start, _length);
				return new MemoryMappedQueueConsumer(_queueName, accessor);
			}
			catch (Exception)
			{
				accessor?.Dispose();
				throw;
			}
		}
	}
}