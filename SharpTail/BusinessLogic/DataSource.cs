using System;
using System.IO;

namespace SharpTail.BusinessLogic
{
	[Serializable]
	public sealed class DataSource
	{
		public string FullFileName;
		public DateTime LastOpened;
		public DateTime LastWritten;
		public bool IsOpen;
		public bool FollowTail;
		public string StringFilter;
		public LevelFlags Levels;

		public DataSource()
		{
			Levels = LevelFlags.All;
		}

		public DataSource(string file)
		{
			FullFileName = Path.GetFullPath(file);
			Levels = LevelFlags.All;
		}
	}
}