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
	public sealed class LogEntryListTest
		: ReadOnlyLogEntriesTest
	{
		[Test]
		public void TestConstruction2()
		{
			var entries = new LogEntryList(LogFileColumns.DeltaTime, LogFileColumns.ElapsedTime, LogFileColumns.RawContent);
			entries.Count.Should().Be(0);
			entries.Columns.Should()
			       .Equal(new object[] {LogFileColumns.DeltaTime, LogFileColumns.ElapsedTime, LogFileColumns.RawContent},
			              "because the order columns should've been preserved");
		}

		[Test]
		public void TestClearEmpty()
		{
			var entries = new LogEntryList(LogFileColumns.ElapsedTime);
			entries.Count.Should().Be(0);
			entries.Columns.Should().Equal(LogFileColumns.ElapsedTime);

			entries.Clear();
			entries.Count.Should().Be(0);
			entries.Columns.Should().Equal(LogFileColumns.ElapsedTime);
		}

		[Test]
		public void TestClearOneEntry()
		{
			var entries = new LogEntryList();

			entries.Add();
			entries.Count.Should().Be(1);

			entries.Clear();
			entries.Count.Should().Be(0);
		}

		[Test]
		public void TestClearMany()
		{
			var entries = new LogEntryList(LogFileColumns.LineNumber);
			entries.Add(42);
			entries.Add(9001);
			entries.Count.Should().Be(2);

			entries.Clear();
			entries.Count.Should().Be(0);

			entries.AddEmpty();
			entries.AddEmpty();
			entries.Count.Should().Be(2);
			entries[0].LineNumber.Should().Be(0);
			entries[1].LineNumber.Should().Be(0);
		}

		[Test]
		public void TestAccessInvalidIndex([Range(-1, 1)] int invalidIndex)
		{
			var entries = new LogEntryList(LogFileColumns.LineNumber);
			new Action(() =>
			{
				var unused = entries[invalidIndex];
			}).ShouldThrow<ArgumentOutOfRangeException>();
		}

		[Test]
		public void TestAddEmpty1()
		{
			var entries = new LogEntryList(LogFileColumns.LineNumber);

			entries.AddEmpty();
			entries.Count.Should().Be(1);
			entries[0].LineNumber.Should().Be(0);
		}

		[Test]
		public void TestAddEmpty2()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);

			entries.AddEmpty();
			entries.Count.Should().Be(1);
			entries[0].RawContent.Should().Be(null);
		}

		[Test]
		public void TestAddEmpty3()
		{
			var entries = new LogEntryList(LogFileColumns.Timestamp);

			entries.AddEmpty();
			entries.Count.Should().Be(1);
			entries[0].Timestamp.Should().Be(null);
		}

		[Test]
		public void TestAddEmpty4()
		{
			var entries = new LogEntryList(LogFileColumns.DeltaTime);

			entries.AddEmpty();
			entries.Count.Should().Be(1);
			entries[0].DeltaTime.Should().Be(null);
		}

		[Test]
		public void TestAddOneEntry()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Count.Should().Be(0);

			entries.Add(ReadOnlyLogEntry.Create(new[] {LogFileColumns.RawContent},
			                                    new[] {"Foobar"}));
			entries.Count.Should().Be(1);
			entries[0].RawContent.Should().Be("Foobar");
		}

		[Test]
		[Description("Verifies that adding a log entry which doesn't contain all columns is allowed - all other columns in that row will be filled with that columns default value")]
		public void TestAddPartialEntry()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent, LogFileColumns.Timestamp);

			var logEntry = new LogEntry2();
			logEntry.Add(LogFileColumns.RawContent, "Foobar");
			entries.Add(logEntry);

			logEntry = new LogEntry2();
			logEntry.Add(LogFileColumns.Timestamp, new DateTime(2017, 12, 19, 16, 08, 0));
			entries.Add(logEntry);

			entries[0].RawContent.Should().Be("Foobar");
			entries[0].Timestamp.Should().BeNull();

			entries[1].RawContent.Should().BeNull();
			entries[1].Timestamp.Should().Be(new DateTime(2017, 12, 19, 16, 08, 0));
		}

		[Test]
		public void TestAddManyEntries()
		{
			const int count = 101;
			var entries = new LogEntryList(LogFileColumns.Index, LogFileColumns.RawContent);
			for (int i = 0; i < count; ++i)
			{
				entries.Add(new LogLineIndex(i), i.ToString());
				entries.Count.Should().Be(i + 1);
			}

			for (int i = 0; i < count; ++i)
			{
				entries[i].Index.Should().Be(i);
				entries[i].RawContent.Should().Be(i.ToString());
			}
		}

		[Test]
		public void TestRemoveAtInvalidIndex([Values(-1, 1, 2)] int invalidIndex)
		{
			var entries = new LogEntryList(LogFileColumns.DeltaTime, LogFileColumns.RawContent);
			entries.Add(TimeSpan.FromSeconds(44), "stuff");
			entries.Count.Should().Be(1);
			entries[0].DeltaTime.Should().Be(TimeSpan.FromSeconds(44));
			entries[0].RawContent.Should().Be("stuff");

			new Action(() => entries.RemoveAt(invalidIndex)).ShouldThrow<ArgumentOutOfRangeException>();
			entries.Count.Should().Be(1);
			entries[0].DeltaTime.Should().Be(TimeSpan.FromSeconds(44));
			entries[0].RawContent.Should().Be("stuff");
		}

		[Test]
		public void TestRemoveAt()
		{
			var entries = new LogEntryList(LogFileColumns.DeltaTime);
			entries.Add(TimeSpan.FromSeconds(5));
			entries.Add(TimeSpan.FromSeconds(6));
			entries.Add(TimeSpan.FromSeconds(7));

			entries.Count.Should().Be(3);
			entries[0].DeltaTime.Should().Be(TimeSpan.FromSeconds(5));
			entries[1].DeltaTime.Should().Be(TimeSpan.FromSeconds(6));
			entries[2].DeltaTime.Should().Be(TimeSpan.FromSeconds(7));

			entries.RemoveAt(2);
			entries.Count.Should().Be(2);
			entries[0].DeltaTime.Should().Be(TimeSpan.FromSeconds(5));
			entries[1].DeltaTime.Should().Be(TimeSpan.FromSeconds(6));

			entries.RemoveAt(0);
			entries.Count.Should().Be(1);
			entries[0].DeltaTime.Should().Be(TimeSpan.FromSeconds(6));

			entries.RemoveAt(0);
			entries.Count.Should().Be(0);
		}

		[Test]
		public void TestInsertOneEntry()
		{
			var entries = new LogEntryList(LogFileColumns.DeltaTime);
			entries.Insert(0, ReadOnlyLogEntry.Create(new[] {LogFileColumns.DeltaTime},
			                                          new object[] {TimeSpan.FromHours(22)}));
			entries.Count.Should().Be(1);
			entries[0].DeltaTime.Should().Be(TimeSpan.FromHours(22));
		}

		[Test]
		public void TestInsertMany()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Insert(0, "r");
			entries.Insert(0, "a");
			entries.Insert(0, "b");
			entries.Insert(0, "o");
			entries.Insert(0, "o");
			entries.Insert(0, "f");

			entries.Count.Should().Be(6);
			entries[0].RawContent.Should().Be("f");
			entries[1].RawContent.Should().Be("o");
			entries[2].RawContent.Should().Be("o");
			entries[3].RawContent.Should().Be("b");
			entries[4].RawContent.Should().Be("a");
			entries[5].RawContent.Should().Be("r");
		}

		[Test]
		public void TestInsertInvalidIndex([Values(-2, -1, 1, 42)] int invalidIndex)
		{
			var entries = new LogEntryList(LogFileColumns.ElapsedTime);

			var logEntry = ReadOnlyLogEntry.Create(new[] {LogFileColumns.ElapsedTime },
			                                       new object[] {TimeSpan.FromHours(22)});
			new Action(() => entries.Insert(invalidIndex, logEntry))
				.ShouldThrow<ArgumentOutOfRangeException>();
			entries.Count.Should().Be(0);
		}

		[Test]
		public void TestInsertEmpty1()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Add("Foo");
			entries.Count.Should().Be(1);

			entries.InsertEmpty(1);
			entries.Count.Should().Be(2);
			entries[0].RawContent.Should().Be("Foo");
			entries[1].RawContent.Should().BeNull();
		}

		[Test]
		public void TestInsertEmpty2()
		{
			var entries = new LogEntryList(LogFileColumns.DeltaTime);
			entries.Add(TimeSpan.FromSeconds(10));
			entries.Count.Should().Be(1);

			entries.InsertEmpty(1);
			entries.Count.Should().Be(2);
			entries[0].DeltaTime.Should().Be(TimeSpan.FromSeconds(10));
			entries[1].DeltaTime.Should().BeNull();
		}

		[Test]
		public void TestInsertEmptyInvalidIndex([Values(-2, -1, 1, 42)] int invalidIndex)
		{
			var entries = new LogEntryList(LogFileColumns.DeltaTime);
			entries.Count.Should().Be(0);
			new Action(() => entries.InsertEmpty(invalidIndex)).ShouldThrow<ArgumentOutOfRangeException>();
			entries.Count.Should().Be(0);
		}

		[Test]
		public void TestRemoveRangeInvalidIndex1([Values(-2, -1)] int invalidIndex)
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Add("foo");
			entries.Add("bar");
			entries.Count.Should().Be(2);

			new Action(() => entries.RemoveRange(invalidIndex, 1)).ShouldThrow<ArgumentOutOfRangeException>();
			entries.Count.Should().Be(2);
			entries[0].RawContent.Should().Be("foo");
			entries[1].RawContent.Should().Be("bar");
		}

		[Test]
		public void TestRemoveRangeInvalidIndex2([Values(3, 42)] int invalidIndex)
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Add("foo");
			entries.Add("bar");
			entries.Count.Should().Be(2);

			new Action(() => entries.RemoveRange(invalidIndex, 1)).ShouldThrow<ArgumentException>();
			entries.Count.Should().Be(2);
			entries[0].RawContent.Should().Be("foo");
			entries[1].RawContent.Should().Be("bar");
		}

		[Test]
		public void TestRemoveRangePartiallyInvalidRange([Values(0, 1, 2)] int index)
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Add("f");
			entries.Add("o");
			entries.Add("o");
			entries.Add("b");
			entries.Add("a");
			entries.Add("r");
			entries.Count.Should().Be(6);

			new Action(() => entries.RemoveRange(index, 7)).ShouldThrow<ArgumentException>();
			entries.Count.Should().Be(6);
			entries[0].RawContent.Should().Be("f");
			entries[1].RawContent.Should().Be("o");
			entries[2].RawContent.Should().Be("o");
			entries[3].RawContent.Should().Be("b");
			entries[4].RawContent.Should().Be("a");
			entries[5].RawContent.Should().Be("r");
		}

		[Test]
		public void TestEnumerateEmpty()
		{
			var entries = new LogEntryList();
			entries.Should().Equal(new object[0]);
		}

		[Test]
		public void TestAccessCurrentWithoutMove()
		{
			var entries = new LogEntryList();
			using (var enumerator = entries.GetEnumerator())
			{
				new Action(() =>
				{
					var unused = enumerator.Current;
				}).ShouldThrow<InvalidOperationException>();
			}
		}

		[Test]
		public void TestEnumerateItems([Range(1, 10)] int count)
		{
			var entries = new LogEntryList(LogFileColumns.LineNumber);
			for (int i = 0; i < count; ++i)
			{
				entries.Add(42+i);
			}

			int n = 0;
			foreach (var logEntry in entries)
			{
				logEntry.LineNumber.Should().Be(42 + n);

				++n;
			}
		}

		protected override IReadOnlyLogEntries CreateEmpty(IEnumerable<ILogFileColumn> columns)
		{
			return new LogEntryList(columns);
		}

		protected override IReadOnlyLogEntries Create(IEnumerable<IReadOnlyLogEntry> entries)
		{
			if (entries.Any())
			{
				var list =  new LogEntryList(entries.First().Columns);
				foreach (var entry in entries)
				{
					list.Add(entry);
				}
				return list;
			}

			return CreateEmpty(new ILogFileColumn[0]);
		}
	}
}