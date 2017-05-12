namespace Tailviewer.BusinessLogic.Bookmarks
{
	public sealed class Bookmark
	{
		public Bookmark(LogLineIndex index)
		{
			Index = index;
		}

		public LogLineIndex Index { get; }

		public override string ToString()
		{
			return string.Format("Bookmark at {0}", Index);
		}
	}
}