using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

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
			              "because the order columns should have been preserved");
		}

		[Test]
		public void TestContains()
		{
			var entries = new LogEntryList(LogFileColumns.DeltaTime, LogFileColumns.ElapsedTime, LogFileColumns.RawContent);
			entries.Contains(LogFileColumns.DeltaTime).Should().BeTrue();
			entries.Contains(LogFileColumns.ElapsedTime).Should().BeTrue();
			entries.Contains(LogFileColumns.RawContent).Should().BeTrue();
			entries.Contains(LogFileColumns.LogLevel).Should().BeFalse("because we've not specified this column during construction");
			entries.Contains(LogFileColumns.Timestamp).Should().BeFalse("because we've not specified this column during construction");
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
			}).Should().Throw<ArgumentOutOfRangeException>();
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

			var logEntry = new LogEntry
			{
				RawContent = "Foobar"
			};
			entries.Add(logEntry);

			logEntry = new LogEntry
			{
				Timestamp = new DateTime(2017, 12, 19, 16, 08, 0)
			};
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
		public void TestAddRange()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			var other = new LogEntryList(LogFileColumns.RawContent);
			other.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "rob"}}));
			other.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "ert"}}));

			entries.AddRange(other);
			entries.Count.Should().Be(2, "because we've added all two entries from the other list");
			entries[0].RawContent.Should().Be("rob");
			entries[1].RawContent.Should().Be("ert");
		}

		[Test]
		public void TestRemoveAtInvalidIndex([Values(-1, 1, 2)] int invalidIndex)
		{
			var entries = new LogEntryList(LogFileColumns.DeltaTime, LogFileColumns.RawContent);
			entries.Add(TimeSpan.FromSeconds(44), "stuff");
			entries.Count.Should().Be(1);
			entries[0].DeltaTime.Should().Be(TimeSpan.FromSeconds(44));
			entries[0].RawContent.Should().Be("stuff");

			new Action(() => entries.RemoveAt(invalidIndex)).Should().Throw<ArgumentOutOfRangeException>();
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
				.Should().Throw<ArgumentOutOfRangeException>();
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
			new Action(() => entries.InsertEmpty(invalidIndex)).Should().Throw<ArgumentOutOfRangeException>();
			entries.Count.Should().Be(0);
		}

		[Test]
		public void TestRemoveRange()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Add("foo");
			entries.Add("clondyke");
			entries.Add("bar");
			entries.Count.Should().Be(3);

			entries.RemoveRange(1, 1);
			entries.Count.Should().Be(2);
			entries[0].RawContent.Should().Be("foo", "because the first message shouldn't have been modified");
			entries[1].RawContent.Should().Be("bar", "because the second cell should have been removed, the third cell should have moved down");
		}

		[Test]
		public void TestRemoveRangeInvalidIndex1([Values(-2, -1)] int invalidIndex)
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Add("foo");
			entries.Add("bar");
			entries.Count.Should().Be(2);

			new Action(() => entries.RemoveRange(invalidIndex, 1)).Should().Throw<ArgumentOutOfRangeException>();
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

			new Action(() => entries.RemoveRange(invalidIndex, 1)).Should().Throw<ArgumentException>();
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

			new Action(() => entries.RemoveRange(index, 7)).Should().Throw<ArgumentException>();
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
				}).Should().Throw<InvalidOperationException>();
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

		[Test]
		public void TestResizeAddRows()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "Hello!"}}));
			entries.Resize(3);
			entries.Count.Should().Be(3);
			entries[0].RawContent.Should().Be("Hello!", "because the content of the first cell should have been left untouched");
			entries[1].RawContent.Should().BeNull("because an default value should have been placed in the newly added cell");
			entries[2].RawContent.Should().BeNull("because an default value should have been placed in the newly added cell");
		}

		[Test]
		public void TestResizeRemoveRows()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "Hello,"}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "World!"}}));
			entries.Resize(1);
			entries.Count.Should().Be(1, "because we just removed a cell");
			entries[0].RawContent.Should().Be("Hello,", "Because the content of the first cell should been left untouched");
		}

		[Test]
		public void TestResizeInvalidCount()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "Hello,"}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "World!"}}));
			new Action(() => entries.Resize(-1)).Should().Throw<ArgumentOutOfRangeException>();
			entries.Count.Should().Be(2, "because the list shouldn't have been modified");
			entries[0].RawContent.Should().Be("Hello,", "Because the content of the first cell should been left untouched");
			entries[1].RawContent.Should().Be("World!", "Because the content of the second cell should been left untouched");
		}

		[Test]
		public void TestCopyFromArray_Empty()
		{
			var entries = new LogEntryList(LogFileColumns.LogLevel);

			var buffer = new LevelFlags[]
			{
				LevelFlags.All,
				LevelFlags.Debug,
				LevelFlags.Error,
				LevelFlags.Warning
			};

			entries.Resize(4);
			entries.Count.Should().Be(buffer.Length);
			entries.CopyFrom(LogFileColumns.LogLevel, 0, buffer, 0, buffer.Length);
			entries[0].LogLevel.Should().Be(LevelFlags.All);
			entries[1].LogLevel.Should().Be(LevelFlags.Debug);
			entries[2].LogLevel.Should().Be(LevelFlags.Error);
			entries[3].LogLevel.Should().Be(LevelFlags.Warning);
		}

		[Test]
		public void TestCopyFromArray_PartiallyFilled()
		{
			var entries = new LogEntryList(LogFileColumns.LogLevel);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Info}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Trace}}));

			var buffer = new LevelFlags[]
			{
				LevelFlags.All,
				LevelFlags.Debug,
				LevelFlags.Error,
				LevelFlags.Warning
			};
			
			entries.Resize(4);
			entries.Count.Should().Be(buffer.Length);
			entries.CopyFrom(LogFileColumns.LogLevel, 0, buffer, 0, buffer.Length);
			entries[0].LogLevel.Should().Be(LevelFlags.All, "because the first entry's level should have been overwritten");
			entries[1].LogLevel.Should().Be(LevelFlags.Debug, "because the second entry's level should have been overwritten");
			entries[2].LogLevel.Should().Be(LevelFlags.Error, "because the third log entry should been added");
			entries[3].LogLevel.Should().Be(LevelFlags.Warning, "because the third log entry should been added");
		}

		[Test]
		public void TestCopyFromArray_CompletelyOutOfBounds()
		{
			var entries = new LogEntryList(LogFileColumns.LogLevel);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Info}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Trace}}));

			var buffer = new LevelFlags[]
			{
				LevelFlags.All,
				LevelFlags.Debug,
				LevelFlags.Error,
				LevelFlags.Warning
			};

			new Action(() => entries.CopyFrom(LogFileColumns.LogLevel, 2, buffer, 0, buffer.Length))
				.Should().Throw<ArgumentOutOfRangeException>();
			entries[0].LogLevel.Should().Be(LevelFlags.Info, "because the first entry's level should have been overwritten");
			entries[1].LogLevel.Should().Be(LevelFlags.Trace, "because the second entry's level should have been overwritten");
		}

		[Test]
		public void TestCopyFromArray_PartiallyOutOfBounds1()
		{
			var entries = new LogEntryList(LogFileColumns.LogLevel);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Info}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Trace}}));

			var buffer = new LevelFlags[]
			{
				LevelFlags.All,
				LevelFlags.Debug,
				LevelFlags.Error,
				LevelFlags.Warning
			};

			new Action(() => entries.CopyFrom(LogFileColumns.LogLevel, 1, buffer, 0, buffer.Length))
				.Should().Throw<ArgumentOutOfRangeException>();
			entries[0].LogLevel.Should().Be(LevelFlags.Info, "because the first entry's level should have been overwritten");
			entries[1].LogLevel.Should().Be(LevelFlags.Trace, "because the second entry's level should have been overwritten");
		}

		[Test]
		public void TestCopyFromArray_PartiallyOutOfBounds2()
		{
			var entries = new LogEntryList(LogFileColumns.LogLevel);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Info}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Trace}}));

			var buffer = new LevelFlags[]
			{
				LevelFlags.All,
				LevelFlags.Debug
			};

			new Action(() => entries.CopyFrom(LogFileColumns.LogLevel, -1, buffer, 0, buffer.Length))
				.Should().Throw<ArgumentOutOfRangeException>();
			entries[0].LogLevel.Should().Be(LevelFlags.Info, "because the first entry's level should have been overwritten");
			entries[1].LogLevel.Should().Be(LevelFlags.Trace, "because the second entry's level should have been overwritten");
		}

		[Test]
		public void TestCopyFromArray_NullColumn()
		{
			var entries = new LogEntryList(LogFileColumns.LogLevel);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Info}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Trace}}));

			var buffer = new string[]
			{
				"Foo",
				"Bar"
			};

			new Action(() => entries.CopyFrom(null, 0, buffer, 0, buffer.Length))
				.Should().Throw<ArgumentNullException>();
			entries[0].LogLevel.Should().Be(LevelFlags.Info, "because the first entry's level should have been overwritten");
			entries[1].LogLevel.Should().Be(LevelFlags.Trace, "because the second entry's level should have been overwritten");
		}

		[Test]
		public void TestCopyFromArray_NoSuchColumn()
		{
			var entries = new LogEntryList(LogFileColumns.LogLevel);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Info}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.LogLevel, LevelFlags.Trace}}));

			var buffer = new string[]
			{
				"Foo",
				"Bar"
			};

			new Action(() => entries.CopyFrom(LogFileColumns.RawContent, 0, buffer, 0, buffer.Length))
				.Should().Throw<NoSuchColumnException>();
			entries[0].LogLevel.Should().Be(LevelFlags.Info, "because the first entry's level should have been overwritten");
			entries[1].LogLevel.Should().Be(LevelFlags.Trace, "because the second entry's level should have been overwritten");
		}

		[Test]
		public void TestCopyFromLogFile_Contiguous()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "I"}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "want"}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "a"}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "Clondyke"}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "Bar"}}));

			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello", LevelFlags.Debug);
			logFile.AddEntry("World!", LevelFlags.Info);

			entries.CopyFrom(LogFileColumns.RawContent, 3, logFile, new LogFileSection(0, 2));
			entries.Count.Should().Be(5, "because the count shouldn't have been modified");
			entries[0].RawContent.Should().Be("I", "because the first entry's raw content should not have been overwritten");
			entries[1].RawContent.Should().Be("want", "because the second entry's raw content should not have been overwritten");
			entries[2].RawContent.Should().Be("a", "because the third entry's raw content should not have been overwritten");
			entries[3].RawContent.Should().Be("Hello", "because the fourth entry's raw content should have been overwritten");
			entries[4].RawContent.Should().Be("World!", "because the fifth entry's raw content should have been overwritten");
		}

		[Test]
		public void TestCopyFromLogFile_Non_Contiguous()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "I"}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "want"}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "a"}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "Clondyke"}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "Bar"}}));

			var logFile = new InMemoryLogFile();
			logFile.AddEntry("What", LevelFlags.Debug);
			logFile.AddEntry("are", LevelFlags.Debug);
			logFile.AddEntry("you", LevelFlags.Debug);
			logFile.AddEntry("doing", LevelFlags.Debug);
			logFile.AddEntry("Turn?", LevelFlags.Info);

			entries.CopyFrom(LogFileColumns.RawContent, 3, logFile, new[]{new LogLineIndex(2), new LogLineIndex(4) });
			entries.Count.Should().Be(5, "because the count shouldn't have been modified");
			entries[0].RawContent.Should().Be("I", "because the first entry's raw content should not have been overwritten");
			entries[1].RawContent.Should().Be("want", "because the second entry's raw content should not have been overwritten");
			entries[2].RawContent.Should().Be("a", "because the third entry's raw content should not have been overwritten");
			entries[3].RawContent.Should().Be("you", "because the fourth entry's raw content should have been overwritten");
			entries[4].RawContent.Should().Be("Turn?", "because the fifth entry's raw content should have been overwritten");
		}

		[Test]
		public void TestFillDefault()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent, LogFileColumns.LogLevel);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "I"}, {LogFileColumns.LogLevel, LevelFlags.Debug}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "want"}, {LogFileColumns.LogLevel, LevelFlags.Info}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "a"}, {LogFileColumns.LogLevel, LevelFlags.Warning}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "Clondyke"}, {LogFileColumns.LogLevel, LevelFlags.Error}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "Bar"}, {LogFileColumns.LogLevel, LevelFlags.Fatal}}));

			entries.FillDefault(2, 2);
			entries.Count.Should().Be(5, "because the count shouldn't have been modified");

			entries[0].RawContent.Should().Be("I", "because the first entry's raw content should not have been overwritten");
			entries[0].LogLevel.Should().Be(LevelFlags.Debug, "because the first entry's raw content should not have been overwritten");

			entries[1].RawContent.Should().Be("want", "because the second entry's raw content should not have been overwritten");
			entries[1].LogLevel.Should().Be(LevelFlags.Info, "because the second entry's raw content should not have been overwritten");

			entries[2].RawContent.Should().Be(null, "because the third entry's raw content should have been overwritten");
			entries[2].LogLevel.Should().Be(LevelFlags.None, "because the third entry's log level should have been overwritten");

			entries[3].RawContent.Should().Be(null, "because the fourth entry's raw content should have been overwritten");
			entries[3].LogLevel.Should().Be(LevelFlags.None, "because the fourth entry's log level should have been overwritten");

			entries[4].RawContent.Should().Be("Bar", "because the fifth entry's raw content should not have been overwritten");
			entries[4].LogLevel.Should().Be(LevelFlags.Fatal, "because the fifth entry's log level should not have been overwritten");
		}

		[Test]
		public void TestFillDefault_SingleColumn()
		{
			var entries = new LogEntryList(LogFileColumns.RawContent, LogFileColumns.LogLevel);
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "I"}, {LogFileColumns.LogLevel, LevelFlags.Debug}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "want"}, {LogFileColumns.LogLevel, LevelFlags.Info}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "a"}, {LogFileColumns.LogLevel, LevelFlags.Warning}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "Clondyke"}, {LogFileColumns.LogLevel, LevelFlags.Error}}));
			entries.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{LogFileColumns.RawContent, "Bar"}, {LogFileColumns.LogLevel, LevelFlags.Fatal}}));

			entries.FillDefault(LogFileColumns.RawContent,  1, 3);
			entries.Count.Should().Be(5, "because the count shouldn't have been modified");

			entries[0].RawContent.Should().Be("I", "because the first entry's raw content should NOT have been overwritten");
			entries[0].LogLevel.Should().Be(LevelFlags.Debug, "because the first entry's raw content should NOT have been overwritten");

			entries[1].RawContent.Should().Be(null, "because the second entry's raw content should have been overwritten");
			entries[1].LogLevel.Should().Be(LevelFlags.Info, "because the second entry's raw content should NOT have been overwritten");

			entries[2].RawContent.Should().Be(null, "because the third entry's raw content should have been overwritten");
			entries[2].LogLevel.Should().Be(LevelFlags.Warning, "because the third entry's log level should NOT have been overwritten");

			entries[3].RawContent.Should().Be(null, "because the fourth entry's raw content should have been overwritten");
			entries[3].LogLevel.Should().Be(LevelFlags.Error, "because the fourth entry's log level should NOT have been overwritten");

			entries[4].RawContent.Should().Be("Bar", "because the fifth entry's raw content should NOT have been overwritten");
			entries[4].LogLevel.Should().Be(LevelFlags.Fatal, "because the fifth entry's log level should NOT have been overwritten");
		}

		protected override IReadOnlyLogEntries CreateEmpty(IEnumerable<IColumnDescriptor> columns)
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

			return CreateEmpty(new IColumnDescriptor[0]);
		}
	}
}
