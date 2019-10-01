using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings.Bookmarks;

namespace Tailviewer.Test.BusinessLogic.Bookmarks
{
	[TestFixture]
	public sealed class BookmarkCollectionTest
	{
		private InMemoryLogFile _logFile;
		private Mock<IDataSource> _dataSource;
		private Mock<IBookmarks> _bookmarks;

		[SetUp]
		public void Setup()
		{
			_logFile = new InMemoryLogFile();
			_dataSource = new Mock<IDataSource>();
			_dataSource.Setup(x => x.UnfilteredLogFile).Returns(_logFile);
			_bookmarks = new Mock<IBookmarks>();
		}

		[Test]
		public void TestConstruction1()
		{
			var collection = new BookmarkCollection(_bookmarks.Object, TimeSpan.Zero);
			collection.Count.Should().Be(0);
			collection.Bookmarks.Should().BeEmpty();

			collection.Contains(null, 0).Should().BeFalse();
			collection.Contains(_dataSource.Object, -1).Should().BeFalse();
			collection.Contains(_dataSource.Object, 0).Should().BeFalse();
			collection.Contains(_dataSource.Object, 1).Should().BeFalse();
			collection.Contains(_dataSource.Object, 2).Should().BeFalse();
		}

		[Test]
		public void TestConstruction2()
		{
			var bookmarks = new List<BookmarkSettings>();
			bookmarks.Add(new BookmarkSettings(DataSourceId.CreateNew(), new LogLineIndex(42)));
			bookmarks.Add(new BookmarkSettings(DataSourceId.CreateNew(), new LogLineIndex(101)));

			_bookmarks.Setup(x => x.All).Returns(bookmarks);
			var collection = new BookmarkCollection(_bookmarks.Object, TimeSpan.Zero);
			collection.Bookmarks.Should().BeEmpty();

			_dataSource.Setup(x => x.Id).Returns(bookmarks[1].DataSourceId);
			collection.AddDataSource(_dataSource.Object);
			collection.Bookmarks.Should().HaveCount(1);
			collection.Bookmarks[0].DataSource.Should().BeSameAs(_dataSource.Object);
			collection.Bookmarks[0].Index.Should().Be(new LogLineIndex(101));
		}

		[Test]
		public void TestAddOneBookmark()
		{
			var collection = new BookmarkCollection(_bookmarks.Object, TimeSpan.Zero);
			collection.AddDataSource(_dataSource.Object);

			_logFile.AddEntry("", LevelFlags.Error);
			_logFile.AddEntry("", LevelFlags.Error);

			_bookmarks.Verify(x => x.SaveAsync(), Times.Never);

			var bookmark = collection.TryAddBookmark(_dataSource.Object, 1);
			bookmark.Should().NotBeNull();
			bookmark.Index.Should().Be(new LogLineIndex(1));
			collection.Count.Should().Be(1);
			collection.Bookmarks.Should().BeEquivalentTo(new object[] {bookmark});

			_bookmarks.Verify(x => x.SaveAsync(), Times.Once);
		}

		[Test]
		public void TestAddTwoBookmarks()
		{
			var collection = new BookmarkCollection(_bookmarks.Object, TimeSpan.Zero);
			collection.AddDataSource(_dataSource.Object);

			_logFile.AddEntry("", LevelFlags.Error);
			_logFile.AddEntry("", LevelFlags.Error);

			var bookmark1 = collection.TryAddBookmark(_dataSource.Object, 1);
			var bookmark2 = collection.TryAddBookmark(_dataSource.Object, 0);
			bookmark2.Should().NotBeNull();
			bookmark2.Index.Should().Be(new LogLineIndex(0));
			collection.Count.Should().Be(2);
			collection.Bookmarks.Should().BeEquivalentTo(new object[] { bookmark1, bookmark2 });
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/214")]
		[Description("Verifies that AddDataSource properly handles having the same bookmark present multiple times gracefully (i.e. no crash, no data corruption, etc...)")]
		public void TestAddDataSourceDuplicateBookmarks()
		{
			var dataSourceId = DataSourceId.CreateNew();
			_bookmarks.Setup(x => x.All).Returns(new[]
			{
				new BookmarkSettings(dataSourceId, new LogLineIndex(42)),
				new BookmarkSettings(dataSourceId, new LogLineIndex(42)),
			});
			var collection = new BookmarkCollection(_bookmarks.Object, TimeSpan.Zero);
			var dataSource = new Mock<IDataSource>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(new InMemoryLogFile());
			dataSource.Setup(x => x.Id).Returns(dataSourceId);
			new Action(() => collection.AddDataSource(dataSource.Object)).Should().NotThrow();
			collection.Bookmarks.Should().Equal(new object[]
			{
				new Bookmark(dataSource.Object, new LogLineIndex(42))
			}, "because even though there are two bookmarks in the settings object, they describe the same log line and thus only one bookmark should have been added in the end");
		}

		[Test]
		public void TestAddSameBookmarkTwice()
		{
			var collection = new BookmarkCollection(_bookmarks.Object, TimeSpan.Zero);
			collection.AddDataSource(_dataSource.Object);

			_logFile.AddEntry("", LevelFlags.Error);
			_logFile.AddEntry("", LevelFlags.Error);

			var bookmark = collection.TryAddBookmark(_dataSource.Object, 1);
			collection.TryAddBookmark(_dataSource.Object, 1).Should().BeNull();
			collection.Count.Should().Be(1);
			collection.Bookmarks.Should().BeEquivalentTo(new object[] {bookmark});
		}

		[Test]
		public void TestRemoveBookmark()
		{
			var collection = new BookmarkCollection(_bookmarks.Object, TimeSpan.Zero);
			collection.AddDataSource(_dataSource.Object);

			_logFile.AddEntry("", LevelFlags.Error);
			_logFile.AddEntry("", LevelFlags.Error);

			var bookmark = collection.TryAddBookmark(_dataSource.Object, 1);
			_bookmarks.Verify(x => x.SaveAsync(), Times.Once);

			collection.RemoveBookmark(bookmark);
			_bookmarks.Verify(x => x.SaveAsync(), Times.Exactly(2));
			collection.Bookmarks.Should().BeEmpty();
		}
	}
}