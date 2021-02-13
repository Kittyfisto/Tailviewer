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
	public abstract class ReadOnlyLogEntriesTest
	{
		private IReadOnlyLogEntries CreateEmpty(params ILogFileColumnDescriptor[] columns)
		{
			return CreateEmpty((IEnumerable<ILogFileColumnDescriptor>)columns);
		}

		protected abstract IReadOnlyLogEntries CreateEmpty(IEnumerable<ILogFileColumnDescriptor> columns);

		private IReadOnlyLogEntries Create(params IReadOnlyLogEntry[] entries)
		{
			return Create((IEnumerable<IReadOnlyLogEntry>) entries);
		}

		protected abstract IReadOnlyLogEntries Create(IEnumerable<IReadOnlyLogEntry> entries);

		[Test]
		public void TestEmptyConstruction1()
		{
			var entries = CreateEmpty();
			entries.Count.Should().Be(0);
			entries.Columns.Should().BeEmpty();
			entries.Should().BeEmpty();
		}

		[Test]
		public void TestEmptyConstruction2()
		{
			var entries = new LogEntryList(LogFileColumns.DeltaTime, LogFileColumns.ElapsedTime, LogFileColumns.RawContent);
			entries.Count.Should().Be(0);
			entries.Columns.Should()
			       .Equal(new object[] { LogFileColumns.DeltaTime, LogFileColumns.ElapsedTime, LogFileColumns.RawContent },
			              "because the order columns should've been preserved");
		}

		[Test]
		public void TestCopyToRangeUnknownColumn()
		{
			var buffer = CreateEmpty(LogFileColumns.DeltaTime);
			new Action(() => buffer.CopyTo(LogFileColumns.ElapsedTime, 0, new TimeSpan?[0], 0, 0))
				.Should().Throw<NoSuchColumnException>();
		}

		[Test]
		public void TestCopyToByIndicesUnknownColumn()
		{
			var buffer = CreateEmpty(LogFileColumns.DeltaTime);
			new Action(() => buffer.CopyTo(LogFileColumns.ElapsedTime, new int[0], new TimeSpan?[0], 0))
				.Should().Throw<NoSuchColumnException>();
		}

		[Test]
		public void TestCopyToRangeNullColumn()
		{
			var buffer = CreateEmpty(LogFileColumns.DeltaTime);
			new Action(() => buffer.CopyTo(null, 0, new TimeSpan?[0], 0, 0))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestCopyToByIndicesNullColumn()
		{
			var buffer = CreateEmpty(LogFileColumns.DeltaTime);
			new Action(() => buffer.CopyTo(null, new int[0], new TimeSpan?[0], 0))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestCopyToRangeNullDestination()
		{
			var buffer = CreateEmpty(LogFileColumns.RawContent);
			new Action(() => buffer.CopyTo(LogFileColumns.RawContent, 0, null, 0, 0))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestCopyToByIndicesNullDestination()
		{
			var buffer = CreateEmpty(LogFileColumns.RawContent);
			new Action(() => buffer.CopyTo(LogFileColumns.RawContent, new int[0], null, 0))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that regions before the buffer ")]
		public void TestCopyToRangeInvalidSourceIndex1()
		{
			var buffer = CreateEmpty(LogFileColumns.RawContent);
			var destination = new[] {"foo"};
			new Action(() => buffer.CopyTo(LogFileColumns.RawContent, -1, destination, 0, 1))
				.Should().NotThrow();
			destination[0].Should().BeNull();
		}

		[Test]
		[Description("Verifies that invalid index accesses are treated as default values for that index")]
		public void TestCopyToRangeInvalidSourceIndex2()
		{
			var entry = ReadOnlyLogEntry.Create(new[] { LogFileColumns.RawContent },
			                                    new[] { "stuff" });
			var buffer = Create(entry);
			var destination = new[] { "f", "o", "o" };
			new Action(() => buffer.CopyTo(LogFileColumns.RawContent, 0, destination, 0, 3))
				.Should().NotThrow();
			destination[0].Should().Be("stuff");
			destination[1].Should().BeNull("because the second index we access (1) isn't valid and thus should be served with null");
			destination[2].Should().BeNull("because the last index we access (2) isn't valid and thus should be served with null");
		}

		[Test]
		[Description("Verifies that invalid index accesses are treated as default values for that index")]
		public void TestCopyToRangeInvalidSourceIndex3()
		{
			var entry = ReadOnlyLogEntry.Create(new[] {LogFileColumns.RawContent},
			                                    new[] {"stuff"});
			var buffer = Create(entry);
			var destination = new[] { "f", "o", "o" };
			new Action(() => buffer.CopyTo(LogFileColumns.RawContent, -1, destination, 0, 3))
				.Should().NotThrow();
			destination[0].Should().BeNull("because the first index we access (-1) isn't valid and thus should be served with null");
			destination[1].Should().Be("stuff");
			destination[2].Should().BeNull("because the last index we access (1) isn't valid and thus should be served with null");
		}

		[Test]
		[Description("Verifies that invalid index accesses are treated as default values for that index")]
		public void TestCopyToByIndicesInvalidSourceIndex()
		{
			var buffer = CreateEmpty(LogFileColumns.RawContent);
			var destination = new[] {"foo"};
			new Action(() => buffer.CopyTo(LogFileColumns.RawContent, new[] {-1}, destination, 0))
				.Should().NotThrow();
			destination[0].Should().BeNull();
		}

		[Test]
		[Description("Verifies that invalid index accesses are treated as default values for that index")]
		public void TestCopyToPartiallyInvalidSourceIndices()
		{
			var entry = ReadOnlyLogEntry.Create(new[] {LogFileColumns.RawContent}, new[] {"bar"});
			var buffer = Create(entry);
			var destination = new[] { "foo", "stuff" };
			new Action(() => buffer.CopyTo(LogFileColumns.RawContent, new[] { -1, 0 }, destination, 0))
				.Should().NotThrow();
			destination[0].Should().BeNull();
			destination[1].Should().Be("bar");
		}

		[Test]
		public void TestCopyToNullIndices()
		{
			var buffer = CreateEmpty(LogFileColumns.RawContent);
			new Action(() => buffer.CopyTo(LogFileColumns.RawContent, null, new string[0], 0))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestCopyToRange([Range(0, 2)] int sourceIndex,
		                            [Range(0, 2)] int destinationIndex,
		                            [Range(1, 2)] int count)
		{
			var entries = Enumerable.Range(0, sourceIndex + count).Select(i => ReadOnlyLogEntry.Create(new[] {LogFileColumns.DeltaTime},
			                                                                             new object[] {TimeSpan.FromSeconds(i)}));
			var buffer = Create(entries);

			var dest = new TimeSpan?[destinationIndex + count];
			for (int i = 0; i < destinationIndex; ++i)
			{
				dest[i] = TimeSpan.FromTicks(42);
			}

			buffer.CopyTo(LogFileColumns.DeltaTime, sourceIndex, dest, destinationIndex, count);
			for (int i = 0; i < destinationIndex; ++i)
			{
				dest[i].Should().Be(TimeSpan.FromTicks(42));
			}

			for (int i = 0; i < count; ++i)
			{
				var actual = dest[destinationIndex + i];
				var expected = buffer[sourceIndex + i].DeltaTime;
				actual.Should().Be(expected);
			}
		}

		[Test]
		public void TestCopyToByIndices([Range(0, 2)] int destinationIndex)
		{
			var entries = Enumerable.Range(0, 5)
			                        .Select(i => ReadOnlyLogEntry.Create(new[] {LogFileColumns.DeltaTime},
			                                                             new object[] {TimeSpan.FromSeconds(i)}));
			var source = Create(entries);

			var dest = new TimeSpan?[destinationIndex + 4];
			for (int i = 0; i < destinationIndex; ++i)
			{
				dest[i] = TimeSpan.FromTicks(42);
			}

			var sourceIndices = new[]
			{
				4, 1, 2, 3
			};
			source.CopyTo(LogFileColumns.DeltaTime, sourceIndices, dest, destinationIndex);
			for (int i = 0; i < destinationIndex; ++i)
			{
				dest[i].Should().Be(TimeSpan.FromTicks(42));
			}

			dest[destinationIndex + 0].Should().Be(source[4].DeltaTime);
			dest[destinationIndex + 1].Should().Be(source[1].DeltaTime);
			dest[destinationIndex + 2].Should().Be(source[2].DeltaTime);
			dest[destinationIndex + 3].Should().Be(source[3].DeltaTime);
		}

		[Test]
		public void TestReuseEnumerator()
		{
			var entry = ReadOnlyLogEntry.Create(new[] {LogFileColumns.RawContent},
			                                    new[] {"Foo"});
			var entries = Create(entry);

			using (var enumerator = entries.GetEnumerator())
			{
				enumerator.MoveNext().Should().BeTrue();
				enumerator.Current.RawContent.Should().Be("Foo");
				enumerator.MoveNext().Should().BeFalse();

				enumerator.Reset();
				enumerator.MoveNext().Should().BeTrue();
				enumerator.Current.RawContent.Should().Be("Foo");
				enumerator.MoveNext().Should().BeFalse();
			}
		}

		[Test]
		public void TestAccessAllColumnsByLogEntry()
		{
			var columns = new List<ILogFileColumnDescriptor>();
			var values = new List<object>();

			columns.Add(LogFileColumns.RawContent);
			values.Add("Foo");

			columns.Add(LogFileColumns.DeltaTime);
			values.Add(TimeSpan.FromSeconds(10));

			columns.Add(LogFileColumns.ElapsedTime);
			values.Add(TimeSpan.FromDays(44));

			columns.Add(LogFileColumns.Index);
			values.Add(new LogLineIndex(10));

			columns.Add(LogFileColumns.OriginalIndex);
			values.Add(new LogLineIndex(9001));

			columns.Add(LogFileColumns.LineNumber);
			values.Add(5);

			columns.Add(LogFileColumns.OriginalLineNumber);
			values.Add(7);

			columns.Add(LogFileColumns.LogEntryIndex);
			values.Add(new LogEntryIndex(1));

			columns.Add(LogFileColumns.Timestamp);
			values.Add(new DateTime(2017, 12, 19, 12, 45, 33));

			columns.Add(LogFileColumns.LogLevel);
			values.Add(LevelFlags.Error);

			var entry = ReadOnlyLogEntry.Create(columns, values);
			var logEntries = Create(entry);
			var actualEntry = logEntries.First();
			actualEntry.RawContent.Should().Be("Foo");
			actualEntry.DeltaTime.Should().Be(TimeSpan.FromSeconds(10));
			actualEntry.ElapsedTime.Should().Be(TimeSpan.FromDays(44));
			actualEntry.Index.Should().Be(10);
			actualEntry.OriginalIndex.Should().Be(9001);
			actualEntry.LineNumber.Should().Be(5);
			actualEntry.OriginalLineNumber.Should().Be(7);
			actualEntry.LogEntryIndex.Should().Be(1);
			actualEntry.Timestamp.Should().Be(new DateTime(2017, 12, 19, 12, 45, 33));
			actualEntry.LogLevel.Should().Be(LevelFlags.Error);
		}

		[Test]
		public void TestAccessNotRetrievedColumn()
		{
			var entry = ReadOnlyLogEntry.Create(new[] {LogFileColumns.RawContent},
			                                    new[] {"Foo"});
			var entries = Create(entry);
			var actualEntry = entries.First();
			actualEntry.RawContent.Should().Be("Foo");
			new Action(() =>
			{
				var unused = actualEntry.Timestamp;
			}).Should().Throw<ColumnNotRetrievedException>();
		}
	}
}
