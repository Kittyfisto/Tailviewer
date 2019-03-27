using System.Collections.Generic;
using System.Xml;

namespace Tailviewer.Settings
{
	public sealed class BookmarksSettings
		: List<BookmarkSettings>
	{
		public void Restore(XmlReader reader)
		{
			var bookmarks = new List<BookmarkSettings>();
			XmlReader subtree = reader.ReadSubtree();

			while (subtree.Read())
			{
				switch (subtree.Name)
				{
					case "bookmark":
						var bookmark = new BookmarkSettings();
						bookmark.Restore(reader);
						bookmarks.Add(bookmark);
						break;
				}
			}

			Clear();
			Capacity = bookmarks.Count;
			AddRange(bookmarks);
		}

		public void Save(XmlWriter writer)
		{
			foreach (BookmarkSettings bookmark in this)
			{
				writer.WriteStartElement("bookmark");
				bookmark.Save(writer);
				writer.WriteEndElement();
			}
		}
	}
}