using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogEntryBufferTest
	{
		[Test]
		public void TestConstruction1()
		{
			var buffer = new LogEntryBuffer(0);
			buffer.Columns.Should().BeEmpty();
			buffer.Count.Should().Be(0);
			buffer.Should().BeEmpty();
		}

		[Test]
		public void TestConstruction2([Values(1, 2, 5, 10, 42, 100, 9001)] int count)
		{
			var buffer = new LogEntryBuffer(count);
			buffer.Columns.Should().BeEmpty();
			buffer.Count.Should().Be(count);
			buffer.Should().HaveCount(count);
		}

		[Test]
		public void TestConstruction3([Values(1, 2, 5, 10, 42, 100, 9001)] int count)
		{
			var buffer = new LogEntryBuffer(count, new List<ILogFileColumn> {LogFileColumns.RawContent, LogFileColumns.DeltaTime});
			buffer.Columns.Should().Equal(new object[] {LogFileColumns.RawContent, LogFileColumns.DeltaTime });
			buffer.Count.Should().Be(count);
			buffer.Should().HaveCount(count);
		}

		[Test]
		public void TestGetEntryByIndex1([Values(-1, 0, 1)] int invalidIndex)
		{
			var buffer = new LogEntryBuffer(0);
			new Action(() =>
			{
				var unused = buffer[invalidIndex];
			}).ShouldThrow<IndexOutOfRangeException>();
		}

		[Test]
		public void TestCopyFrom()
		{
			var buffer = new LogEntryBuffer(1, LogFileColumns.Timestamp);
			var timestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			buffer.CopyFrom(LogFileColumns.Timestamp, new DateTime?[] {timestamp});
			buffer[0].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		[Description("Verifies that the sourceIndex parameter is honored")]
		public void TestCopyFromPartial1()
		{
			var buffer = new LogEntryBuffer(1, LogFileColumns.Timestamp);
			var unusedTimestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			var timestamp = new DateTime(2017, 12, 11, 21, 43, 0);
			buffer.CopyFrom(LogFileColumns.Timestamp, 0, new DateTime?[] { unusedTimestamp, timestamp }, 1, 1);
			buffer[0].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		[Description("Verifies that the sourceIndex parameter is honored")]
		public void TestCopyFromPartial2()
		{
			var buffer = new LogEntryBuffer(1, LogFileColumns.Timestamp);
			var timestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			var unusedTimestamp = new DateTime(2017, 12, 11, 21, 44, 0);
			buffer.CopyFrom(LogFileColumns.Timestamp, 0, new DateTime?[] { timestamp, unusedTimestamp }, 0, 1);
			buffer[0].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		[Description("Verifies that the destIndex parameter is honored")]
		public void TestCopyFromPartial3()
		{
			var buffer = new LogEntryBuffer(2, LogFileColumns.Timestamp);

			var timestamp = new DateTime(2017, 12, 11, 21, 41, 0);
			buffer.CopyFrom(LogFileColumns.Timestamp, 1, new DateTime?[] { timestamp }, 0, 1);
			buffer[0].Timestamp.Should().BeNull("because we didn't copy any data for this timestamp");
			buffer[1].Timestamp.Should().Be(timestamp, "Because we've just copied this timestamp to the buffer");
		}

		[Test]
		public void TestCopyFromNullColumn()
		{
			var buffer = new LogEntryBuffer(2, LogFileColumns.Timestamp);
			new Action(() => buffer.CopyFrom(null, 0, new string[0], 0, 0))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestCopyFromUnknownColumn()
		{
			var buffer = new LogEntryBuffer(2, LogFileColumns.Timestamp);
			new Action(() => buffer.CopyFrom(LogFileColumns.RawContent, 0, new string[0], 0, 0))
				.ShouldThrow<NoSuchColumnException>();
		}

		[Test]
		public void TestCopyFromMultipleColumns()
		{
			var buffer = new LogEntryBuffer(2,
			                                       LogFileColumns.Index,
			                                       LogFileColumns.Timestamp);
			buffer.CopyFrom(LogFileColumns.Index, new LogEntryIndex[] {1, 42});
			buffer.CopyFrom(LogFileColumns.Timestamp, new DateTime?[] {DateTime.MinValue, DateTime.MaxValue});

			buffer[0].Index.Should().Be(1);
			buffer[0].Timestamp.Should().Be(DateTime.MinValue);

			buffer[1].Index.Should().Be(42);
			buffer[1].Timestamp.Should().Be(DateTime.MaxValue);
		}

		[Test]
		public void TestCopyFromManyRows()
		{
			const int count = 1000;
			var buffer = new LogEntryBuffer(count, LogFileColumns.OriginalIndex);
			buffer.CopyFrom(LogFileColumns.OriginalIndex, Enumerable.Range(0, count).Select(i => (LogEntryIndex)i).ToArray());
			for (int i = 0; i < count; ++i)
			{
				buffer[i].OriginalIndex.Should().Be(i);
			}
		}

		[Test]
		public void TestCopyFromOverwrite()
		{
			var buffer = new LogEntryBuffer(2, LogFileColumns.RawContent);
			buffer[0].RawContent.Should().BeNull();
			buffer[1].RawContent.Should().BeNull();

			buffer.CopyFrom(LogFileColumns.RawContent, new [] {"foo", "bar"});
			buffer[0].RawContent.Should().Be("foo");
			buffer[1].RawContent.Should().Be("bar");
		}

		[Test]
		public void TestAccessUnavailableColumn()
		{
			var buffer = new LogEntryBuffer(1);
			new Action(() => { var unused = buffer[0].RawContent; }).ShouldThrow<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].Index; }).ShouldThrow<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].OriginalIndex; }).ShouldThrow<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].LineNumber; }).ShouldThrow<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].OriginalLineNumber; }).ShouldThrow<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].Timestamp; }).ShouldThrow<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].ElapsedTime; }).ShouldThrow<ColumnNotRetrievedException>();
			new Action(() => { var unused = buffer[0].DeltaTime; }).ShouldThrow<ColumnNotRetrievedException>();
		}
	}
}