using System;
using System.IO;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.BusinessLogic.DataSources;

namespace Tailviewer.Ui.Controls.SidePanel
{
	public sealed class BookmarkViewModel
	{
		private readonly Action<BookmarkViewModel> _navigate;
		private readonly Action<BookmarkViewModel> _remove;

		public BookmarkViewModel(Bookmark bookmark,
			Action<BookmarkViewModel> navigate,
			Action<BookmarkViewModel> remove)
		{
			if (bookmark == null)
				throw new ArgumentNullException(nameof(bookmark));
			if (navigate == null)
				throw new ArgumentNullException(nameof(navigate));
			if (remove == null)
				throw new ArgumentNullException(nameof(remove));

			_navigate = navigate;
			_remove = remove;
			Bookmark = bookmark;
			Name = string.Format("Line #{0}, {1}", Bookmark.Index.Value + 1, DataSourceName(bookmark.DataSource));
			NavigateCommand = new DelegateCommand(Navigate);
			RemoveCommand = new DelegateCommand(Remove);
		}

		public Bookmark Bookmark { get; }

		public string Name { get; }

		public ICommand NavigateCommand { get; }
		public ICommand RemoveCommand { get; }

		private string DataSourceName(IDataSource dataSource)
		{
			var fname = dataSource?.FullFileName;
			var name = Path.GetFileName(fname);
			return name;
		}

		private void Navigate()
		{
			_navigate?.Invoke(this);
		}

		private void Remove()
		{
			_remove?.Invoke(this);
		}
	}
}