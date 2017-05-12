using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Ui.Controls.SidePanel;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui.Controls.SidePanel
{
	[TestFixture]
	public sealed class BookmarksViewModelTest
	{
		private BookmarksViewModel _viewModel;
		private Mock<IDataSourceViewModel> _dataSourceViewModel;
		private Mock<IDataSource> _dataSource;
		private List<Bookmark> _bookmarks;

		[SetUp]
		public void Setup()
		{
			_dataSourceViewModel = new Mock<IDataSourceViewModel>();
			_dataSource = new Mock<IDataSource>();
			_bookmarks = new List<Bookmark>();
			_dataSource.Setup(x => x.Bookmarks).Returns(_bookmarks);
			_dataSource.Setup(x => x.TryAddBookmark(It.IsAny<LogLineIndex>())).Returns(() => new Bookmark(1));
			_dataSourceViewModel.Setup(x => x.DataSource).Returns(_dataSource.Object);

			_viewModel = new BookmarksViewModel();
		}

		[Test]
		public void TestUpdateNoLineSelected()
		{
			_viewModel.CurrentDataSource = _dataSourceViewModel.Object;
			_viewModel.AddBookmarkCommand.CanExecute(null).Should().BeFalse("because not a single line is selected");

			_dataSourceViewModel.Setup(x => x.SelectedLogLines).Returns(new HashSet<LogLineIndex> { 42 });
			_viewModel.Update();
			_viewModel.AddBookmarkCommand.CanExecute(null).Should().BeTrue("because a single line is selected and thus it should be possible to add a bookmark there");

			_dataSourceViewModel.Setup(x => x.SelectedLogLines).Returns(new HashSet<LogLineIndex>());
			_viewModel.Update();
			_viewModel.AddBookmarkCommand.CanExecute(null).Should().BeFalse("because not a single line is selected");
		}

		[Test]
		public void TestRemove1()
		{
			_bookmarks.Add(new Bookmark(1));
			_viewModel.CurrentDataSource = _dataSourceViewModel.Object;
			_viewModel.Update();
			_viewModel.Bookmarks.Should().HaveCount(1);

			var bookmark = _viewModel.Bookmarks.First();
			bookmark.RemoveCommand.CanExecute(null).Should().BeTrue();
			new Action(() => bookmark.RemoveCommand.Execute(null)).ShouldNotThrow();
			_viewModel.Bookmarks.Should().BeEmpty();
			_dataSource.Verify(x => x.RemoveBookmark(It.IsAny<Bookmark>()), Times.Once);
		}

		[Test]
		public void TestAddRemove()
		{
			_dataSourceViewModel.Setup(x => x.SelectedLogLines).Returns(new HashSet<LogLineIndex> {13});
			_viewModel.CurrentDataSource = _dataSourceViewModel.Object;
			_viewModel.AddBookmarkCommand.Execute(null);
			_viewModel.Bookmarks.Should().NotBeEmpty();
			var bookmark = _viewModel.Bookmarks.First();
			bookmark.RemoveCommand.Execute(null);
			_viewModel.Bookmarks.Should().BeEmpty();
		}
	}
}