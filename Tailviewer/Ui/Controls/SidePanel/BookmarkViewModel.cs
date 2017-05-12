using System;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.Bookmarks;

namespace Tailviewer.Ui.Controls.SidePanel
{
	public sealed class BookmarkViewModel
	{
		public BookmarkViewModel(Bookmark bookmark)
		{
			if (bookmark == null)
				throw new ArgumentNullException(nameof(bookmark));

			Bookmark = bookmark;
			Name = string.Format("Line #{0}", Bookmark.Index.Value+1);
			RemoveCommand = new DelegateCommand(Remove);
		}

		public Bookmark Bookmark { get; }

		public string Name { get; }

		public ICommand RemoveCommand { get; }

		public event Action<BookmarkViewModel> OnRemove;

		private void Remove()
		{
			OnRemove?.Invoke(this);
		}
	}
}