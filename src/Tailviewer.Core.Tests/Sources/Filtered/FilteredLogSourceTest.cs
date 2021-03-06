﻿using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Api.Tests;

namespace Tailviewer.Core.Tests.Sources.Filtered
{
	[TestFixture]
	public sealed class FilteredLogSourceTest
		: AbstractLogSourceTest
	{
		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new ManualTaskScheduler();
			_logFile = new Mock<ILogSource>();
			_logFile.SetupGet(x => x.Columns).Returns(Core.Columns.Minimum);
			_logFile.SetupGet(x => x.Properties).Returns(new IReadOnlyPropertyDescriptor[]{Core.Properties.LogEntryCount, Core.Properties.PercentageProcessed});
			_logFile.Setup(x => x.GetProperty(Core.Properties.LogEntryCount)).Returns(() => 0);
			_logFile.Setup(x => x.GetProperty(Core.Properties.PercentageProcessed)).Returns(Percentage.HundredPercent);
			_logFile.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
			        .Callback((IPropertiesBuffer destination) =>
			        {
				        destination.SetValue(Core.Properties.LogEntryCount,
				                             _logFile.Object.GetProperty(Core.Properties.LogEntryCount));
				        destination.SetValue(Core.Properties.PercentageProcessed,
				                             _logFile.Object.GetProperty(Core.Properties.PercentageProcessed));
			        });

			_modifications = new List<LogSourceModification>();
			_listener = new Mock<ILogSourceListener>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
			         .Callback((ILogSource l, LogSourceModification s) => _modifications.Add(s));
		}

		private Mock<ILogSource> _logFile;
		private List<LogSourceModification> _modifications;
		private Mock<ILogSourceListener> _listener;
		private ManualTaskScheduler _taskScheduler;

		[Test]
		public void TestChangeColumns()
		{
			_logFile.SetupGet(x => x.Columns).Returns(new IColumnDescriptor[]
				                                          {Core.Columns.RawContent, Core.Columns.LogLevel});
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, _logFile.Object, null,
			                                      Filter.Create(null, true, LevelFlags.Debug)))
			{
				file.Columns.Should().Contain(new IColumnDescriptor[]
					                            {Core.Columns.RawContent, Core.Columns.LogLevel});

				var customColumn = new Mock<IColumnDescriptor>().Object;
				_logFile.SetupGet(x => x.Columns).Returns(new IColumnDescriptor[]
					                                          {Core.Columns.RawContent, Core.Columns.LogLevel, customColumn});
				file.Columns.Should().Contain(new IColumnDescriptor[]
					                            {Core.Columns.RawContent, Core.Columns.LogLevel, customColumn});
			}
		}

		[Test]
		public void TestConstruction()
		{
			var source = new InMemoryLogSource();
			source.AddMultilineEntry(LevelFlags.Debug, null, "DEBUG: This is a test", "Yikes");

			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null,
				Filter.Create(null, true, LevelFlags.Debug)))
			{
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.Zero, "because the filtered log file hasn't consumed anything of its source (yet)");

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "because the filtered log file has consumed the entire source");
			}
		}

		[Test]
		[Description("Verifies that the filtered log file correctly listens to a reset event")]
		public void TestClear()
		{
			var source = new InMemoryLogSource();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null, Filter.Create(null, true, LevelFlags.Debug)))
			{
				source.AddMultilineEntry(LevelFlags.Debug, null, "DEBUG: This is a test", "DEBUG: Yikes");

				_taskScheduler.RunOnce();

				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

				source.Clear();

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(0);
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
			}
		}

		[Test]
		public void TestEmptyLogFile1()
		{
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create("Test", true, LevelFlags.All)))
			{
				_taskScheduler.RunOnce();
				
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(0);
				file.GetLogLineIndexOfOriginalLineIndex(new LogLineIndex(0)).Should().Be(LogLineIndex.Invalid);
			}
		}

		[Test]
		public void TestEntryLevelNone()
		{
			var source = new InMemoryLogSource();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null, Filter.Create("ello", true, LevelFlags.All)))
			{
				source.AddEntry("Hello world!", LevelFlags.Other);

				_taskScheduler.RunOnce();
				
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(1);
				var entries = file.GetEntries(new LogSourceSection(0, 1), Core.Columns.Minimum);
				entries.Should().HaveCount(1);
				entries[0].Index.Should().Be(0);
				entries[0].OriginalIndex.Should().Be(0);
				entries[0].LogEntryIndex.Should().Be(0);
				entries[0].RawContent.Should().Be("Hello world!");
				entries[0].LogLevel.Should().Be(LevelFlags.Other);
			}
		}

		[Test]
		public void TestCreated()
		{
			var created = new DateTime(2017, 12, 21, 20, 51, 0);
			var props = new Dictionary<IReadOnlyPropertyDescriptor, object> {[Core.Properties.Created] = created};
			var source = new InMemoryLogSource(props);

			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null,
			                                      Filter.Create(null, true, LevelFlags.Info)))
			{
				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.Created).Should().Be(created, "because the creation date of the source should be forwarded since that is of interest to the user");
			}
		}

		[Test]
		public void TestLastModified()
		{
			var lastModified = new DateTime(2017, 12, 21, 20, 52, 0);
			var props = new Dictionary<IReadOnlyPropertyDescriptor, object> {[Core.Properties.LastModified] = lastModified};
			var source = new InMemoryLogSource(props);

			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null,
			                                      Filter.Create(null, true, LevelFlags.Info)))
			{
				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.LastModified).Should().Be(lastModified, "because the last modification date of the source should be forwarded since that is of interest to the user");
			}
		}

		[Test]
		public void TestSize()
		{
			var size = Size.FromGigabytes(5);
			var props = new Dictionary<IReadOnlyPropertyDescriptor, object> {[Core.Properties.Size] = size};
			var source = new InMemoryLogSource(props);

			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null,
			                                      Filter.Create(null, true, LevelFlags.Info)))
			{
				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.Size).Should().Be(size, "because the size of the source should be forwarded since that is of interest to the user");
			}
		}

		[Test]
		public void TestInvalidate1()
		{
			var source = new InMemoryLogSource();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null, Filter.Create(null, true, LevelFlags.Info)))
			{
				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

				source.AddRange(new[]
				{
					new LogEntry { RawContent = "A" },
					new LogEntry { RawContent = "B" },
					new LogEntry { RawContent = "C" },
					new LogEntry { RawContent = "D" }
				});
				source.RemoveFrom(2);

				_taskScheduler.RunOnce();
				
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(2, "because we've invalidated the last 2 out of 4 lines");
			}
		}

		[Test]
		public void TestInvalidate2()
		{
			var source = new InMemoryLogSource();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null, Filter.Create(null, true, LevelFlags.Info)))
			{
				file.AddListener(_listener.Object, TimeSpan.Zero, 1);

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

				source.AddRange(new []
				{
					new LogEntry(Core.Columns.Minimum){RawContent = "A", LogLevel = LevelFlags.Info},
					new LogEntry(Core.Columns.Minimum){RawContent = "B", LogLevel = LevelFlags.Info},
					new LogEntry(Core.Columns.Minimum){RawContent = "C", LogLevel = LevelFlags.Info},
					new LogEntry(Core.Columns.Minimum){RawContent = "D", LogLevel = LevelFlags.Info},
				});

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(4);

				source.RemoveFrom(2);

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);

				_modifications.Should().Equal(new[]
					{
						LogSourceModification.Reset(),
						LogSourceModification.Appended(0, 1),
						LogSourceModification.Appended(1, 1),
						LogSourceModification.Appended(2, 1),
						LogSourceModification.Appended(3, 1),
						LogSourceModification.Removed(2, 2)
					});
			}
		}

		[Test]
		[Description(
			"Verifies that the FilteredLogFile won't get stuck in an endless loop when an Invalidate() follows a multiline log entry"
			)]
		public void TestInvalidate3()
		{
			var source = new InMemoryLogSource();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null, Filter.Create(null, true, LevelFlags.Info)))
			{
				file.AddListener(_listener.Object, TimeSpan.Zero, 1);

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

				source.AddMultilineEntry(LevelFlags.Info, null, "A", "B", "C", "D");

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

				source.RemoveFrom(2);

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent, "Because the filtered log file should be finished");
				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);

				_modifications.Should().Equal(new[]
					{
						LogSourceModification.Reset(),
						LogSourceModification.Appended(0, 1),
						LogSourceModification.Appended(1, 1),
						LogSourceModification.Appended(2, 1),
						LogSourceModification.Appended(3, 1),
						LogSourceModification.Removed(2, 2)
					});
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/221")]
		public void TestInvalidate4()
		{
			var logFile = new InMemoryLogSource();

			using (var multiLine = new MultiLineLogSource(_taskScheduler, logFile, TimeSpan.Zero))
			using (var filtered = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, multiLine, null,
			                                      Filter.Create(null, true, LevelFlags.Info)))
			{
				logFile.AddRange(new[]
				{
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 19, 195), LogLevel = LevelFlags.Info, RawContent = "2017-03-24 11-45-19.195339; 0; 0;  0; 108;  0; 124;   1;INFO; ; ; ; ; ; 0; Some interesting message"},
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 19, 751), LogLevel = LevelFlags.Info, RawContent = "2017-03-24 11-45-19.751428; 0; 0;  0; 129;  0; 145;   1;INFO; ; ; ; ; ; 0; Very interesting stuff"},
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 21, 708), LogLevel = LevelFlags.Other, RawContent = "2017-03-24 11-45-21.708485; 0; 0;  0; 109;  0; 125;   1;PB_CREATE; ; ; 109; 2;"}
				});

				_taskScheduler.RunOnce();
				filtered.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);
				filtered.GetEntry(0).OriginalIndex.Should().Be(0);
				filtered.GetEntry(1).OriginalIndex.Should().Be(1);


				logFile.RemoveFrom(new LogLineIndex(2));
				logFile.AddRange(new []
				{
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 21, 708), LogLevel = LevelFlags.Other, RawContent = "2017-03-24 11-45-21.708485; 0; 0;  0; 109;  0; 125;   1;PB_CREATE; ; ; 109; 2; Sooo interesting"},
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 21, 708), LogLevel = LevelFlags.Info, RawContent = "2017-03-24 11-45-21.708599; 0; 0;  0; 108;  0; 124;   1;INFO; ; ; ; ; ; 0; Go on!"},
					new LogEntry{Timestamp = new DateTime(2017, 3, 24, 11, 45, 21, 811), LogLevel = LevelFlags.Info, RawContent = "2017-03-24 11-45-21.811838; 0; 0;  0; 108;  0; 124;   1;INFO; ; ; ; ; ; 0; done."}
				});
				_taskScheduler.RunOnce();
				filtered.GetProperty(Core.Properties.LogEntryCount).Should().Be(4);
				filtered.GetEntry(0).OriginalIndex.Should().Be(0);
				filtered.GetEntry(1).OriginalIndex.Should().Be(1);
				filtered.GetEntry(2).OriginalIndex.Should().Be(3);
				filtered.GetEntry(3).OriginalIndex.Should().Be(4);
			}
		}

		[Test]
		[Description(
			"Verifies that listeners are notified eventually, even when the # of filtered entries is less than the minimum batch size"
			)]
		public void TestListener()
		{
			var source = new InMemoryLogSource();
			source.AddMultilineEntry(LevelFlags.Debug, null, "DEBUG: This is a test", "Yikes");
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null, Filter.Create("yikes", true, LevelFlags.All)))
			{
				var modifications = new List<LogSourceModification>();
				var listener = new Mock<ILogSourceListener>();

				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
				        .Callback((ILogSource l, LogSourceModification s) => modifications.Add(s));
				// We deliberately set the batchSize to be greater than the amount of entries that will be matched.
				// If the FilteredLogFile is implemented correctly, then it will continously notifiy the listener until
				// the maximum wait time is elapsed.
				const int batchSize = 10;
				file.AddListener(listener.Object, TimeSpan.FromMilliseconds(100), batchSize);
				file.OnLogFileModified(source, LogSourceModification.Appended(0, 2));

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);
				modifications.Should().Equal(new[]
					{
						LogSourceModification.Reset(),
						LogSourceModification.Appended(0, 2)
					});
			}
		}

		[Test]
		[Description("Verifies that all lines belonging to an entry pass the filter, even though only one line passes it")]
		public void TestMultiLineLogEntry1()
		{
			var source = new InMemoryLogSource();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null,
			                                      Filter.Create("Test", true, LevelFlags.All)))
			{
				source.AddMultilineEntry(LevelFlags.Debug, null, "DEBUG: This is a test", "Yikes");

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);
				var entries = file.GetEntries(new LogSourceSection(0, 2), new IColumnDescriptor[]{Core.Columns.Index, Core.Columns.LogEntryIndex, Core.Columns.RawContent, Core.Columns.LogLevel});
				entries[0].Index.Should().Be(0);
				entries[0].LogEntryIndex.Should().Be(0);
				entries[0].RawContent.Should().Be("DEBUG: This is a test");
				entries[0].LogLevel.Should().Be(LevelFlags.Debug);
				entries[1].Index.Should().Be(1);
				entries[1].LogEntryIndex.Should().Be(0);
				entries[1].RawContent.Should().Be("Yikes");
				entries[1].LogLevel.Should().Be(LevelFlags.Debug);
			}
		}

		[Test]
		[Description(
			"Verifies that all lines belonging to an entry pass the filter, even though only the second line passes it")]
		public void TestMultiLineLogEntry2()
		{
			var source = new InMemoryLogSource();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null, Filter.Create("yikes", true, LevelFlags.All)))
			{
				source.AddMultilineEntry( LevelFlags.Debug, null, "DEBUG: This is a test", "Yikes");

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);
				var entries = file.GetEntries(new LogSourceSection(0, 2));
				entries[0].Index.Should().Be(0);
				entries[0].LogEntryIndex.Should().Be(0);
				entries[0].RawContent.Should().Be("DEBUG: This is a test");
				entries[0].LogLevel.Should().Be(LevelFlags.Debug);
				entries[1].Index.Should().Be(1);
				entries[1].LogEntryIndex.Should().Be(0);
				entries[1].RawContent.Should().Be("Yikes");
				entries[1].LogLevel.Should().Be(LevelFlags.Debug);
			}
		}

		[Test]
		[Description("Verifies that the filtered log file repeatedly calls the listener when the source has been fully read")]
		public void TestWait()
		{
			var source = new InMemoryLogSource();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null, Filter.Create(null, true, LevelFlags.Debug)))
			{
				var modifications = new List<LogSourceModification>();
				var listener = new Mock<ILogSourceListener>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
				        .Callback((ILogSource logFile, LogSourceModification modification) => modifications.Add(modification));
				file.AddListener(listener.Object, TimeSpan.FromMilliseconds(100), 3);

				source.AddMultilineEntry(LevelFlags.Debug, null, "DEBUG: This is a test", "Yikes");

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
				modifications.Should().Equal(new object[]
					{
						LogSourceModification.Reset(),
						LogSourceModification.Appended(new LogLineIndex(0), 2)
					});
			}
		}

		[Test]
		[Description("Verifies that filtered log entries present the correct index from the view of the filtered file")]
		public void TestGetEntries1()
		{
			var source = new InMemoryLogSource();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null, Filter.Create("yikes", true, LevelFlags.All)))
			{
				source.AddEntry("DEBUG: This is a test", LevelFlags.Debug);
				source.AddEntry("Yikes", LevelFlags.Info);
				file.OnLogFileModified(_logFile.Object, LogSourceModification.Appended(0, 2));

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

				var entries = file.GetEntries(new LogSourceSection(0, 1), file.Columns);
				entries.Should().NotBeNull();
				entries.Count.Should().Be(1);
				entries[0].Index.Should().Be(0, "because the filtered log file only represents a file with one line, thus the only entry should have an index of 0, not 1, which is the original index");
				entries[0].OriginalIndex.Should().Be(1, "because the given line is the second line in the source file");
				entries[0].RawContent.Should().Be("Yikes");
				entries[0].LogLevel.Should().Be(LevelFlags.Info);
			}
		}

		[Test]
		[Description("Verifies that filtered log entries present the correct index from the view of the filtered file")]
		public void TestGetEntry1()
		{
			var source = new InMemoryLogSource();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null, Filter.Create("yikes", true, LevelFlags.All)))
			{
				source.AddEntry("DEBUG: This is a test", LevelFlags.Debug);
				source.AddEntry("Yikes", LevelFlags.Other);

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

				var entry = file.GetEntry(0);
				entry.Index.Should().Be(0, "because the filtered log file only represents a file with one line, thus the only entry should have an index of 0, not 1, which is the original index");
				entry.RawContent.Should().Be("Yikes");
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/154")]
		[Description("Verifies that filtered log file actually returns the correct source id")]
		public void TestGetEntry2()
		{
			var source = new InMemoryLogSource(Core.Columns.SourceId);
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, null, Filter.Create(LevelFlags.Error)))
			{
				source.Add(new Dictionary<IColumnDescriptor, object>
				{
					{Core.Columns.SourceId, new LogEntrySourceId(0) },
					{Core.Columns.RawContent, "DEBUG: This is a test"},
					{Core.Columns.LogLevel, LevelFlags.Debug }
				});

				source.Add(new Dictionary<IColumnDescriptor, object>
				{
					{Core.Columns.SourceId, new LogEntrySourceId(42) },
					{Core.Columns.RawContent, "ERROR: I feel a disturbance in the source"},
					{Core.Columns.LogLevel, LevelFlags.Error }
				});

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);

				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(1, "because only one line matches the filter");
				var entry = file.GetEntry(0);
				entry.Index.Should().Be(0);
				entry.LogEntryIndex.Should().Be(0);
				entry.OriginalIndex.Should().Be(1);
				entry.RawContent.Should().Be("ERROR: I feel a disturbance in the source");
				entry.GetValue(Core.Columns.SourceId).Should().Be(new LogEntrySourceId(42), "Because the filtered log file is supposed to simply forward the source id of the log line in question (Issue #154)");
			}
		}

		[Test]
		[Description("Verifies that the log file queries the LogLineFilter for each added entry")]
		public void TestSingleLineFilter1()
		{
			var source = new InMemoryLogSource();
			var filter = new Mock<ILogLineFilter>();
			filter.Setup(x => x.PassesFilter(It.IsAny<IReadOnlyLogEntry>())).Returns(true);
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter.Object, null))
			{
				var logEntry = source.AddEntry("DEBUG: This is a test", LevelFlags.Debug);
				_taskScheduler.RunOnce();

				filter.Verify(x => x.PassesFilter(It.Is<IReadOnlyLogEntry>(y => Equals(y, logEntry))), Times.AtLeastOnce,
					"because the log file should've used our filter at least once to determine if the given log line should've been added");

				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(1, "because the filter should've passed the only log line");
			}
		}

		[Test]
		[Description("Verifies that the log file honors the result of the log line filter")]
		public void TestSingleLineFilter2()
		{
			var source = new InMemoryLogSource();
			var filter = new Mock<ILogLineFilter>();
			filter.Setup(x => x.PassesFilter(It.IsAny<IReadOnlyLogEntry>())).Returns(false);
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter.Object, null))
			{
				source.AddEntry("DEBUG: This is a test", LevelFlags.Debug);
				_taskScheduler.RunOnce();

				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(0, "because the log line filter didn't pass the added line");

				source.AddEntry("INFO: Something mundane", LevelFlags.Info);
				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(0, "because the log line filter didn't pass the added line");
			}
		}

		[Test]
		[Description("Verifies that the log line filter is used per log line")]
		public void TestSingleLineFilter3()
		{
			var source = new InMemoryLogSource();
			var filter = new EmptyLogLineFilter();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("DEBUG: This is a test", LevelFlags.Debug);
				source.AddEntry("More stuff", LevelFlags.Debug);
				source.AddEntry("", LevelFlags.Debug);
				source.AddEntry("And even more stuff", LevelFlags.Debug);
				_taskScheduler.RunOnce();

				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(3, "because the log file should've filtered out the one log line that is empty");
			}
		}

		[Test]
		[Description("Verifies that the filter adjusts log entry indices to be consecutive once again")]
		public void TestSingleLineFilter4()
		{
			var source = new InMemoryLogSource();
			var filter = new EmptyLogLineFilter();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("DEBUG: This is a test", LevelFlags.Debug);
				source.AddEntry("More stuff", LevelFlags.Debug);
				source.AddEntry("", LevelFlags.Debug);
				source.AddEntry("And even more stuff", LevelFlags.Debug);
				_taskScheduler.RunOnce();

				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(3, "because the log file should've filtered out the one log line that is empty");

				const string reason = "because log entry indices are supposed to be consecutive for a data source";
				file.GetEntry(0).LogEntryIndex.Should().Be(0, reason);
				file.GetEntry(1).LogEntryIndex.Should().Be(1, reason);
				file.GetEntry(2).LogEntryIndex.Should().Be(2, reason);
			}
		}

		[Test]
		[Description("Verifies that the filter adjusts log entry indices to be consecutive once again")]
		public void TestSingleLineFilter5()
		{
			var source = new InMemoryLogSource();
			var filter = new EmptyLogLineFilter();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("DEBUG: This is a test", LevelFlags.Debug);
				source.AddEntry("More stuff", LevelFlags.Debug);
				source.AddEntry("", LevelFlags.Debug);
				source.AddEntry("And even more stuff", LevelFlags.Debug);
				file.OnLogFileModified(_logFile.Object, LogSourceModification.Appended(0, 4));
				_taskScheduler.RunOnce();

				file.OnLogFileModified(_logFile.Object, LogSourceModification.Reset());
				_taskScheduler.RunOnce();

				file.OnLogFileModified(_logFile.Object, LogSourceModification.Appended(0, 1));
				_taskScheduler.RunOnce();

				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(1, "because only one line remains in the source");
				file.GetEntry(0).LogEntryIndex.Should().Be(0, "because log entry indices should always start at 0");
			}
		}

		[Test]
		[Description("Verifies that the filter adjusts log entry indices to be consecutive once again")]
		public void TestSingleLineFilter6()
		{
			var source = new InMemoryLogSource();
			var filter = new EmptyLogLineFilter();
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("DEBUG: This is a test", LevelFlags.Debug);
				source.AddEntry("More stuff", LevelFlags.Debug);
				source.AddEntry("", LevelFlags.Debug);
				source.AddEntry("And even more stuff", LevelFlags.Debug);
				source.AddEntry("And even more stuff", LevelFlags.Debug);
				file.OnLogFileModified(source, LogSourceModification.Appended(0, 5));
				_taskScheduler.RunOnce();

				file.OnLogFileModified(source, LogSourceModification.Removed(3, 2));
				_taskScheduler.RunOnce();

				file.OnLogFileModified(source, LogSourceModification.Appended(3, 2));
				_taskScheduler.RunOnce();
				
				file.GetProperty(Core.Properties.LogEntryCount).Should().Be(4, "because the source represents 4 lines (of which the last two changed over its lifetime)");

				const string reason = "because log entry indices are supposed to be consecutive for a data source";
				file.GetEntry(0).LogEntryIndex.Should().Be(0, reason);
				file.GetEntry(1).LogEntryIndex.Should().Be(1, reason);
				file.GetEntry(2).LogEntryIndex.Should().Be(2, reason);
				file.GetEntry(3).LogEntryIndex.Should().Be(3, reason);
			}
		}

		[Test]
		[Description("Verifies that the log file unregisters itself as a listener from the source upon being removed")]
		public void TestDispose1()
		{
			var filter = new EmptyLogLineFilter();
			FilteredLogSource source = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, _logFile.Object, filter, null);
			_logFile.Verify(x => x.AddListener(It.Is<ILogSourceListener>(y => Equals(y, source)),
				It.IsAny<TimeSpan>(),
				It.IsAny<int>()), Times.Once, "because the filtered log file should register itself as a listener with its source");

			new Action(() => source.Dispose()).Should().NotThrow("because Dispose() must always succeed");

			_logFile.Verify(x => x.RemoveListener(It.Is<ILogSourceListener>(y => Equals(y, source))), Times.Once,
				"because the filtered log file should unregister itself as a listener from its source when being disposed of");
		}

		[Test]
		public void TestGetLogLineIndexOfOriginalLineIndex1()
		{
			var source = new InMemoryLogSource();
			var filter = new LevelFilter(LevelFlags.Info);
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("This is a test", LevelFlags.Debug);
				source.AddEntry("This is a test", LevelFlags.Info);
				source.AddEntry("This is a test", LevelFlags.Error);
				source.AddEntry("This is a test", LevelFlags.Info);
				file.OnLogFileModified(_logFile.Object, LogSourceModification.Appended(0, 4));
				_taskScheduler.RunOnce();

				file.GetLogLineIndexOfOriginalLineIndex(new LogLineIndex(0)).Should().Be(LogLineIndex.Invalid);
				file.GetLogLineIndexOfOriginalLineIndex(new LogLineIndex(1)).Should().Be(new LogLineIndex(0));
				file.GetLogLineIndexOfOriginalLineIndex(new LogLineIndex(2)).Should().Be(LogLineIndex.Invalid);
				file.GetLogLineIndexOfOriginalLineIndex(new LogLineIndex(3)).Should().Be(new LogLineIndex(1));
			}
		}

		#region Well Known Columns

		#region Original Index
		
		[Test]
		public void TestGetOriginalIndicesFrom3()
		{
			var source = new InMemoryLogSource();
			var filter = new LevelFilter(LevelFlags.Info);
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("This is a test", LevelFlags.Debug);
				source.AddEntry("This is a test", LevelFlags.Info);
				source.AddEntry("This is a test", LevelFlags.Error);
				source.AddEntry("This is a test", LevelFlags.Info);
				file.OnLogFileModified(_logFile.Object, LogSourceModification.Appended(0, 4));
				_taskScheduler.RunOnce();

				var originalIndices = file.GetColumn(new LogSourceSection(0, 2), Core.Columns.OriginalIndex);
				originalIndices.Should().Equal(new LogLineIndex(1), new LogLineIndex(3));
			}
		}

		[Test]
		public void TestGetOriginalIndicesFrom4()
		{
			var source = new InMemoryLogSource();
			var filter = new LevelFilter(LevelFlags.Info);
			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("This is a test", LevelFlags.Debug);
				source.AddEntry("This is a test", LevelFlags.Info);
				source.AddEntry("This is a test", LevelFlags.Error);
				source.AddEntry("This is a test", LevelFlags.Info);
				source.AddEntry("This is a test", LevelFlags.Error);
				source.AddEntry("This is a test", LevelFlags.Info);
				file.OnLogFileModified(_logFile.Object, LogSourceModification.Appended(0, 6));
				_taskScheduler.RunOnce();

				var originalIndices = file.GetColumn(new LogLineIndex[] {0, 2}, Core.Columns.OriginalIndex);
				originalIndices.Should().Equal(new LogLineIndex(1), new LogLineIndex(5));
			}
		}

		#endregion

		#region Line Number

		[Test]
		public void TestGetLineNumbersBySection()
		{
			var filter = new SubstringFilter("B", true);
			var source = new InMemoryLogSource();
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "B" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "B" });
			var filteredLogFile = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null);
			_taskScheduler.RunOnce();

			filteredLogFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);
			var lineNumbers = filteredLogFile.GetColumn(new LogSourceSection(0, 2), Core.Columns.LineNumber);
			lineNumbers[0].Should().Be(1);
			lineNumbers[1].Should().Be(2);
		}

		[Test]
		public void TestGetLineNumbersByIndices1()
		{
			var filter = new SubstringFilter("B", true);
			var source = new InMemoryLogSource();
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "B" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "B" });
			var filteredLogFile = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null);
			_taskScheduler.RunOnce();

			filteredLogFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);
			var lineNumbers = filteredLogFile.GetColumn(new LogLineIndex[] {1, 0}, Core.Columns.LineNumber);
			lineNumbers[0].Should().Be(2);
			lineNumbers[1].Should().Be(1);

			lineNumbers = filteredLogFile.GetColumn(new LogLineIndex[] {1}, Core.Columns.LineNumber);
			lineNumbers[0].Should().Be(2);
		}

		#endregion

		#region Original Line Number

		[Test]
		public void TestGetOriginalLineNumbersBySection()
		{
			var filter = new SubstringFilter("B", true);
			var source = new InMemoryLogSource();
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "B" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "B" });
			var filteredLogFile = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null);
			_taskScheduler.RunOnce();

			filteredLogFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);
			var lineNumbers = filteredLogFile.GetColumn(new LogSourceSection(0, 2), Core.Columns.OriginalLineNumber);
			lineNumbers[0].Should().Be(2);
			lineNumbers[1].Should().Be(4);
		}

		[Test]
		public void TestGetOriginalLineNumbersByIndices()
		{
			var filter = new SubstringFilter("B", true);
			var source = new InMemoryLogSource();
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "B" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry(Core.Columns.Minimum) { RawContent = "B" });
			var filteredLogFile = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null);
			_taskScheduler.RunOnce();

			filteredLogFile.GetProperty(Core.Properties.LogEntryCount).Should().Be(2);
			var lineNumbers = filteredLogFile.GetColumn(new LogLineIndex[] {1, 0}, Core.Columns.OriginalLineNumber);
			lineNumbers[0].Should().Be(4);
			lineNumbers[1].Should().Be(2);

			lineNumbers = filteredLogFile.GetColumn(new LogLineIndex[] {1}, Core.Columns.OriginalLineNumber);
			lineNumbers[0].Should().Be(4);
		}

		#endregion

		#region Delta Time

		[Test]
		[Description("Verifies that the first entry doesn't have delta time")]
		public void TestGetDeltaTime1()
		{
			var filter = new NoFilter();
			var source = new InMemoryLogSource();
			using (var logFile = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("", LevelFlags.Other, DateTime.MinValue);

				var deltas = logFile.GetColumn(new LogSourceSection(0, 1), Core.Columns.DeltaTime);
				deltas.Should().NotBeNull();
				deltas.Should().HaveCount(1);
				deltas[0].Should().BeNull();
			}
		}

		[Test]
		[Description("Verifies that the log file can return the time between two consecutive non-filtered events")]
		public void TestGetDeltaTime2()
		{
			var filter = new NoFilter();
			var source = new InMemoryLogSource();
			using (var logFile = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("", LevelFlags.Other, new DateTime(2017, 12, 11, 19, 34, 0));
				source.AddEntry("", LevelFlags.Other, new DateTime(2017, 12, 11, 19, 35, 0));
				_taskScheduler.RunOnce();

				var deltas = logFile.GetColumn(new LogSourceSection(0, 2), Core.Columns.DeltaTime);
				deltas.Should().NotBeNull();
				deltas.Should().HaveCount(2);
				deltas[0].Should().BeNull();
				deltas[1].Should().Be(TimeSpan.FromMinutes(1));
			}
		}

		[Test]
		public void TestGetDeltaTime3()
		{
			var filter = new LevelFilter(LevelFlags.Info);
			var source = new InMemoryLogSource();
			using (var logFile = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("", LevelFlags.Info, new DateTime(2017, 12, 11, 19, 34, 0));
				source.AddEntry("", LevelFlags.Debug, new DateTime(2017, 12, 11, 19, 35, 0));
				source.AddEntry("", LevelFlags.Info, new DateTime(2017, 12, 11, 19, 36, 0));
				_taskScheduler.RunOnce();

				var deltas = logFile.GetColumn(new LogSourceSection(0, 2), Core.Columns.DeltaTime);
				deltas.Should().NotBeNull();
				deltas.Should().HaveCount(2);
				deltas[0].Should().BeNull();
				deltas[1].Should().Be(TimeSpan.FromMinutes(2), "because the delta time should be calculated based on events which match the filter");
			}
		}

		[Test]
		[Description("Verifies that accessing the delta time column by random indices works")]
		public void TestGetDeltaTime4()
		{
			var filter = new LevelFilter(LevelFlags.Info);
			var source = new InMemoryLogSource();
			using (var logFile = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("", LevelFlags.Info, new DateTime(2017, 12, 11, 19, 34, 0));
				source.AddEntry("", LevelFlags.Debug, new DateTime(2017, 12, 11, 19, 35, 0));
				source.AddEntry("", LevelFlags.Info, new DateTime(2017, 12, 11, 19, 36, 0));
				_taskScheduler.RunOnce();

				var deltas = logFile.GetColumn(new LogLineIndex[] {1}, Core.Columns.DeltaTime);
				deltas.Should().NotBeNull();
				deltas.Should().Equal(new object[]
				{
					TimeSpan.FromMinutes(2)
				}, "because the delta time should be calculated based on events which match the filter");
			}
		}

		#endregion

		#region Timestamp

		[Test]
		public void TestGetTimestamp1()
		{
			var filter = new NoFilter();
			var source = new InMemoryLogSource();
			using (var logFile = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				var timestamp = new DateTime(2017, 12, 11, 20, 46, 0);
				source.AddEntry("", LevelFlags.Other, timestamp);
				_taskScheduler.RunOnce();

				var timestamps = logFile.GetColumn(new LogSourceSection(0, 1), Core.Columns.Timestamp);
				timestamps.Should().NotBeNull();
				timestamps.Should().Equal(new object[] {timestamp});
			}
		}

		[Test]
		public void TestGetTimestamp2()
		{
			var filter = new LevelFilter(LevelFlags.Error);
			var source = new InMemoryLogSource();
			using (var logFile = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				var timestamp1 = new DateTime(2017, 12, 11, 20, 46, 0);
				source.AddEntry("", LevelFlags.Warning, timestamp1);

				var timestamp2 = new DateTime(2017, 12, 11, 20, 50, 0);
				source.AddEntry("", LevelFlags.Error, timestamp2);
				_taskScheduler.RunOnce();

				var timestamps = logFile.GetColumn(new LogSourceSection(0, 1), Core.Columns.Timestamp);
				timestamps.Should().NotBeNull();
				timestamps.Should().Equal(new object[] { timestamp2 }, "because the first entry doesn't match the filter and thus the timestamp of the 2nd one should've been returned");
			}
		}

		#endregion

		#endregion

		#region Well Known Properties

		[Test]
		public void TestPercentageProcessed()
		{
			var source = new Mock<ILogSource>();
			var sourceProperties = new PropertiesBufferList();
			sourceProperties.SetValue(Core.Properties.PercentageProcessed, Percentage.Zero);
			source.Setup(x => x.GetAllProperties(It.IsAny<IPropertiesBuffer>()))
			      .Callback((IPropertiesBuffer destination) => sourceProperties.CopyAllValuesTo(destination));
			source.Setup(x => x.Properties).Returns(() => sourceProperties.Properties);

			using (var file = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source.Object, null,
			                                      Filter.Create(null, true, LevelFlags.Debug)))
			{
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.Zero, "because the filtered log file hasn't consumed anything of its source (yet)");

				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.Zero, "because even though the filter doesn't have anything to do just yet - it's because its own source hasn't even started");

				sourceProperties.SetValue(Core.Properties.PercentageProcessed, Percentage.FromPercent(42));
				file.OnLogFileModified(source.Object, LogSourceModification.Appended(0, 84));
				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.FromPercent(42), "because now the filtered log file has processed 100% of the data the source sent it, but the original data source is still only at 42%");

				sourceProperties.SetValue(Core.Properties.PercentageProcessed, Percentage.HundredPercent);
				file.OnLogFileModified(source.Object, LogSourceModification.Appended(84, 200));
				_taskScheduler.RunOnce();
				file.GetProperty(Core.Properties.PercentageProcessed).Should().Be(Percentage.HundredPercent);
			}
		}

		#endregion

		protected override ILogSource CreateEmpty()
		{
			var filter = new NoFilter();
			var source = new InMemoryLogSource();
			return new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null);
		}

		protected override ILogSource CreateFromContent(IReadOnlyLogBuffer content)
		{
			var source = new InMemoryLogSource(content);
			var filter = new NoFilter();
			var filtered = new FilteredLogSource(_taskScheduler, TimeSpan.Zero, source, filter, null);
			_taskScheduler.RunOnce();
			return filtered;
		}
	}
}