using System;
using System.Collections.Generic;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class InMemoryLogFileTest
		: AbstractLogFileTest
	{
		private Mock<ILogFileListener> _listener;
		private List<LogFileSection> _modifications;

		[SetUp]
		public void Setup()
		{
			_modifications = new List<LogFileSection>();
			_listener = new Mock<ILogFileListener>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
				.Callback((ILogFile logFile, LogFileSection section) =>
				{
					_modifications.Add(section);
				});
		}

		[Test]
		public void TestConstruction1()
		{
			var logFile = new InMemoryLogFile();
			logFile.Columns.Should().Equal(LogFileColumns.Minimum);
			logFile.GetProperty(LogFileProperties.Size).Should().Be(Size.Zero);
			logFile.GetProperty(TextLogFileProperties.MaxCharactersInLine).Should().Be(0);
			logFile.GetProperty(LogFileProperties.LastModified).Should().BeNull();
			logFile.GetProperty(LogFileProperties.StartTimestamp).Should().BeNull();
			logFile.GetProperty(LogFileProperties.EndTimestamp).Should().BeNull();
			logFile.GetProperty(LogFileProperties.Duration).Should().BeNull();
			logFile.GetProperty(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.None);
			logFile.GetProperty(LogFileProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
			logFile.Count.Should().Be(0);
		}

		[Test]
		public void TestConstruction2()
		{
			var logFile = new InMemoryLogFile(LogFileColumns.ElapsedTime);
			logFile.Columns.Should().Equal(LogFileColumns.Minimum, "because a log file should never offer less columns than the minimum set");
		}

		[Test]
		public void TestAddEntry1()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello, World!", LevelFlags.Info);
			logFile.Count.Should().Be(1);
			logFile.GetProperty(TextLogFileProperties.MaxCharactersInLine).Should().Be(13);
			logFile.GetProperty(LogFileProperties.StartTimestamp).Should().BeNull();

			var entry = logFile.GetEntry(0);
			entry.ElapsedTime.Should().BeNull("because this entry doesn't have any timestamp and thus its elapsed time must be null");
		}

		[Test]
		public void TestAddEntry2()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello,", LevelFlags.Info, new DateTime(2017, 4, 29, 14, 56, 0));
			logFile.AddEntry(" World!", LevelFlags.Warning, new DateTime(2017, 4, 29, 14, 56, 2));
			logFile.Count.Should().Be(2);

			var entry1 = logFile.GetEntry(0);
			entry1.Index.Should().Be(0);
			entry1.LogEntryIndex.Should().Be(0);
			entry1.LogLevel.Should().Be(LevelFlags.Info);
			entry1.RawContent.Should().Be("Hello,");
			entry1.Timestamp.Should().Be(new DateTime(2017, 4, 29, 14, 56, 0));

			var entry2 = logFile.GetEntry(1);
			entry2.Index.Should().Be(1);
			entry2.LogEntryIndex.Should().Be(1);
			entry2.LogLevel.Should().Be(LevelFlags.Warning);
			entry2.RawContent.Should().Be(" World!");
			entry2.Timestamp.Should().Be(new DateTime(2017, 4, 29, 14, 56, 2));

			logFile.GetProperty(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2017, 4, 29, 14, 56, 0));
			logFile.GetProperty(LogFileProperties.Duration).Should().Be(TimeSpan.FromSeconds(2));
		}

		[Test]
		[Description("Verifies that the MaxCharactersPerLine doesn't decrease")]
		public void TestAddEntry3()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello, World!", LevelFlags.Info);
			logFile.AddEntry("Hi", LevelFlags.Info);
			logFile.GetProperty(TextLogFileProperties.MaxCharactersInLine).Should().Be(13);
		}

		[Test]
		[Description("Verifies that the MaxCharactersPerLine only increases")]
		public void TestAddEntry4()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hi", LevelFlags.Info);
			logFile.GetProperty(TextLogFileProperties.MaxCharactersInLine).Should().Be(2);
			logFile.AddEntry("Hello, World!", LevelFlags.Info);
			logFile.GetProperty(TextLogFileProperties.MaxCharactersInLine).Should().Be(13);
		}

		[Test]
		[Description("Verifies that an added listener is notified of changes")]
		public void TestAddEntry5()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddListener(_listener.Object, TimeSpan.Zero, 1);
			logFile.AddEntry("Hi", LevelFlags.Info);
			_modifications.Should()
				.Equal(new object[]
				{
					LogFileSection.Reset,
					new LogFileSection(0, 1)
				});
		}

		[Test]
		public void TestAddEntry6()
		{
			var logFile = new InMemoryLogFile(LogFileColumns.LogLevel);

			var logEntry = new LogEntry
			{
				LogLevel = LevelFlags.Error
			};
			logFile.Add(logEntry);

			var buffer = new LogEntryArray(1, LogFileColumns.LogLevel);
			logFile.GetEntries(new LogFileSection(0, 1), buffer);
			buffer[0].LogLevel.Should().Be(LevelFlags.Error);
		}

		[Test]
		public void TestAddMultilineEntry1()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddMultilineEntry(LevelFlags.Debug, null, "foo", "bar");
			logFile.Count.Should().Be(2);
			var entry1 = logFile.GetEntry(0);
			entry1.Index.Should().Be(0);
			entry1.LogEntryIndex.Should().Be(0);
			entry1.RawContent.Should().Be("foo");
			entry1.LogLevel.Should().Be(LevelFlags.Debug);
			entry1.Timestamp.Should().Be(null);

			var entry2 = logFile.GetEntry(1);
			entry2.Index.Should().Be(1);
			entry2.LogEntryIndex.Should().Be(0);
			entry2.RawContent.Should().Be("bar");
			entry2.LogLevel.Should().Be(LevelFlags.Debug);
			entry2.Timestamp.Should().Be(null);
		}

		[Test]
		public void TestAddMultilineEntry2()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello, World!", LevelFlags.Debug);
			var t1 = new DateTime(2017, 11, 26, 11, 56, 0);
			logFile.AddMultilineEntry(LevelFlags.Info, t1, "foo", "bar");
			logFile.Count.Should().Be(3);
			var entry1 = logFile.GetEntry(0);
			entry1.Index.Should().Be(0);
			entry1.LogEntryIndex.Should().Be(0);
			entry1.RawContent.Should().Be("Hello, World!");
			entry1.LogLevel.Should().Be(LevelFlags.Debug);
			entry1.Timestamp.Should().Be(null);

			var entry2 = logFile.GetEntry(1);
			entry2.Index.Should().Be(1);
			entry2.LogEntryIndex.Should().Be(1);
			entry2.RawContent.Should().Be("foo");
			entry2.LogLevel.Should().Be(LevelFlags.Info);
			entry2.Timestamp.Should().Be(t1);

			var entry3 = logFile.GetEntry(2);
			entry3.Index.Should().Be(2);
			entry3.LogEntryIndex.Should().Be(1);
			entry3.RawContent.Should().Be("bar");
			entry3.LogLevel.Should().Be(LevelFlags.Info);
			entry3.Timestamp.Should().Be(t1);
		}

		[Test]
		public void TestAddMultilineEntry3()
		{
			var logFile = new InMemoryLogFile();
			var t1 = new DateTime(2017, 11, 26, 11, 56, 0);
			logFile.AddMultilineEntry(LevelFlags.Info, t1, "foo", "bar");
			var t2 = new DateTime(2017, 11, 26, 11, 57, 0);
			logFile.AddMultilineEntry(LevelFlags.Warning, t2, "H", "e", "l", "l", "o");
			logFile.Count.Should().Be(7);
			
			var entry1 = logFile.GetEntry(0);
			entry1.Index.Should().Be(0);
			entry1.LogEntryIndex.Should().Be(0);
			entry1.RawContent.Should().Be("foo");
			entry1.LogLevel.Should().Be(LevelFlags.Info);
			entry1.Timestamp.Should().Be(t1);

			var entry2 = logFile.GetEntry(1);
			entry2.Index.Should().Be(1);
			entry2.LogEntryIndex.Should().Be(0);
			entry2.RawContent.Should().Be("bar");
			entry2.LogLevel.Should().Be(LevelFlags.Info);
			entry2.Timestamp.Should().Be(t1);

			var entry3 = logFile.GetEntry(2);
			entry3.Index.Should().Be(2);
			entry3.LogEntryIndex.Should().Be(1);
			entry3.RawContent.Should().Be("H");
			entry3.LogLevel.Should().Be(LevelFlags.Warning);
			entry3.Timestamp.Should().Be(t2);
			
			var entry4 = logFile.GetEntry(3);
			entry4.Index.Should().Be(3);
			entry4.LogEntryIndex.Should().Be(1);
			entry4.RawContent.Should().Be("e");
			entry4.LogLevel.Should().Be(LevelFlags.Warning);
			entry4.Timestamp.Should().Be(t2);

			var entry5 = logFile.GetEntry(4);
			entry5.Index.Should().Be(4);
			entry5.LogEntryIndex.Should().Be(1);
			entry5.RawContent.Should().Be("l");
			entry5.LogLevel.Should().Be(LevelFlags.Warning);
			entry5.Timestamp.Should().Be(t2);

			var entry6 = logFile.GetEntry(5);
			entry6.Index.Should().Be(5);
			entry6.LogEntryIndex.Should().Be(1);
			entry6.RawContent.Should().Be("l");
			entry6.LogLevel.Should().Be(LevelFlags.Warning);
			entry6.Timestamp.Should().Be(t2);
			
			var entry7 = logFile.GetEntry(6);
			entry7.Index.Should().Be(6);
			entry7.LogEntryIndex.Should().Be(1);
			entry7.RawContent.Should().Be("o");
			entry7.LogLevel.Should().Be(LevelFlags.Warning);
			entry7.Timestamp.Should().Be(t2);
		}

		[Test]
		public void TestAddMultilineEntry4()
		{
			var logFile = new InMemoryLogFile();

			logFile.AddListener(_listener.Object, TimeSpan.Zero, 2);

			var t1 = new DateTime(2017, 11, 26, 11, 56, 0);
			logFile.AddMultilineEntry(LevelFlags.Info, t1, "foo", "bar");

			_modifications.Should().Equal(new object[]
			{
				LogFileSection.Reset,
				new LogFileSection(0, 2)
			});
		}

		[Test]
		[Description("Verifies that a listener is notified of changes immediately while being added")]
		public void TestAddListener1()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddListener(_listener.Object, TimeSpan.Zero, 1);
			_modifications.Should().Equal(new object[] {LogFileSection.Reset});
		}

		[Test]
		[Description("Verifies that a listener is notified of changes immediately while being added")]
		public void TestAddListener2()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Foo", LevelFlags.Other);

			logFile.AddListener(_listener.Object, TimeSpan.Zero, 1);
			_modifications.Should().Equal(new object[]
			{
				LogFileSection.Reset,
				new LogFileSection(0, 1)
			});
		}

		[Test]
		[Description("Verifies that clear works on an empty file and doesn't do anything")]
		public void TestClear1()
		{
			var logFile = new InMemoryLogFile();
			logFile.Clear();
			logFile.GetProperty(LogFileProperties.Size).Should().Be(Size.Zero);
			logFile.GetProperty(TextLogFileProperties.MaxCharactersInLine).Should().Be(0);
			logFile.GetProperty(LogFileProperties.LastModified).Should().BeNull();
			logFile.GetProperty(LogFileProperties.StartTimestamp).Should().BeNull();
			logFile.GetProperty(LogFileProperties.EndTimestamp).Should().BeNull();
			logFile.GetProperty(LogFileProperties.Duration).Should().BeNull();
			logFile.GetProperty(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.None);
			logFile.Count.Should().Be(0);
		}

		[Test]
		[Description("Verifies that Clear actually removes lines")]
		public void TestClear2()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hello,", LevelFlags.Info);
			logFile.AddEntry(" World!", LevelFlags.Warning);
			logFile.Clear();
			logFile.Count.Should().Be(0);
		}

		[Test]
		[Description("Verifies that the MaxCharactersPerLine is reset when Clear()ed")]
		public void TestClear3()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hi", LevelFlags.Info);
			logFile.GetProperty(TextLogFileProperties.MaxCharactersInLine).Should().Be(2);
			logFile.Clear();
			logFile.GetProperty(TextLogFileProperties.MaxCharactersInLine).Should().Be(0);
		}

		[Test]
		[Description("Verifies that a reset event is fired when the log file is cleared")]
		public void TestClear4()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Hi", LevelFlags.Info);
			logFile.GetProperty(TextLogFileProperties.MaxCharactersInLine).Should().Be(2);

			logFile.AddListener(_listener.Object, TimeSpan.Zero, 1);
			logFile.Clear();
			_modifications.Should().EndWith(LogFileSection.Reset);
		}

		[Test]
		public void TestGetLogLineIndexOfOriginalLineIndex1()
		{
			var logFile = new InMemoryLogFile();
			const string reason = "because the log file is empty";
			logFile.GetLogLineIndexOfOriginalLineIndex(LogLineIndex.Invalid).Should().Be(LogLineIndex.Invalid, reason);
			logFile.GetLogLineIndexOfOriginalLineIndex(0).Should().Be(LogLineIndex.Invalid, reason);
			logFile.GetLogLineIndexOfOriginalLineIndex(1).Should().Be(LogLineIndex.Invalid, reason);
		}

		[Test]
		public void TestGetLogLineIndexOfOriginalLineIndex2()
		{
			var logFile = new InMemoryLogFile();
			logFile.GetLogLineIndexOfOriginalLineIndex(0).Should().Be(LogLineIndex.Invalid);
			logFile.AddEntry("", LevelFlags.All);
			logFile.GetLogLineIndexOfOriginalLineIndex(0).Should().Be(new LogLineIndex(0));
			logFile.GetLogLineIndexOfOriginalLineIndex(1).Should().Be(LogLineIndex.Invalid);
			logFile.AddEntry("", LevelFlags.All);
			logFile.GetLogLineIndexOfOriginalLineIndex(0).Should().Be(new LogLineIndex(0));
			logFile.GetLogLineIndexOfOriginalLineIndex(1).Should().Be(new LogLineIndex(1));
			logFile.Clear();
			logFile.GetLogLineIndexOfOriginalLineIndex(0).Should().Be(LogLineIndex.Invalid);
			logFile.GetLogLineIndexOfOriginalLineIndex(1).Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		public void TestGetEntriesEmpty()
		{
			var logFile = new InMemoryLogFile();
			logFile.GetEntries(new LogFileSection()).Should().BeEmpty("because the log file is empty");
		}

		[Test]
		public void TestGetEntriesRawContent1()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("foobar");
			var entries = logFile.GetEntries(new LogFileSection(0, 1), new[]{LogFileColumns.RawContent});
			entries.Count.Should().Be(1);
			entries.Columns.Should().Equal(new object[] {LogFileColumns.RawContent}, "because we've only retrieved that column");
			entries[0].RawContent.Should().Be("foobar");
		}

		[Test]
		public void TestGetEntriesRawContent2()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("foo");
			logFile.AddEntry("bar");
			logFile.AddEntry("some lazy fox");
			var entries = logFile.GetEntries(new LogFileSection(1, 2), new[]{LogFileColumns.RawContent});
			entries.Count.Should().Be(2);
			entries.Columns.Should().Equal(new object[] { LogFileColumns.RawContent }, "because we've only retrieved that column");
			entries[0].RawContent.Should().Be("bar");
			entries[1].RawContent.Should().Be("some lazy fox");
		}

		[Test]
		public void TestGetEntriesMultipleColumns()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("", LevelFlags.Debug, new DateTime(2017, 12, 12, 00, 11, 0));
			logFile.AddEntry("", LevelFlags.Info);
			logFile.AddEntry("", LevelFlags.Error, new DateTime(2017, 12, 12, 00, 12, 0));

			var entries = logFile.GetEntries(new LogFileSection(1, 2), new ILogFileColumnDescriptor[]{LogFileColumns.LogLevel, LogFileColumns.Timestamp});
			entries.Count.Should().Be(2);
			entries.Columns.Should().Equal(new object[] {LogFileColumns.LogLevel, LogFileColumns.Timestamp});
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[0].Timestamp.Should().Be(null);
			entries[1].LogLevel.Should().Be(LevelFlags.Error);
			entries[1].Timestamp.Should().Be(new DateTime(2017, 12, 12, 00, 12, 0));
		}

		[Test]
		public void TestGetEntriesRandomAccess()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("", LevelFlags.Debug, new DateTime(2017, 12, 12, 00, 11, 0));
			logFile.AddEntry("", LevelFlags.Info);
			logFile.AddEntry("", LevelFlags.Error, new DateTime(2017, 12, 12, 00, 12, 0));

			var entries = logFile.GetEntries(new LogFileSection(1, 2), new ILogFileColumnDescriptor[]{LogFileColumns.LogLevel, LogFileColumns.Timestamp});
			entries.Count.Should().Be(2);
			entries.Columns.Should().Equal(new object[] { LogFileColumns.LogLevel, LogFileColumns.Timestamp });
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[0].Timestamp.Should().Be(null);
			entries[1].LogLevel.Should().Be(LevelFlags.Error);
			entries[1].Timestamp.Should().Be(new DateTime(2017, 12, 12, 00, 12, 0));
		}

		[Test]
		public void TestGetEntriesWithMinimumColumns()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("", LevelFlags.Debug, new DateTime(2017, 12, 12, 00, 11, 0));
			logFile.AddEntry("", LevelFlags.Info);
			logFile.AddEntry("", LevelFlags.Error, new DateTime(2017, 12, 12, 00, 12, 0));

			var entries = logFile.GetEntries(new LogFileSection(1, 2), LogFileColumns.Minimum);
			entries.Count.Should().Be(2);
			entries.Columns.Should().Equal(LogFileColumns.Minimum);
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[0].Timestamp.Should().Be(null);
			entries[1].LogLevel.Should().Be(LevelFlags.Error);
			entries[1].Timestamp.Should().Be(new DateTime(2017, 12, 12, 00, 12, 0));
		}
		
		[Test]
		public void TestGetEntriesWithElapsedTimeColumns()
		{
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("", LevelFlags.Info);
			logFile.AddEntry("", LevelFlags.Debug, new DateTime(2017, 12, 12, 00, 11, 0));
			logFile.AddEntry("", LevelFlags.Info);
			logFile.AddEntry("", LevelFlags.Error, new DateTime(2017, 12, 12, 00, 12, 0));
			logFile.AddEntry("", LevelFlags.Error, new DateTime(2017, 12, 20, 17, 01, 0));

			var entries = logFile.GetEntries(new LogFileSection(0, 5), new []{LogFileColumns.ElapsedTime});
			entries.Count.Should().Be(5);
			entries.Columns.Should().Equal(LogFileColumns.ElapsedTime);
			entries[0].ElapsedTime.Should().Be(null);
			entries[1].ElapsedTime.Should().Be(TimeSpan.Zero);
			entries[2].ElapsedTime.Should().Be(null);
			entries[3].ElapsedTime.Should().Be(TimeSpan.FromMinutes(1));
			entries[4].ElapsedTime.Should().Be(TimeSpan.FromDays(8)+TimeSpan.FromHours(16)+TimeSpan.FromMinutes(50));
		}

		protected override ILogFile CreateEmpty()
		{
			return new InMemoryLogFile();
		}

		protected override ILogFile CreateFromContent(IReadOnlyLogEntries content)
		{
			var logFile = new InMemoryLogFile(content);
			return logFile;
		}
	}
}