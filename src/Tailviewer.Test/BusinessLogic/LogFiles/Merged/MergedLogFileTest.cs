﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Merged
{
	[TestFixture]
	public sealed class MergedLogFileTest
		: AbstractLogFileTest
	{
		private ManualTaskScheduler _taskScheduler;

		private static List<LogFileSection> ListenToChanges(ILogFile logFile, int maximumLineCount)
		{
			var changes = new List<LogFileSection>();
			var listener = new Mock<ILogFileListener>();
			listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
			        .Callback((ILogFile file, LogFileSection section) => { changes.Add(section); });
			logFile.AddListener(listener.Object, TimeSpan.Zero, maximumLineCount);
			return changes;
		}

		private static ILogEntries Listen(ILogFile logFile)
		{
			var data = new LogEntryList(logFile.Columns);
			var listener = new Mock<ILogFileListener>();
			listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
			        .Callback((ILogFile file, LogFileSection section) =>
				        {
					        if (section.IsReset)
					        {
						        data.Clear();
					        }
					        else if (section.IsInvalidate)
					        {
						        data.RemoveRange((int) section.Index, section.Count);
					        }
					        else
					        {
						        var destinationIndex = data.Count;
								data.Resize(data.Count + section.Count);
						        file.GetEntries(section, data, destinationIndex);
					        }
				        });
			logFile.AddListener(listener.Object, TimeSpan.Zero, 1);
			return data;
		}

		[Pure]
		private static ILogFileColumnDescriptor CreateCustomColumn<T>(T defaultValue)
		{
			var column = new Mock<ILogFileColumnDescriptor<T>>();
			column.SetupGet(x => x.DefaultValue).Returns(defaultValue);
			column.SetupGet(x => x.DataType).Returns(typeof(T));
			return column.Object;
		}

		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new ManualTaskScheduler();
		}

		[Test]
		[Description("Verifies that creating a merged log file from two sources is possible")]
		public void TestConstruction1()
		{
			var source1 = new Mock<ILogFile>();
			source1.SetupGet(x => x.Columns).Returns(new ILogFileColumnDescriptor[0]);
			var source2 = new Mock<ILogFile>();
			source2.SetupGet(x => x.Columns).Returns(new ILogFileColumnDescriptor[0]);

			MergedLogFile logFile = null;
			new Action(() => logFile = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), source1.Object, source2.Object))
				.Should().NotThrow();
			logFile.Should().NotBeNull();
		}

		[Test]
		[Description("Verifies that a merged log file can be created using the maximum number of supported sources")]
		public void TestConstruction2()
		{
			var sources = Enumerable.Range(0, LogLineSourceId.MaxSources)
				.Select(unused =>
				{
					var logFileSource = new Mock<ILogFile>();
					logFileSource.Setup(x => x.Columns).Returns(new ILogFileColumnDescriptor[0]);
					return logFileSource.Object;
				}).ToArray();
			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), sources);
			logFile.Sources.Should().Equal(sources);
		}

		[Test]
		[Description("Verifies that the ctor complains if too many sources are merged")]
		public void TestConstruction3()
		{
			var sources = Enumerable.Range(0, LogLineSourceId.MaxSources+1)
				.Select(unused => new Mock<ILogFile>().Object).ToArray();

			new Action(() => new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), sources))
				.Should().Throw<ArgumentException>("because the a merged log file can only support so many sources");
		}

		[Test]
		[Description("Verifies that a merged log file is able to provide access to custom columns")]
		public void TestCustomColumn()
		{
			var myCustomColumn = CreateCustomColumn<string>(null);
			var source1 = new InMemoryLogFile();
			source1.Add(new LogEntry
			{
				RawContent = "What is up Munich?",
				Timestamp = new DateTime(2021, 02, 11, 22, 16, 49)
			});
			var source2 = new InMemoryLogFile(myCustomColumn);
			var entry2 = new LogEntry
			{
				RawContent = "Everything",
				Timestamp = new DateTime(2021, 02, 11, 22, 15, 11)
			};
			entry2.SetValue(myCustomColumn, "A very important piece of information");
			source2.Add(entry2);

			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, new[] {source1, source2});
			_taskScheduler.RunOnce();

			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);
			var entries = merged.GetEntries(new LogFileSection(0, 2), new ILogFileColumnDescriptor[]{LogFileColumns.RawContent, LogFileColumns.Timestamp, LogFileColumns.SourceId, myCustomColumn});
			entries.Count.Should().Be(2);
			entries[0].RawContent.Should().Be("Everything");
			entries[0].Timestamp.Should().Be(new DateTime(2021, 02, 11, 22, 15, 11));
			entries[0].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(1), "because this log entry is from the second source of the log file");
			entries[0].GetValue(myCustomColumn).Should().Be("A very important piece of information");
			entries[1].RawContent.Should().Be("What is up Munich?");
			entries[1].Timestamp.Should().Be(new DateTime(2021, 02, 11, 22, 16, 49));
			entries[1].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(0), "because this log entry is from the first source of the log file");
			entries[1].GetValue(myCustomColumn).Should().Be(myCustomColumn.DefaultValue, "because the first source doesn't have this column");
		}

		[Test]
		[Description("Verifies that disposing a logfile works")]
		public void TestDispose1()
		{
			var source1 = new Mock<ILogFile>();
			source1.SetupGet(x => x.Columns).Returns(new ILogFileColumnDescriptor[0]);
			var source2 = new Mock<ILogFile>();
			source2.SetupGet(x => x.Columns).Returns(new ILogFileColumnDescriptor[0]);

			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), source1.Object, source2.Object);
			new Action(logFile.Dispose).Should().NotThrow();
		}

		[Test]
		[Description("Verifies that disposing a removes it as listener from its sources")]
		public void TestDispose2()
		{
			var source1 = new Mock<ILogFile>();
			source1.SetupGet(x => x.Columns).Returns(new ILogFileColumnDescriptor[0]);
			var source2 = new Mock<ILogFile>();
			source2.SetupGet(x => x.Columns).Returns(new ILogFileColumnDescriptor[0]);

			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), source1.Object, source2.Object);
			source1.Verify(x => x.AddListener(logFile, It.IsAny<TimeSpan>(), It.IsAny<int>()), Times.Once);
			source2.Verify(x => x.AddListener(logFile, It.IsAny<TimeSpan>(), It.IsAny<int>()), Times.Once);

			logFile.Dispose();
			source1.Verify(x => x.RemoveListener(logFile), Times.Once, "because a merged log file is supposed to remove itself as a listener from its sources when its being disposed of");
			source2.Verify(x => x.RemoveListener(logFile), Times.Once, "because a merged log file is supposed to remove itself as a listener from its sources when its being disposed of");
		}

		[Test]
		public void TestMerge1()
		{
			var source1 = new InMemoryLogFile();
			var source2 = new InMemoryLogFile();
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);
			var entries = Listen(merged);

			source1.AddEntry("foobar", LevelFlags.Info, new DateTime(2019, 5, 28, 20, 31, 1));

			_taskScheduler.RunOnce();
			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(1);
			entries.Count.Should().Be(1);
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("foobar");
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[0].Timestamp.Should().Be(new DateTime(2019, 5, 28, 20, 31, 1));
		}

		[Test]
		public void TestMerge2()
		{
			var source1 = new InMemoryLogFile();
			var source2 = new InMemoryLogFile();
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);
			var entries = Listen(merged);

			source1.AddEntry("a", LevelFlags.Info, new DateTime(2019, 5, 28, 21, 59, 0));
			source1.AddEntry("b", LevelFlags.Debug, new DateTime(2019, 5, 28, 22, 0, 0));

			_taskScheduler.RunOnce();
			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);
			entries.Count.Should().Be(2);
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("a");
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[0].Timestamp.Should().Be(new DateTime(2019, 5, 28, 21, 59, 0));
			entries[0].ElapsedTime.Should().Be(TimeSpan.Zero);
			entries[0].DeltaTime.Should().BeNull();
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].RawContent.Should().Be("b");
			entries[1].LogLevel.Should().Be(LevelFlags.Debug);
			entries[1].Timestamp.Should().Be(new DateTime(2019, 5, 28, 22, 0, 0));
			entries[1].ElapsedTime.Should().Be(TimeSpan.FromMinutes(1));
			entries[1].DeltaTime.Should().Be(TimeSpan.FromMinutes(1));
		}

		[Test]
		[Description("Verifies that the order of OnLogFileModified invocations is preserved when invoked from 2 data sources")
		]
		public void TestMerge3()
		{
			var source0 = new InMemoryLogFile();
			var source1 = new InMemoryLogFile();
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source0, source1);
			var entries = Listen(merged);

			DateTime timestamp = DateTime.Now;
			source0.AddEntry("a", LevelFlags.Info, timestamp);

			_taskScheduler.RunOnce();
			merged.GetValue(LogFileProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

			source1.AddEntry("b", LevelFlags.Debug, timestamp);

			_taskScheduler.RunOnce();
			merged.GetValue(LogFileProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);
			entries.Count.Should().Be(2);
			entries[0].Index.Should().Be(0);
			entries[0].OriginalIndex.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(0));
			entries[0].RawContent.Should().Be("a");
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[0].Timestamp.Should().Be(timestamp);
			entries[1].Index.Should().Be(1);
			entries[1].OriginalIndex.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(1));
			entries[1].RawContent.Should().Be("b");
			entries[1].LogLevel.Should().Be(LevelFlags.Debug);
			entries[1].Timestamp.Should().Be(timestamp);
		}

		[Test]
		[Description("Verifies that log lines without timestamp are ignored")]
		public void TestMerge4()
		{
			var source1 = new InMemoryLogFile();
			var source2 = new InMemoryLogFile();
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);
			var entries = Listen(merged);

			source1.AddEntry("a", LevelFlags.Warning, new DateTime(2019, 5, 28, 22, 40, 0));
			source1.AddEntry("b", LevelFlags.Info);
			source1.AddEntry("c", LevelFlags.Error, new DateTime(2019, 5, 28, 22, 41, 0));

			_taskScheduler.RunOnce();
			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);
			entries.Count.Should().Be(2);
			entries[0].Index.Should().Be(0);
			entries[0].OriginalIndex.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].RawContent.Should().Be("a");
			entries[0].LogLevel.Should().Be(LevelFlags.Warning);
			entries[0].Timestamp.Should().Be(new DateTime(2019, 5, 28, 22, 40, 0));
			entries[1].Index.Should().Be(1);
			entries[1].OriginalIndex.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].RawContent.Should().Be("c");
			entries[1].LogLevel.Should().Be(LevelFlags.Error);
			entries[1].Timestamp.Should().Be(new DateTime(2019, 5, 28, 22, 41, 0));
		}

		[Test]
		[Description("Verifies that log messages from different sources are ordered correctly, even when arring out of order")]
		public void TestMerge5()
		{
			var source0 = new InMemoryLogFile();
			var source1 = new InMemoryLogFile();

			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source0, source1);
			var entries = Listen(merged);

			var later = new DateTime(2016, 2, 16);
			var earlier = new DateTime(2016, 2, 15);

			source0.AddEntry("a", LevelFlags.Warning, later);

			_taskScheduler.RunOnce();
			merged.GetValue(LogFileProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

			source1.AddEntry("c", LevelFlags.Error, earlier);

			_taskScheduler.RunOnce();
			merged.GetValue(LogFileProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);
			entries.Count.Should().Be(2);
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(1));
			entries[0].RawContent.Should().Be("c");
			entries[0].LogLevel.Should().Be(LevelFlags.Error);
			entries[0].Timestamp.Should().Be(earlier);
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(0));
			entries[1].RawContent.Should().Be("a");
			entries[1].LogLevel.Should().Be(LevelFlags.Warning);
			entries[1].Timestamp.Should().Be(later);
		}

		[Test]
		[Description(
			"Verifies that Reset() events from an always empty data source do not result in reset events from the merged log file"
			)]
		public void TestMerge6()
		{
			var source0 = new InMemoryLogFile();
			var source1 = new InMemoryLogFile();

			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source0, source1);
			var entries = Listen(merged);
			var changes = ListenToChanges(merged, 1);

			merged.OnLogFileModified(source0, LogFileSection.Reset);
			merged.OnLogFileModified(source0, LogFileSection.Reset);
			merged.OnLogFileModified(source0, LogFileSection.Reset);
			merged.OnLogFileModified(source0, LogFileSection.Reset);
			merged.OnLogFileModified(source0, LogFileSection.Reset);
			merged.OnLogFileModified(source0, LogFileSection.Reset);
			merged.OnLogFileModified(source1, LogFileSection.Reset);

			DateTime timestamp = DateTime.Now;
			source1.AddEntry("Hello World", LevelFlags.Info, timestamp);

			_taskScheduler.RunOnce();
			merged.GetValue(LogFileProperties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

			entries.Count.Should().Be(1);
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(1));
			entries[0].RawContent.Should().Be("Hello World");
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[0].Timestamp.Should().Be(timestamp);

			int count = changes.Count;
			changes.ElementAt(count - 2).Should().Equal(LogFileSection.Reset);
			changes.ElementAt(count - 1).Should().Equal(new LogFileSection(0, 1));
		}

		[Test]
		[Description("Verifies that merging a multi line entry in order works")]
		public void TestMergeMultiline1()
		{
			var source1 = new InMemoryLogFile();
			var source1Id = new LogLineSourceId(0);
			var source2 = new InMemoryLogFile();
			var source2Id = new LogLineSourceId(1);
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);

			var t1 = new DateTime(2017, 11, 26, 11, 45, 0);
			source1.AddEntry("Foo", LevelFlags.Info, t1);

			var t2 = new DateTime(2017, 11, 26, 11, 45, 1);
			source2.AddMultilineEntry(LevelFlags.Debug, t2, "Hello,", "World!");

			var t3 = new DateTime(2017, 11, 26, 11, 45, 2);
			source1.AddEntry("bar", LevelFlags.Warning, t3);

			_taskScheduler.RunOnce();
			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(4);
			var entries = merged.GetEntries(new[]
			{
				new LogLineIndex(0), new LogLineIndex(1), new LogLineIndex(2), new LogLineIndex(3)
			});
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].GetValue(LogFileColumns.SourceId).Should().Be(source1Id);
			entries[0].RawContent.Should().Be("Foo");
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[0].Timestamp.Should().Be(t1);
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].GetValue(LogFileColumns.SourceId).Should().Be(source2Id);
			entries[1].RawContent.Should().Be("Hello,");
			entries[1].LogLevel.Should().Be(LevelFlags.Debug);
			entries[1].Timestamp.Should().Be(t2);
			entries[2].Index.Should().Be(2);
			entries[2].LogEntryIndex.Should().Be(1);
			entries[2].GetValue(LogFileColumns.SourceId).Should().Be(source2Id);
			entries[2].RawContent.Should().Be("World!");
			entries[2].LogLevel.Should().Be(LevelFlags.Debug);
			entries[2].Timestamp.Should().Be(t2);
			entries[3].Index.Should().Be(3);
			entries[3].LogEntryIndex.Should().Be(2);
			entries[3].GetValue(LogFileColumns.SourceId).Should().Be(source1Id);
			entries[3].RawContent.Should().Be("bar");
			entries[3].LogLevel.Should().Be(LevelFlags.Warning);
			entries[3].Timestamp.Should().Be(t3);
		}

		[Test]
		[Description("Verifies that merging a multi line entry out of order works")]
		public void TestMergeMultiline2()
		{
			var source1 = new InMemoryLogFile();
			var source1Id = new LogLineSourceId(0);
			var source2 = new InMemoryLogFile();
			var source2Id = new LogLineSourceId(1);
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);

			var t1 = new DateTime(2017, 11, 26, 11, 45, 0);
			source1.AddEntry("Foo", LevelFlags.Info, t1);

			var t3 = new DateTime(2017, 11, 26, 11, 45, 2);
			source1.AddEntry("bar", LevelFlags.Warning, t3);

			var t2 = new DateTime(2017, 11, 26, 11, 45, 1);
			source2.AddMultilineEntry(LevelFlags.Debug, t2, "Hello,", "World!");

			_taskScheduler.RunOnce();
			var entries = merged.GetEntries(new LogFileSection(0, 4));
			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(4);
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].GetValue(LogFileColumns.SourceId).Should().Be(source1Id);
			entries[0].RawContent.Should().Be("Foo");
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[0].Timestamp.Should().Be(t1);
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].GetValue(LogFileColumns.SourceId).Should().Be(source2Id);
			entries[1].RawContent.Should().Be("Hello,");
			entries[1].LogLevel.Should().Be(LevelFlags.Debug);
			entries[1].Timestamp.Should().Be(t2);
			entries[2].Index.Should().Be(2);
			entries[2].LogEntryIndex.Should().Be(1);
			entries[2].GetValue(LogFileColumns.SourceId).Should().Be(source2Id);
			entries[2].RawContent.Should().Be("World!");
			entries[2].LogLevel.Should().Be(LevelFlags.Debug);
			entries[2].Timestamp.Should().Be(t2);
			entries[3].Index.Should().Be(3);
			entries[3].LogEntryIndex.Should().Be(2);
			entries[3].GetValue(LogFileColumns.SourceId).Should().Be(source1Id);
			entries[3].RawContent.Should().Be("bar");
			entries[3].LogLevel.Should().Be(LevelFlags.Warning);
			entries[3].Timestamp.Should().Be(t3);
		}

		[Test]
		[Description("Verifies that merging a multi line entry out of order works")]
		public void TestMergeMultiline3()
		{
			var source1 = new InMemoryLogFile();
			var source1Id = new LogLineSourceId(0);
			var source2 = new InMemoryLogFile();
			var source2Id = new LogLineSourceId(1);
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);

			var t1 = new DateTime(2017, 11, 26, 11, 45, 0);
			source1.AddEntry("Foo", LevelFlags.Info, t1);
			_taskScheduler.RunOnce();

			var t3 = new DateTime(2017, 11, 26, 11, 45, 2);
			source1.AddEntry("bar", LevelFlags.Warning, t3);
			_taskScheduler.RunOnce();

			var t2 = new DateTime(2017, 11, 26, 11, 45, 1);
			source2.AddMultilineEntry(LevelFlags.Debug, t2, "Hello,", "World!");
			_taskScheduler.RunOnce();

			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(4);
			var entries = merged.GetEntries(new LogFileSection(0, 4));
			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(4);
			entries[0].Index.Should().Be(0);
			entries[0].LogEntryIndex.Should().Be(0);
			entries[0].GetValue(LogFileColumns.SourceId).Should().Be(source1Id);
			entries[0].RawContent.Should().Be("Foo");
			entries[0].LogLevel.Should().Be(LevelFlags.Info);
			entries[0].Timestamp.Should().Be(t1);
			entries[1].Index.Should().Be(1);
			entries[1].LogEntryIndex.Should().Be(1);
			entries[1].GetValue(LogFileColumns.SourceId).Should().Be(source2Id);
			entries[1].RawContent.Should().Be("Hello,");
			entries[1].LogLevel.Should().Be(LevelFlags.Debug);
			entries[1].Timestamp.Should().Be(t2);
			entries[2].Index.Should().Be(2);
			entries[2].LogEntryIndex.Should().Be(1);
			entries[2].GetValue(LogFileColumns.SourceId).Should().Be(source2Id);
			entries[2].RawContent.Should().Be("World!");
			entries[2].LogLevel.Should().Be(LevelFlags.Debug);
			entries[2].Timestamp.Should().Be(t2);
			entries[3].Index.Should().Be(3);
			entries[3].LogEntryIndex.Should().Be(2);
			entries[3].GetValue(LogFileColumns.SourceId).Should().Be(source1Id);
			entries[3].RawContent.Should().Be("bar");
			entries[3].LogLevel.Should().Be(LevelFlags.Warning);
			entries[3].Timestamp.Should().Be(t3);
		}

		[Test]
		public void TestMergeMultiline4()
		{
			var source = new InMemoryLogFile();
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source);

			source.AddMultilineEntry(LevelFlags.Other, new DateTime(2017, 12, 3, 11, 59, 30), new []
			{
				"2017-12-03 11:59:30 Hello, ",
				"World!"
			});
			_taskScheduler.RunOnce();

			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);

			var entries = merged.GetEntries(new[] { new LogLineIndex(0), new LogLineIndex(1) },
				new ILogFileColumnDescriptor[]
				{
					LogFileColumns.LineNumber, LogFileColumns.LogEntryIndex, LogFileColumns.Timestamp,
					LogFileColumns.RawContent
				});
			var line = entries[0];
			line.GetValue(LogFileColumns.LineNumber).Should().Be(1);
			line.GetValue(LogFileColumns.LogEntryIndex).Should().Be(0);
			line.RawContent.Should().Be("2017-12-03 11:59:30 Hello, ");

			line = entries[1];
			line.GetValue(LogFileColumns.LineNumber).Should().Be(2);
			line.GetValue(LogFileColumns.LogEntryIndex).Should().Be(0);
			line.RawContent.Should().Be("World!");
		}

		[Test]
		//[Ignore("Not yet implemented")]
		[Description("Verifies that changes from many sources are batched together")]
		public void TestManySources1()
		{
			const int sourceCount = 100;
			var sources = new InMemoryLogFile[sourceCount];
			for (int i = 0; i < sourceCount; ++i)
			{
				sources[i] = new InMemoryLogFile();
			}

			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, sources);
			var start = new DateTime(2017, 11, 26, 17, 56, 0);
			for (int i = 0; i < sourceCount; ++i)
			{
				// Sources are modified in order with perfectly ascending timestamps:
				// This is a rather unrealistic scenario...
				sources[i].AddEntry(i.ToString(), LevelFlags.Info, start + TimeSpan.FromSeconds(i));
			}

			var changes = ListenToChanges(merged, sourceCount);
			_taskScheduler.RunOnce();

			// For once, we expect the content of the merged data source to be as expected...
			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(sourceCount, "because every source added one line");

			var buffer = merged.GetEntries(new LogFileSection(0, sourceCount));
			for (byte i = 0; i < sourceCount; ++i)
			{
				buffer[i].Index.Should().Be(i);
				buffer[i].LogEntryIndex.Should().Be(i);
				buffer[i].SourceId.Should().Be(new LogLineSourceId(i));
				buffer[i].RawContent.Should().Be(i.ToString());
				buffer[i].LogLevel.Should().Be(LevelFlags.Info);
				buffer[i].Timestamp.Should().Be(start + TimeSpan.FromSeconds(i));
			}

			// But then it should also have fired as few changes as possible!
			changes.Should().Equal(new object[]
			{
				LogFileSection.Reset,
				new LogFileSection(0, sourceCount)
			});
		}

		[Test]
		[Description("Verifies that changes from many sources are batched together")]
		public void TestManySources2()
		{
			const int sourceCount = 100;
			var sources = new InMemoryLogFile[sourceCount];
			for (int i = 0; i < sourceCount; ++i)
			{
				sources[i] = new InMemoryLogFile();
			}

			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, sources);
			var end = new DateTime(2017, 11, 26, 17, 56, 0);
			for (int i = 0; i < sourceCount; ++i)
			{
				// Sources are modified in  reverse order: This is the worst case.
				// Reality is somewhere in between...
				sources[i].AddEntry(i.ToString(), LevelFlags.Info, end - TimeSpan.FromSeconds(i));
			}

			var changes = ListenToChanges(merged, sourceCount);
			_taskScheduler.RunOnce();

			// For once, we expect the content of the merged data source to be as expected...
			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(sourceCount, "because every source added one line");
			var entries = merged.GetEntries(new LogFileSection(0, sourceCount));
			for (int i = 0; i < sourceCount; ++i)
			{
				var entry = entries[i];
				entry.Index.Should().Be(i);
				entry.LogEntryIndex.Should().Be(i);
				int idx = sourceCount - i - 1;
				entry.SourceId.Should().Be(new LogLineSourceId((byte) idx));
				entry.RawContent.Should().Be(idx.ToString());
				entry.LogLevel.Should().Be(LevelFlags.Info);
				entry.Timestamp.Should().Be(end - TimeSpan.FromSeconds(idx));
			}

			// But then it should also have fired as few changes as possible!
			changes.Should().Equal(new object[]
			{
				LogFileSection.Reset,
				new LogFileSection(0, sourceCount)
			});
		}

		[Test]
		[Description("Verifies that merging a multi line entry in order works")]
		public void TestOriginalDataSourceName1()
		{
			var source1 = new InMemoryLogFile(LogFileColumns.OriginalDataSourceName);
			var source2 = new InMemoryLogFile(LogFileColumns.OriginalDataSourceName);
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);

			source1.Add(new Dictionary<ILogFileColumnDescriptor, object>
			{
				{LogFileColumns.OriginalDataSourceName, "important_document.txt" },
				{LogFileColumns.Timestamp, new DateTime(2021, 02, 11, 23, 33, 10)}
			});

			source2.Add(new Dictionary<ILogFileColumnDescriptor, object>
			{
				{LogFileColumns.OriginalDataSourceName, "rubbish.log" },
				{LogFileColumns.Timestamp, new DateTime(2021, 02, 11, 23, 29, 10)}
			});

			_taskScheduler.RunOnce();
			merged.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);
			var entries = merged.GetEntries(new LogFileSection(0, 2));
			entries[0].GetValue(LogFileColumns.OriginalDataSourceName).Should().Be("rubbish.log");
			entries[0].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(1));
			entries[1].GetValue(LogFileColumns.OriginalDataSourceName).Should().Be("important_document.txt");
			entries[1].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(0));

			entries = merged.GetEntries(new []{new LogLineIndex(1), new LogLineIndex(0) });
			entries[0].GetValue(LogFileColumns.OriginalDataSourceName).Should().Be("important_document.txt");
			entries[0].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(0));
			entries[1].GetValue(LogFileColumns.OriginalDataSourceName).Should().Be("rubbish.log");
			entries[1].GetValue(LogFileColumns.SourceId).Should().Be(new LogLineSourceId(1));
		}

		[Test]
		[Description("Verifies that starting a merged log file causes it to add listeners with the source files")]
		public void TestStart1()
		{
			var source = new Mock<ILogFile>();
			source.SetupGet(x => x.Columns).Returns(new ILogFileColumnDescriptor[0]);

			var listeners = new List<Tuple<ILogFileListener, TimeSpan, int>>();
			source.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			      .Callback(
				      (ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount) =>
				      listeners.Add(Tuple.Create(listener, maximumWaitTime, maximumLineCount)));

			TimeSpan waitTime = TimeSpan.FromSeconds(1);
			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.FromSeconds(1), source.Object);

			listeners.Count.Should()
			         .Be(1, "Because the merged file should have registered exactly 1 listener with the source file");
			listeners[0].Item1.Should().NotBeNull();
			listeners[0].Item2.Should().Be(waitTime);
			listeners[0].Item3.Should().BeGreaterThan(0);

			GC.KeepAlive(logFile);
		}

		#region Well Known Columns

		#region Original Index

		[Test]
		public void TestGetOriginalIndicesBySection2()
		{
			var source1 = new InMemoryLogFile();
			source1.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 20, 23, 1, 0)});

			var source2 = new InMemoryLogFile();
			source1.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 20, 23, 0, 0)});

			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);
			_taskScheduler.RunOnce();
			logFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);

			var lineNumbers = logFile.GetColumn(new LogFileSection(0, 2), LogFileColumns.Index);
			lineNumbers[0].Should().Be(0);
			lineNumbers[1].Should().Be(1);

			lineNumbers = logFile.GetColumn(new LogFileSection(0, 2), LogFileColumns.OriginalIndex);
			lineNumbers[0].Should().Be(0);
			lineNumbers[1].Should().Be(1);
		}

		[Test]
		public void TestGetOriginalIndexByIndices()
		{
			var source1 = new InMemoryLogFile();
			source1.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 20, 23, 1, 0)});

			var source2 = new InMemoryLogFile();
			source1.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 20, 23, 0, 0)});

			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);
			_taskScheduler.RunOnce();
			logFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);

			var lineNumbers = logFile.GetColumn(new LogLineIndex[] {1, 0}, LogFileColumns.Index);
			lineNumbers[0].Should().Be(1);
			lineNumbers[1].Should().Be(0);

			lineNumbers = logFile.GetColumn(new LogLineIndex[] {1, 0}, LogFileColumns.OriginalIndex);
			lineNumbers[0].Should().Be(1);
			lineNumbers[1].Should().Be(0);
		}

		#endregion

		#region Line Number / Original Line Number

		[Test]
		public void TestGetLineNumbersBySection()
		{
			var source1 = new InMemoryLogFile();
			source1.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 20, 23, 1, 0)});

			var source2 = new InMemoryLogFile();
			source1.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 20, 23, 0, 0)});

			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);
			_taskScheduler.RunOnce();
			logFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);

			var lineNumbers = logFile.GetColumn(new LogFileSection(0, 2), LogFileColumns.LineNumber);
			lineNumbers[0].Should().Be(1);
			lineNumbers[1].Should().Be(2);

			lineNumbers = logFile.GetColumn(new LogFileSection(0, 2), LogFileColumns.OriginalLineNumber);
			lineNumbers[0].Should().Be(1);
			lineNumbers[1].Should().Be(2);
		}

		[Test]
		public void TestGetLineNumbersByIndices()
		{
			var source1 = new InMemoryLogFile();
			source1.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 20, 23, 1, 0)});

			var source2 = new InMemoryLogFile();
			source1.Add(new LogEntry {Timestamp = new DateTime(2017, 12, 20, 23, 0, 0)});

			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);
			_taskScheduler.RunOnce();
			logFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);

			var lineNumbers = logFile.GetColumn(new LogLineIndex[] {1, 0}, LogFileColumns.LineNumber);
			lineNumbers[0].Should().Be(2);
			lineNumbers[1].Should().Be(1);

			lineNumbers = logFile.GetColumn(new LogLineIndex[] {1, 0}, LogFileColumns.OriginalLineNumber);
			lineNumbers[0].Should().Be(2);
			lineNumbers[1].Should().Be(1);
		}

		#endregion

		#region Timestamp

		[Test]
		[Description("Verifies that a continuous section of values can be retrieved for one column")]
		public void TestGetTimestampsOneSource1([Range(0, 3)] int offset)
		{
			var source = new InMemoryLogFile();
			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source);

			source.AddEntry("", LevelFlags.Other, new DateTime(2017, 12, 14, 23, 27, 0));
			source.AddEntry("", LevelFlags.Other, new DateTime(2017, 12, 14, 23, 28, 23));
			int count = source.Count;
			_taskScheduler.Run(2);

			var buffer = new DateTime?[offset + count];
			for (int i = 0; i < offset + count; ++i)
			{
				buffer[i] = DateTime.MinValue;
			}

			logFile.GetColumn(new LogFileSection(0, count), LogFileColumns.Timestamp, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(DateTime.MinValue, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			for (int i = 0; i < count; ++i)
			{
				buffer[offset + i].Should().Be(source.GetEntry(i).Timestamp);
			}
		}

		[Test]
		[Description("Verifies that column values can be retrieved by indices")]
		public void TestGetTimestampsOneSource2([Range(0, 3)] int offset)
		{
			var source = new InMemoryLogFile();
			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source);

			source.AddEntry("", LevelFlags.Other, new DateTime(2017, 12, 14, 23, 27, 0));
			source.AddEntry("", LevelFlags.Other, new DateTime(2017, 12, 14, 23, 28, 23));
			source.AddEntry("", LevelFlags.Other, new DateTime(2017, 12, 14, 23, 29, 1));
			int count = source.Count;
			_taskScheduler.Run(2);

			var buffer = new DateTime?[offset + count];
			for (int i = 0; i < offset + count; ++i)
			{
				buffer[i] = DateTime.MinValue;
			}

			logFile.GetColumn(new LogLineIndex[] {2, 1}, LogFileColumns.Timestamp, buffer, offset);

			for (int i = 0; i < offset; ++i)
			{
				buffer[i].Should().Be(DateTime.MinValue, "because we've specified an offset and thus values before that offset shouldn't have been touched");
			}
			buffer[offset + 0].Should().Be(source.GetEntry(2).Timestamp);
			buffer[offset + 1].Should().Be(source.GetEntry(1).Timestamp);
		}

		#endregion

		#region Delta Time

		[Test]
		public void TestGetDeltaTimesOneSource1()
		{
			var source = new InMemoryLogFile();
			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source);

			source.AddEntry("", LevelFlags.Other, new DateTime(2017, 12, 14, 23, 27, 0));
			source.AddEntry("", LevelFlags.Other, new DateTime(2017, 12, 14, 23, 28, 23));
			_taskScheduler.Run(2);

			var deltaTimes = logFile.GetColumn(new LogFileSection(0, 2), LogFileColumns.DeltaTime);
			deltaTimes.Should().Equal(new object[]
			{
				null,
				TimeSpan.FromSeconds(83)
			});
		}

		[Test]
		public void TestGetDeltaTimesOneSource2()
		{
			var source = new InMemoryLogFile();
			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source);

			source.AddEntry("", LevelFlags.Other, new DateTime(2017, 12, 14, 23, 27, 0));
			source.AddEntry("", LevelFlags.Other, new DateTime(2017, 12, 14, 23, 28, 23));
			_taskScheduler.Run(2);

			var deltaTimes = logFile.GetColumn(new LogLineIndex[] {1, 0}, LogFileColumns.DeltaTime);
			deltaTimes.Should().Equal(new object[]
			{
				TimeSpan.FromSeconds(83),
				null
			});
		}

		#endregion

		#endregion

		protected override ILogFile CreateEmpty()
		{
			return new MergedLogFile(_taskScheduler, TimeSpan.Zero);
		}

		protected override ILogFile CreateFromContent(IReadOnlyLogEntries content)
		{
			var source = new InMemoryLogFile(content);
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source);
			_taskScheduler.RunOnce();
			return merged;
		}
	}
}