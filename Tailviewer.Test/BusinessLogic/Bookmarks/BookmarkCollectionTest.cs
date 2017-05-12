using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.Bookmarks
{
	[TestFixture]
	public sealed class BookmarkCollectionTest
	{
		private InMemoryLogFile _logFile;

		[SetUp]
		public void Setup()
		{
			_logFile = new InMemoryLogFile();
		}

		[Test]
		public void TestConstruction1()
		{
			var collection = new BookmarkCollection(_logFile, TimeSpan.Zero);
			collection.Count.Should().Be(0);
			collection.Bookmarks.Should().BeEmpty();

			collection.Contains(0).Should().BeFalse();
			collection.Contains(1).Should().BeFalse();
			collection.Contains(2).Should().BeFalse();

			collection.ContainsAll(Enumerable.Empty<LogLineIndex>()).Should().BeTrue();
			collection.ContainsAll(new LogLineIndex[] {1, 2}).Should().BeFalse();
		}

		[Test]
		public void TestAddOneBookmark()
		{
			var collection = new BookmarkCollection(_logFile, TimeSpan.Zero);

			_logFile.AddEntry("", LevelFlags.Error);
			_logFile.AddEntry("", LevelFlags.Error);

			var bookmark = collection.TryAddBookmark(1);
			bookmark.Should().NotBeNull();
			bookmark.Index.Should().Be(new LogLineIndex(1));
			collection.Count.Should().Be(1);
			collection.Bookmarks.Should().BeEquivalentTo(new object[] {bookmark});
		}

		[Test]
		public void TestAddTwoBookmarks()
		{
			var collection = new BookmarkCollection(_logFile, TimeSpan.Zero);

			_logFile.AddEntry("", LevelFlags.Error);
			_logFile.AddEntry("", LevelFlags.Error);

			var bookmark1 = collection.TryAddBookmark(1);
			var bookmark2 = collection.TryAddBookmark(0);
			bookmark2.Should().NotBeNull();
			bookmark2.Index.Should().Be(new LogLineIndex(0));
			collection.Count.Should().Be(2);
			collection.Bookmarks.Should().BeEquivalentTo(new object[] { bookmark1, bookmark2 });
		}

		[Test]
		public void TestAddSameBookmarkTwice()
		{
			var collection = new BookmarkCollection(_logFile, TimeSpan.Zero);

			_logFile.AddEntry("", LevelFlags.Error);
			_logFile.AddEntry("", LevelFlags.Error);

			var bookmark = collection.TryAddBookmark(1);
			collection.TryAddBookmark(1).Should().BeNull();
			collection.Count.Should().Be(1);
			collection.Bookmarks.Should().BeEquivalentTo(new object[] {bookmark});
		}
	}
}