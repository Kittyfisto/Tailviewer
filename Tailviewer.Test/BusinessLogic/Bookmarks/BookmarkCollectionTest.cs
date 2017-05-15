using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.Bookmarks
{
	[TestFixture]
	public sealed class BookmarkCollectionTest
	{
		private InMemoryLogFile _logFile;
		private Mock<IDataSource> _dataSource;

		[SetUp]
		public void Setup()
		{
			_logFile = new InMemoryLogFile();
			_dataSource = new Mock<IDataSource>();
			_dataSource.Setup(x => x.UnfilteredLogFile).Returns(_logFile);
		}

		[Test]
		public void TestConstruction1()
		{
			var collection = new BookmarkCollection(TimeSpan.Zero);
			collection.Count.Should().Be(0);
			collection.Bookmarks.Should().BeEmpty();

			collection.Contains(null, 0).Should().BeFalse();
			collection.Contains(_dataSource.Object, -1).Should().BeFalse();
			collection.Contains(_dataSource.Object, 0).Should().BeFalse();
			collection.Contains(_dataSource.Object, 1).Should().BeFalse();
			collection.Contains(_dataSource.Object, 2).Should().BeFalse();
		}

		[Test]
		public void TestAddOneBookmark()
		{
			var collection = new BookmarkCollection(TimeSpan.Zero);
			collection.AddDataSource(_dataSource.Object);

			_logFile.AddEntry("", LevelFlags.Error);
			_logFile.AddEntry("", LevelFlags.Error);

			var bookmark = collection.TryAddBookmark(_dataSource.Object, 1);
			bookmark.Should().NotBeNull();
			bookmark.Index.Should().Be(new LogLineIndex(1));
			collection.Count.Should().Be(1);
			collection.Bookmarks.Should().BeEquivalentTo(new object[] {bookmark});
		}

		[Test]
		public void TestAddTwoBookmarks()
		{
			var collection = new BookmarkCollection(TimeSpan.Zero);
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
		public void TestAddSameBookmarkTwice()
		{
			var collection = new BookmarkCollection(TimeSpan.Zero);
			collection.AddDataSource(_dataSource.Object);

			_logFile.AddEntry("", LevelFlags.Error);
			_logFile.AddEntry("", LevelFlags.Error);

			var bookmark = collection.TryAddBookmark(_dataSource.Object, 1);
			collection.TryAddBookmark(_dataSource.Object, 1).Should().BeNull();
			collection.Count.Should().Be(1);
			collection.Bookmarks.Should().BeEquivalentTo(new object[] {bookmark});
		}
	}
}