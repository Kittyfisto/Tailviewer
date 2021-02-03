using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Ui.Controls.SidePanel.Bookmarks;

namespace Tailviewer.Test.Ui.Controls.SidePanel.Bookmarks
{
	[TestFixture]
	public sealed class BookmarkViewModelTest
	{
		[Test]
		public void TestDataSource()
		{
			var dataSource = new Mock<IDataSource>();
			dataSource.SetupGet(x => x.FullFileName).Returns("C:\\my_awesome_log_file.txt");
			var bookmark = new Bookmark(dataSource.Object, 9001);
			var viewModel = new BookmarkViewModel(bookmark, model => {}, model => {});
			viewModel.Name.Should().Be("Line #9002, my_awesome_log_file.txt");
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/269")]
		public void TestMergedDataSource()
		{
			var dataSource = new Mock<IMergedDataSource>();
			dataSource.SetupGet(x => x.DisplayName).Returns("My Merged Data Source");
			var bookmark = new Bookmark(dataSource.Object, 9001);
			var viewModel = new BookmarkViewModel(bookmark, model => {}, model => {});
			viewModel.Name.Should().Be("Line #9002, My Merged Data Source");
		}
	}
}
