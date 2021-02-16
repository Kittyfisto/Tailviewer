using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Sources.Merged;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Merged
{
	[TestFixture]
	public sealed class MergedLogFileChangesTest
	{
		[Test]
		public void TestEmpty([Values(0, 1, 42)] int count)
		{
			var changes = new MergedLogFileChanges(count);
			changes.Sections.Should().BeEmpty();
			changes.TryGetFirstInvalidationIndex(out var index).Should().BeFalse();
			index.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		public void TestAppend()
		{
			var changes = new MergedLogFileChanges(101);
			changes.Append(101, 5);
			changes.Sections.Should().Equal(new object[]
			{
				new LogFileSection(101, 5)
			});

			changes.TryGetFirstInvalidationIndex(out var index).Should().BeFalse();
			index.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		public void TestAppendTwice()
		{
			var changes = new MergedLogFileChanges(101);
			changes.Append(101, 6);
			changes.Append(107, 10);
			changes.Sections.Should().Equal(new object[]
			{
				new LogFileSection(101, 16)
			});
		}

		[Test]
		public void TestAppendOverlap()
		{
			var changes = new MergedLogFileChanges(0);
			changes.Append(0, 2);
			changes.Append(1, 2);
			changes.Sections.Should().Equal(new object[]
			{
				new LogFileSection(0, 3)
			});
		}

		[Test]
		public void TestAppendGap()
		{
			var changes = new MergedLogFileChanges(0);
			changes.Append(0, 2);
			changes.Append(1, 2);
			changes.Sections.Should().Equal(new object[]
			{
				new LogFileSection(0, 3)
			});
		}

		[Test]
		public void TestInvalidate()
		{
			var changes = new MergedLogFileChanges(42);
			changes.InvalidateFrom(10);

			changes.Sections.Should().Equal(new object[]
			{
				LogFileSection.Invalidate(10, 32)
			});

			changes.TryGetFirstInvalidationIndex(out var index).Should().BeTrue();
			index.Should().Be(new LogLineIndex(10));
		}

		[Test]
		public void TestInvalidateTwice()
		{
			var changes = new MergedLogFileChanges(13);
			changes.InvalidateFrom(7);
			changes.InvalidateFrom(5);

			changes.Sections.Should().Equal(new object[]
			{
				LogFileSection.Invalidate(5, 8)
			});


			changes.TryGetFirstInvalidationIndex(out var index).Should().BeTrue();
			index.Should().Be(new LogLineIndex(5));
		}

		[Test]
		public void TestInvalidateTwiceSuperfluous()
		{
			var changes = new MergedLogFileChanges(13);
			changes.InvalidateFrom(5);
			changes.InvalidateFrom(7);

			changes.Sections.Should().Equal(new object[]
			{
				LogFileSection.Invalidate(5, 8)
			});

			changes.TryGetFirstInvalidationIndex(out var index).Should().BeTrue();
			index.Should().Be(new LogLineIndex(5));
		}

		[Test]
		public void TestInvalidateThenAppend()
		{
			var changes = new MergedLogFileChanges(10);
			changes.InvalidateFrom(5);
			changes.Append(5, 6);

			changes.Sections.Should().Equal(new object[]
			{
				LogFileSection.Invalidate(5, 5),
				new LogFileSection(5, 6)
			});

			changes.TryGetFirstInvalidationIndex(out var index).Should().BeTrue();
			index.Should().Be(new LogLineIndex(5));
		}

		[Test]
		public void TestInvalidateThenAppendWithGap()
		{
			var changes = new MergedLogFileChanges(10);
			changes.InvalidateFrom(5);
			changes.Append(7, 10);

			changes.Sections.Should().Equal(new object[]
			{
				LogFileSection.Invalidate(5, 5),
				new LogFileSection(5, 12)
			});
		}

		[Test]
		public void TestAppendInvalidate()
		{
			var changes = new MergedLogFileChanges(0);
			changes.Append(0, 42);
			changes.InvalidateFrom(20);

			changes.Sections.Should().Equal(new object[]
			{
				new LogFileSection(0, 20)
			});
		}

		[Test]
		public void TestAppendInvalidateAppend()
		{
			var changes = new MergedLogFileChanges(0);
			changes.Append(0, 2);
			changes.InvalidateFrom(1);
			changes.Append(1, 1);

			changes.Sections.Should().Equal(new object[]
			{
				new LogFileSection(0, 2)
			});
		}
	}
}
