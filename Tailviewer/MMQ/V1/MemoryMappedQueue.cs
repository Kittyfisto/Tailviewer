using System.IO.MemoryMappedFiles;

namespace Tailviewer.MMQ.V1
{
	internal sealed class MemoryMappedQueue
		: IMemoryMappedQueue
	{
		private readonly MemoryMappedFile _file;
		private readonly string _name;

		public MemoryMappedQueue(string name, MemoryMappedFile file)
		{
			_name = name;
			_file = file;
		}

		public void Dispose()
		{
			_file?.Dispose();
		}

		public IMemoryMappedQueueProducer CreateProducer()
		{
			return MMQ.MemoryMappedQueue.CreateProducer(_name);
		}

		public IMemoryMappedQueueConsumer CreateConsumer()
		{
			return MMQ.MemoryMappedQueue.CreateConsumer(_name);
		}
	}
}