using System;
using System.Xml;
using Metrolib;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Settings.Bookmarks
{
	public sealed class BookmarkSettings
		: ICloneable
	{
		public DataSourceId DataSourceId;
		public LogLineIndex Index;

		public BookmarkSettings()
		{}

		public BookmarkSettings(DataSourceId dataSourceId, LogLineIndex index)
		{
			DataSourceId = dataSourceId;
			Index = index;
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeGuid("datasource", DataSourceId.Value);
			writer.WriteAttributeInt("index", Index.Value);
		}

		public void Restore(XmlReader reader)
		{
			for (var i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "datasource":
						DataSourceId = new DataSourceId(reader.ReadContentAsGuid());
						break;

					case "index":
						Index = reader.ReadContentAsInt();
						break;
				}
			}
		}

		#region Equality members

		private bool Equals(BookmarkSettings other)
		{
			return DataSourceId.Equals(other.DataSourceId) && Index.Equals(other.Index);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(objA: null, objB: obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is BookmarkSettings && Equals((BookmarkSettings) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (DataSourceId.GetHashCode() * 397) ^ Index.GetHashCode();
			}
		}

		public BookmarkSettings Clone()
		{
			return new BookmarkSettings(DataSourceId, Index);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion
	}
}