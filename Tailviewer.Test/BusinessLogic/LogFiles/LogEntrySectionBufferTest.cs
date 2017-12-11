using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogEntrySectionBufferTest
	{
		[Test]
		public void TestConstruction1()
		{
			var buffer = new LogEntrySectionBuffer(new LogFileSection());
			buffer.Columns.Should().BeEmpty();
			buffer.Count.Should().Be(0);
			buffer.Should().BeEmpty();
		}

		[Test]
		public void TestConstruction2([Values(1, 2, 5, 10, 42, 100, 9001)] int count)
		{
			var buffer = new LogEntrySectionBuffer(new LogFileSection(42, count));
			buffer.Columns.Should().BeEmpty();
			buffer.Count.Should().Be(count);
			buffer.Should().HaveCount(count);
		}

		[Test]
		public void TestGetEntryByIndex1([Values(-1, 0, 1)] int invalidIndex)
		{
			var buffer = new LogEntrySectionBuffer(new LogFileSection());
			new Action(() =>
			{
				var unused = buffer[invalidIndex];
			}).ShouldThrow<IndexOutOfRangeException>();
		}

		[Test]
		public void TestCopyFrom()
		{
			var buffer = new LogEntrySectionBuffer(new LogFileSection(42, 1), LogFileColumns.TimeStamp);
			var timestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			buffer.CopyFrom(LogFileColumns.TimeStamp, new DateTime?[] {timestamp});
			buffer[0].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		[Description("Verifies that the sourceIndex parameter is honored")]
		public void TestCopyFromPartial1()
		{
			var buffer = new LogEntrySectionBuffer(new LogFileSection(42, 1), LogFileColumns.TimeStamp);
			var unusedTimestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			var timestamp = new DateTime(2017, 12, 11, 21, 43, 0);
			buffer.CopyFrom(LogFileColumns.TimeStamp, 0, new DateTime?[] { unusedTimestamp, timestamp }, 1, 1);
			buffer[0].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		[Description("Verifies that the sourceIndex parameter is honored")]
		public void TestCopyFromPartial2()
		{
			var buffer = new LogEntrySectionBuffer(new LogFileSection(42, 1), LogFileColumns.TimeStamp);
			var timestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			var unusedTimestamp = new DateTime(2017, 12, 11, 21, 44, 0);
			buffer.CopyFrom(LogFileColumns.TimeStamp, 0, new DateTime?[] { timestamp, unusedTimestamp }, 0, 1);
			buffer[0].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		[Description("Verifies that the destIndex parameter is honored")]
		public void TestCopyFromPartial3()
		{
			var buffer = new LogEntrySectionBuffer(new LogFileSection(42, 2), LogFileColumns.TimeStamp);

			var timestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			buffer.CopyFrom(LogFileColumns.TimeStamp, 1, new DateTime?[] { timestamp }, 0, 1);
			buffer[0].Timestamp.Should().BeNull("because we didn't copy any data for this timestamp");
			buffer[1].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		public void TestCopyFromNullColumn()
		{
			var buffer = new LogEntrySectionBuffer(new LogFileSection(42, 2), LogFileColumns.TimeStamp);
			new Action(() => buffer.CopyFrom(null, 0, new string[0], 0, 0))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestCopyFromUnknownColumn()
		{
			var buffer = new LogEntrySectionBuffer(new LogFileSection(42, 2), LogFileColumns.TimeStamp);
			new Action(() => buffer.CopyFrom(LogFileColumns.RawContent, 0, new string[0], 0, 0))
				.ShouldThrow<NoSuchColumnException>();
		}

		[Test]
		public void TestCopyFromMultipleColumns()
		{
			var buffer = new LogEntrySectionBuffer(new LogFileSection(0, 2),
			                                       LogFileColumns.Index,
			                                       LogFileColumns.TimeStamp);
			buffer.CopyFrom(LogFileColumns.Index, new LogLineIndex[] {1, 42});
			buffer.CopyFrom(LogFileColumns.TimeStamp, new DateTime?[] {DateTime.MinValue, DateTime.MaxValue});

			buffer[0].Index.Should().Be(1);
			buffer[0].Timestamp.Should().Be(DateTime.MinValue);

			buffer[1].Index.Should().Be(42);
			buffer[1].Timestamp.Should().Be(DateTime.MaxValue);
		}

		[Test]
		public void TestCopyFromManyRows()
		{
			const int count = 1000;
			var buffer = new LogEntrySectionBuffer(new LogFileSection(0, count), LogFileColumns.OriginalIndex);
			buffer.CopyFrom(LogFileColumns.OriginalIndex, Enumerable.Range(0, count).Select(i => (LogLineIndex)i).ToArray());
			for (int i = 0; i < count; ++i)
			{
				buffer[i].OriginalIndex.Should().Be(i);
			}
		}

		[Test]
		public void TestCopyFromOverwrite()
		{
			var buffer = new LogEntrySectionBuffer(new LogFileSection(0, 2), LogFileColumns.RawContent);
			buffer[0].RawContent.Should().BeNull();
			buffer[1].RawContent.Should().BeNull();

			buffer.CopyFrom(LogFileColumns.RawContent, new [] {"foo", "bar"});
			buffer[0].RawContent.Should().Be("foo");
			buffer[1].RawContent.Should().Be("bar");
		}
	}
}