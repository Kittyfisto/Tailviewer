using System;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.BusinessLogic.Bookmarks
{
	public sealed class Bookmark : IEquatable<Bookmark>
	{
		public Bookmark(IDataSource dataSource, LogLineIndex index)
		{
			DataSource = dataSource;
			Index = index;
		}

		public bool Equals(Bookmark other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(DataSource, other.DataSource) && Index.Equals(other.Index);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Bookmark && Equals((Bookmark) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((DataSource != null ? DataSource.GetHashCode() : 0) * 397) ^ Index.GetHashCode();
			}
		}

		public static bool operator ==(Bookmark left, Bookmark right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Bookmark left, Bookmark right)
		{
			return !Equals(left, right);
		}

		public IDataSource DataSource { get; }
		public LogLineIndex Index { get; }

		public override string ToString()
		{
			return string.Format("Bookmark at {0}", Index);
		}
	}
}