using System.IO;

namespace Tailviewer.BusinessLogic.Filesystem
{
	internal sealed class FileInfo2
		: IFileInfo
	{
		private readonly string _name;
		private readonly long _length;
		private readonly bool _isReadOnly;
		private readonly bool _exists;

		public FileInfo2(string name, long length, bool isReadOnly, bool exists)
		{
			_name = name;
			_length = length;
			_isReadOnly = isReadOnly;
			_exists = exists;
		}

		public static IFileInfo Capture(string fileName)
		{
			var info = new FileInfo(fileName);
			return new FileInfo2(info.Name, info.Length, info.IsReadOnly, info.Exists);
		}

		public string Name => _name;

		public long Length => _length;

		public bool IsReadOnly => _isReadOnly;

		public bool Exists => _exists;
	}
}