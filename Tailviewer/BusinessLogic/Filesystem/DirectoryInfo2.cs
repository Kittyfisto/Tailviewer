using System.IO;

namespace Tailviewer.BusinessLogic.Filesystem
{
	internal sealed class DirectoryInfo2
		: IDirectoryInfo
	{
		private readonly string _name;
		private readonly string _fullName;
		private readonly bool _exists;

		public DirectoryInfo2(string name, string fullName, bool exists)
		{
			_name = name;
			_fullName = fullName;
			_exists = exists;
		}

		public static IDirectoryInfo Capture(string path)
		{
			return Capture(new DirectoryInfo(path));
		}

		public static IDirectoryInfo Capture(DirectoryInfo info)
		{
			return new DirectoryInfo2(info.Name, info.FullName, info.Exists);
		}

		public string Name => _name;

		public string FullName => _fullName;

		public bool Exists => _exists;
	}
}