using System;

namespace SharpTail.BusinessLogic
{
	public sealed class DataSource
	{
		public string FileName;
		public DateTime LastOpened;
		public bool IsOpen;
		public bool FollowTail;
		public string FilterString;
	}
}