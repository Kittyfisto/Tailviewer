using System.IO.MemoryMappedFiles;

namespace Tailviewer.MMQ.V1
{
	internal sealed class MemoryMappedQueue
		: IMemoryMappedQueue
	{
		private readonly MemoryMappedFile _file;

		public MemoryMappedQueue(MemoryMappedFile file)
		{
			_file = file;
		}

		public void Dispose()
		{
			_file?.Dispose();
		}
	}
}