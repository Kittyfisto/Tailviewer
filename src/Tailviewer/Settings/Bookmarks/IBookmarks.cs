using System.Collections.Generic;

namespace Tailviewer.Settings.Bookmarks
{
	public interface IBookmarks
	{
		void Add(BookmarkSettings bookmark);
		void Remove(IEnumerable<BookmarkSettings> removed);

		IEnumerable<BookmarkSettings> All { get; }

		bool Save();
		void SaveAsync();
	}
}