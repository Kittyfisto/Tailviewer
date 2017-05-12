using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Ui.Controls.SidePanel
{
	internal sealed class BookmarksViewModel
		: AbstractSidePanelViewModel
	{
		private readonly DelegateCommand _addBookmarkCommand;
		private readonly ObservableCollection<BookmarkViewModel> _bookmarks;
		private readonly Dictionary<Bookmark, BookmarkViewModel> _viewModelByBookmark;
		private IDataSourceViewModel _currentDataSource;
		private bool _canAddBookmarks;

		public BookmarksViewModel()
		{
			_addBookmarkCommand = new DelegateCommand(AddBookmark, CanAddBookmark);
			_viewModelByBookmark = new Dictionary<Bookmark, BookmarkViewModel>();
			_bookmarks = new ObservableCollection<BookmarkViewModel>();
		}

		public ICommand AddBookmarkCommand => _addBookmarkCommand;

		private bool CanAddBookmarks
		{
			get { return _canAddBookmarks; }
			set
			{
				if (value == _canAddBookmarks)
					return;

				_canAddBookmarks = value;
				_addBookmarkCommand.RaiseCanExecuteChanged();
			}
		}

		public IEnumerable<BookmarkViewModel> Bookmarks => _bookmarks;

		public IDataSourceViewModel CurrentDataSource
		{
			get { return _currentDataSource; }
			set
			{
				if (value == _currentDataSource)
					return;

				_currentDataSource = value;
				Update();
			}
		}

		public override Geometry Icon => Icons.Bookmark;

		public override string Id => "bookmarks";

		public override void Update()
		{
			var bookmarks = _currentDataSource?.DataSource?.Bookmarks;
			if (bookmarks != null)
			{
				// Add bookmarks we haven't added yet
				foreach (var bookmark in bookmarks)
				{
					BookmarkViewModel viewModel;
					if (!_viewModelByBookmark.TryGetValue(bookmark, out viewModel))
					{
						viewModel = new BookmarkViewModel(bookmark);
						viewModel.OnRemove += OnBookmarkRemoved;

						_viewModelByBookmark.Add(bookmark, viewModel);
						Insert(viewModel);
					}
				}

				// Remove bookmarks that shouldn't be there
				for (int i = 0; i < _bookmarks.Count;)
				{
					var viewModel = _bookmarks[i];
					if (!bookmarks.Contains(viewModel.Bookmark))
					{
						_viewModelByBookmark.Remove(viewModel.Bookmark);
						_bookmarks.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}
			CanAddBookmarks = Any(_currentDataSource?.SelectedLogLines);
		}

		private void OnBookmarkRemoved(BookmarkViewModel viewModel)
		{
			_viewModelByBookmark.Remove(viewModel.Bookmark);
			_bookmarks.Remove(viewModel);
			_currentDataSource.DataSource.RemoveBookmark(viewModel.Bookmark);
		}

		private static bool Any(HashSet<LogLineIndex> selectedLogLines)
		{
			return selectedLogLines?.Count > 0;
		}

		private void Insert(BookmarkViewModel viewModel)
		{
			int i;
			for (i = 0; i < _bookmarks.Count; ++i)
			{
				var bookmark = _bookmarks[i];
				if (bookmark.Bookmark.Index > viewModel.Bookmark.Index)
					break;
			}
			_bookmarks.Insert(i, viewModel);
		}

		private bool CanAddBookmark()
		{
			return CanAddBookmarks;
		}

		private void AddBookmark()
		{
			var lines = _currentDataSource?.SelectedLogLines;
			if (lines == null)
				return;

			foreach (var line in lines)
			{
				var bookmark = _currentDataSource.DataSource.TryAddBookmark(line);
				if (bookmark != null)
				{
					var viewModel = new BookmarkViewModel(bookmark);
					viewModel.OnRemove += OnBookmarkRemoved;
					_viewModelByBookmark.Add(bookmark, viewModel);
					Insert(viewModel);
				}
			}
		}
	}
}