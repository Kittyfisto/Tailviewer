using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogEntryBufferTest
		: ReadOnlyLogEntriesTest
	{
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
			buffer.CopyFrom(LogFileColumns.Index, new LogLineIndex[] {1, 42});
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
			buffer.CopyFrom(LogFileColumns.OriginalIndex, Enumerable.Range(0, count).Select(i => (LogLineIndex)i).ToArray());
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

		[Test]
		[Description("Verifies that an entire column can be filled with default values")]
		public void TestFillDefault1([Range(0, 10)] int length)
		{
			var buffer = new LogEntryBuffer(length, LogFileColumns.RawContent);
			var data = Enumerable.Range(0, length).Select(unused => "Foo").ToArray();
			buffer.CopyFrom(LogFileColumns.RawContent, data);

			for(int i = 0; i < length; ++i)
			{
				buffer[i].RawContent.Should().Be("Foo");
			}

			buffer.FillDefault(LogFileColumns.RawContent, 0, length);

			for (int i = 0; i < length; ++i)
			{
				buffer[i].RawContent.Should().BeNull();
			}
		}

		[Test]
		[Description("Verifies that an entire column can be filled with default values")]
		public void TestFillDefault2([Range(0, 10)] int length)
		{
			var buffer = new LogEntryBuffer(length, LogFileColumns.Timestamp);
			var data = Enumerable.Range(0, length).Select(unused => (DateTime?)new DateTime(2017, 12, 12, 18, 58, 0)).ToArray();
			buffer.CopyFrom(LogFileColumns.Timestamp, data);

			for (int i = 0; i < length; ++i)
			{
				buffer[i].Timestamp.Should().Be(new DateTime(2017, 12, 12, 18, 58, 0));
			}

			buffer.FillDefault(LogFileColumns.Timestamp, 0, length);

			for (int i = 0; i < length; ++i)
			{
				buffer[i].Timestamp.Should().BeNull();
			}
		}

		[Test]
		[Description("Verifies that a column can be partially filled with default values")]
		public void TestFillDefault3()
		{
			var buffer = new LogEntryBuffer(4, LogFileColumns.DeltaTime);
			var data = new TimeSpan?[]
			{
				TimeSpan.FromMilliseconds(1),
				TimeSpan.FromMilliseconds(5),
				TimeSpan.FromSeconds(3),
				TimeSpan.FromSeconds(10)
			};
			buffer.CopyFrom(LogFileColumns.DeltaTime, data);
			buffer.FillDefault(LogFileColumns.DeltaTime, 1, 2);

			buffer[0].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));
			buffer[1].DeltaTime.Should().Be(null);
			buffer[2].DeltaTime.Should().Be(null);
			buffer[3].DeltaTime.Should().Be(TimeSpan.FromSeconds(10));
		}

		[Test]
		[Description("Verifies that all columns can be filled with default values")]
		public void TestFillAllColumns1()
		{
			var buffer = new LogEntryBuffer(4, LogFileColumns.DeltaTime, LogFileColumns.Timestamp);
			var deltas = new TimeSpan?[]
			{
				TimeSpan.FromMilliseconds(1),
				TimeSpan.FromMilliseconds(5),
				TimeSpan.FromSeconds(3),
				TimeSpan.FromSeconds(10)
			};
			buffer.CopyFrom(LogFileColumns.DeltaTime, deltas);
			var timestamps = new DateTime?[]
			{
				new DateTime(2017, 12, 12, 19, 24, 0),
				new DateTime(2017, 12, 12, 19, 25, 0),
				new DateTime(2017, 12, 12, 19, 26, 0),
				new DateTime(2017, 12, 12, 19, 27, 0)
			};
			buffer.CopyFrom(LogFileColumns.Timestamp, timestamps);

			buffer[0].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));
			buffer[0].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 24, 0));
			buffer[1].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(5));
			buffer[1].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 25, 0));
			buffer[2].DeltaTime.Should().Be(TimeSpan.FromSeconds(3));
			buffer[2].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 26, 0));
			buffer[3].DeltaTime.Should().Be(TimeSpan.FromSeconds(10));
			buffer[3].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 27, 0));

			buffer.FillDefault(0, 4);
			buffer[0].DeltaTime.Should().BeNull();
			buffer[0].Timestamp.Should().BeNull();
			buffer[1].DeltaTime.Should().BeNull();
			buffer[1].Timestamp.Should().BeNull();
			buffer[2].DeltaTime.Should().BeNull();
			buffer[2].Timestamp.Should().BeNull();
			buffer[3].DeltaTime.Should().BeNull();
			buffer[3].Timestamp.Should().BeNull();
		}

		[Test]
		[Description("Verifies that all columns can be partially filled with default values")]
		public void TestFillAllColumns2()
		{
			var buffer = new LogEntryBuffer(4, LogFileColumns.DeltaTime, LogFileColumns.Timestamp);
			var deltas = new TimeSpan?[]
			{
				TimeSpan.FromMilliseconds(1),
				TimeSpan.FromMilliseconds(5),
				TimeSpan.FromSeconds(3),
				TimeSpan.FromSeconds(10)
			};
			buffer.CopyFrom(LogFileColumns.DeltaTime, deltas);
			var timestamps = new DateTime?[]
			{
				new DateTime(2017, 12, 12, 19, 24, 0),
				new DateTime(2017, 12, 12, 19, 25, 0),
				new DateTime(2017, 12, 12, 19, 26, 0),
				new DateTime(2017, 12, 12, 19, 27, 0)
			};
			buffer.CopyFrom(LogFileColumns.Timestamp, timestamps);

			buffer[0].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));
			buffer[0].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 24, 0));
			buffer[1].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(5));
			buffer[1].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 25, 0));
			buffer[2].DeltaTime.Should().Be(TimeSpan.FromSeconds(3));
			buffer[2].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 26, 0));
			buffer[3].DeltaTime.Should().Be(TimeSpan.FromSeconds(10));
			buffer[3].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 27, 0));

			buffer.FillDefault(1, 2);
			buffer[0].DeltaTime.Should().Be(TimeSpan.FromMilliseconds(1));
			buffer[0].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 24, 0));
			buffer[1].DeltaTime.Should().BeNull();
			buffer[1].Timestamp.Should().BeNull();
			buffer[2].DeltaTime.Should().BeNull();
			buffer[2].Timestamp.Should().BeNull();
			buffer[3].DeltaTime.Should().Be(TimeSpan.FromSeconds(10));
			buffer[3].Timestamp.Should().Be(new DateTime(2017, 12, 12, 19, 27, 0));
		}

		protected override IReadOnlyLogEntries CreateEmpty(IEnumerable<ILogFileColumn> columns)
		{
			return new LogEntryBuffer(0, columns);
		}

		protected override IReadOnlyLogEntries Create(IEnumerable<IReadOnlyLogEntry> entries)
		{
			if (entries.Any())
			{
				var list = new LogEntryBuffer(entries.Count(), entries.First().Columns);
				int i = 0;
				foreach (var entry in entries)
				{
					foreach (var column in entry.Columns)
					{
						var value = entry.GetValue(column);
						list[i].SetValue(column, value);
					}
				}
				return list;
			}

			return CreateEmpty(new ILogFileColumn[0]);
		}
	}
}