using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Filters;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class FilteredLogFileTest
		: AbstractLogFileTest
	{
		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new ManualTaskScheduler();
			_entries = new List<LogLine>();
			_logFile = new Mock<ILogFile>();
			_logFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
			        .Callback(
				        (LogFileSection section, LogLine[] entries) =>
				        _entries.CopyTo((int) section.Index, entries, 0, section.Count));
			_logFile.Setup(x => x.GetLine(It.IsAny<int>())).Returns((int index) => _entries[index]);
			_logFile.Setup(x => x.Count).Returns(() => _entries.Count);
			_logFile.Setup(x => x.EndOfSourceReached).Returns(true);

			_sections = new List<LogFileSection>();
			_listener = new Mock<ILogFileListener>();
			_listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
			         .Callback((ILogFile l, LogFileSection s) => _sections.Add(s));
		}

		private Mock<ILogFile> _logFile;
		private List<LogLine> _entries;
		private List<LogFileSection> _sections;
		private Mock<ILogFileListener> _listener;
		private ManualTaskScheduler _taskScheduler;

		[Test]
		public void TestEndOfSourceReached1()
		{
			_logFile.Setup(x => x.EndOfSourceReached).Returns(false);

			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create(null, true, LevelFlags.Debug)))
			{
				file.EndOfSourceReached.Should().BeFalse("because the filtered log file hasn't even inspected the source yet");

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeFalse("because the source isn't at its own end yet");

				_logFile.Setup(x => x.EndOfSourceReached).Returns(true);
				file.EndOfSourceReached.Should().BeFalse("because the filtered log file hasn't processed the latest changes to the source yet");

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue("because the source is finished and the filtered log file has processed all changes from the source");
			}
		}

		[Test]
		public void TestConstruction()
		{
			_logFile.Setup(x => x.Count).Returns(2);
			_logFile.Setup(x => x.Progress).Returns(1);
			_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
			_entries.Add(new LogLine(1, 0, "DEBUG: Yikes", LevelFlags.None));

			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null,
				Filter.Create(null, true, LevelFlags.Debug)))
			{
				file.Progress.Should().Be(0, "because the filtered log file hasn't consumed anything of its source (yet)");

				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

				_taskScheduler.RunOnce();
				file.Progress.Should().Be(1, "because the filtered log file has consumed the entire source");
			}
		}

		[Test]
		[Description("Verifies that the filtered log file correctly listens to a reset event")]
		public void TestClear()
		{
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create(null, true, LevelFlags.Debug)))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "DEBUG: Yikes", LevelFlags.None));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

				_taskScheduler.RunOnce();

				file.EndOfSourceReached.Should().BeTrue();
				file.Count.Should().Be(2);

				_entries.Clear();
				file.OnLogFileModified(_logFile.Object, LogFileSection.Reset);

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();
				file.Count.Should().Be(0);
			}
		}

		[Test]
		public void TestEmptyLogFile1()
		{
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create("Test", true, LevelFlags.All)))
			{
				_taskScheduler.RunOnce();

				file.EndOfSourceReached.Should().BeTrue();
				file.Count.Should().Be(0);
				file.GetLogLineIndexOfOriginalLineIndex(new LogLineIndex(0)).Should().Be(LogLineIndex.Invalid);
			}
		}

		[Test]
		public void TestEntryLevelNone()
		{
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create("ello", true, LevelFlags.All)))
			{
				_entries.Add(new LogLine(0, "Hello world!", LevelFlags.None));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 1));

				_taskScheduler.RunOnce();

				file.EndOfSourceReached.Should().BeTrue();
				file.Count.Should().Be(1);
				var section = file.GetSection(new LogFileSection(0, 1));
				section.Should().HaveCount(1);
				section[0].LineIndex.Should().Be(0);
				section[0].OriginalLineIndex.Should().Be(0);
				section[0].LogEntryIndex.Should().Be(0);
				section[0].Message.Should().Be("Hello world!");
				section[0].Level.Should().Be(LevelFlags.None);
			}
		}

		[Test]
		public void TestInvalidate1()
		{
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create(null, true, LevelFlags.Info)))
			{
				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();

				_entries.AddRange(new[]
					{
						new LogLine(0, 0, "A", LevelFlags.Info),
						new LogLine(1, 1, "B", LevelFlags.Info),
						new LogLine(2, 2, "C", LevelFlags.Info),
						new LogLine(3, 3, "D", LevelFlags.Info)
					});

				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 4));
				file.OnLogFileModified(_logFile.Object, LogFileSection.Invalidate(2, 2));

				_taskScheduler.RunOnce();

				file.EndOfSourceReached.Should().BeTrue();
				file.Count.Should().Be(2, "because we've invalidated the last 2 out of 4 lines");
			}
		}

		[Test]
		public void TestInvalidate2()
		{
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create(null, true, LevelFlags.Info)))
			{
				file.AddListener(_listener.Object, TimeSpan.Zero, 1);

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();

				_entries.AddRange(new[]
					{
						new LogLine(0, 0, "A", LevelFlags.Info),
						new LogLine(1, 1, "B", LevelFlags.Info),
						new LogLine(2, 2, "C", LevelFlags.Info),
						new LogLine(3, 3, "D", LevelFlags.Info)
					});
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 4));

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();
				file.Count.Should().Be(4);

				file.OnLogFileModified(_logFile.Object, LogFileSection.Invalidate(2, 2));

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();
				file.Count.Should().Be(2);

				_sections.Should().Equal(new[]
					{
						LogFileSection.Reset,
						new LogFileSection(0, 1),
						new LogFileSection(1, 1),
						new LogFileSection(2, 1),
						new LogFileSection(3, 1),
						LogFileSection.Invalidate(2, 2)
					});
			}
		}

		[Test]
		[Description(
			"Verifies that the FilteredLogFile won't get stuck in an endless loop when an Invalidate() follows a multiline log entry"
			)]
		public void TestInvalidate3()
		{
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create(null, true, LevelFlags.Info)))
			{
				file.AddListener(_listener.Object, TimeSpan.Zero, 1);

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();

				_entries.AddRange(new[]
					{
						new LogLine(0, 0, "A", LevelFlags.Info),
						new LogLine(1, 0, "B", LevelFlags.Info),
						new LogLine(2, 0, "C", LevelFlags.Info),
						new LogLine(3, 0, "D", LevelFlags.Info)
					});
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 4));

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();

				file.OnLogFileModified(_logFile.Object, LogFileSection.Invalidate(2, 2));

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue("Because the filtered log file should be finished");
				file.Count.Should().Be(2);

				_sections.Should().Equal(new[]
					{
						LogFileSection.Reset,
						new LogFileSection(0, 1),
						new LogFileSection(1, 1),
						new LogFileSection(2, 1),
						new LogFileSection(3, 1),
						LogFileSection.Invalidate(2, 2)
					});
			}
		}

		[Test]
		[Description(
			"Verifies that listeners are notified eventually, even when the # of filtered entries is less than the minimum batch size"
			)]
		public void TestListener()
		{
			_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
			_entries.Add(new LogLine(1, 0, "Yikes", LevelFlags.None));
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create("yikes", true, LevelFlags.All)))
			{
				var sections = new List<LogFileSection>();
				var listener = new Mock<ILogFileListener>();

				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
				        .Callback((ILogFile l, LogFileSection s) => sections.Add(s));
				// We deliberately set the batchSize to be greater than the amount of entries that will be matched.
				// If the FilteredLogFile is implemented correctly, then it will continously notifiy the listener until
				// the maximum wait time is elapsed.
				const int batchSize = 10;
				file.AddListener(listener.Object, TimeSpan.FromMilliseconds(100), batchSize);
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();

				file.Count.Should().Be(2);
				sections.Should().Equal(new[]
					{
						LogFileSection.Reset,
						new LogFileSection(0, 2)
					});
			}
		}

		[Test]
		[Description("Verifies that all lines belonging to an entry pass the filter, even though only one line passes it")]
		public void TestMultiLineLogEntry1()
		{
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create("Test", true, LevelFlags.All)))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "Yikes", LevelFlags.None));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();

				file.Count.Should().Be(2);
				file.GetSection(new LogFileSection(0, 2))
				    .Should().Equal(new[]
					    {
						    new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug),
						    new LogLine(1, 0, "Yikes", LevelFlags.None)
					    });
			}
		}

		[Test]
		[Description(
			"Verifies that all lines belonging to an entry pass the filter, even though only the second line passes it")]
		public void TestMultiLineLogEntry2()
		{
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create("yikes", true, LevelFlags.All)))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "Yikes", LevelFlags.None));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();

				file.Count.Should().Be(2);
				file.GetSection(new LogFileSection(0, 2))
				    .Should().Equal(new[]
					    {
						    new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug),
						    new LogLine(1, 0, "Yikes", LevelFlags.None)
					    });
			}
		}

		[Test]
		[Description("Verifies that the filtered log file repeatedly calls the listener when the source has been fully read")]
		public void TestWait()
		{
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create(null, true, LevelFlags.Debug)))
			{
				var sections = new List<LogFileSection>();
				var listener = new Mock<ILogFileListener>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
				        .Callback((ILogFile logFile, LogFileSection section) => sections.Add(section));
				file.AddListener(listener.Object, TimeSpan.FromMilliseconds(100), 3);

				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "DEBUG: Yikes", LevelFlags.None));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();
				sections.Should().Equal(new object[]
					{
						LogFileSection.Reset,
						new LogFileSection(new LogLineIndex(0), 2)
					});
			}
		}

		[Test]
		[Description("Verifies that filtered log entries present the correct index from the view of the filtered file")]
		public void TestGetSection1()
		{
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create("yikes", true, LevelFlags.All)))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 1, "Yikes", LevelFlags.Info));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();

				var section = file.GetSection(new LogFileSection(0, 1));
				section.Should().NotBeNull();
				section.Length.Should().Be(1);
				section[0].LineIndex.Should().Be(0, "because the filtered log file only represents a file with one line, thus the only entry should have an index of 0, not 1, which is the original index");
				section[0].OriginalLineIndex.Should().Be(1, "because the given line is the second line in the source file");
				section[0].Message.Should().Be("Yikes");
				section[0].Level.Should().Be(LevelFlags.Info);

				var line = file.GetLine(0);
				line.LineIndex.Should().Be(0, "because the filtered log file only represents a file with one line, thus the only entry should have an index of 0, not 1, which is the original index");
				line.OriginalLineIndex.Should().Be(1, "because the given line is the second line in the source file");
				line.Message.Should().Be("Yikes");
				line.Level.Should().Be(LevelFlags.Info);
			}
		}

		[Test]
		[Description("Verifies that filtered log entries present the correct index from the view of the filtered file")]
		public void TestGetLine1()
		{
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, null, Filter.Create("yikes", true, LevelFlags.All)))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 1, "Yikes", LevelFlags.None));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 2));

				_taskScheduler.RunOnce();
				file.EndOfSourceReached.Should().BeTrue();

				var line = file.GetLine(0);
				line.LineIndex.Should().Be(0, "because the filtered log file only represents a file with one line, thus the only entry should have an index of 0, not 1, which is the original index");
				line.Message.Should().Be("Yikes");
			}
		}

		[Test]
		[Description("Verifies that the log file queries the LogLineFilter for each added entry")]
		public void TestSingleLineFilter1()
		{
			var filter = new Mock<ILogLineFilter>();
			filter.Setup(x => x.PassesFilter(It.IsAny<LogLine>())).Returns(true);
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, filter.Object, null))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 1));
				_taskScheduler.RunOnce();

				filter.Verify(x => x.PassesFilter(It.Is<LogLine>(y => Equals(y, _entries[0]))), Times.AtLeastOnce,
					"because the log file should've used our filter at least once to determine if the given log line should've been added");

				file.Count.Should().Be(1, "because the filter should've passed the only log line");
			}
		}

		[Test]
		[Description("Verifies that the log file honors the result of the log line filter")]
		public void TestSingleLineFilter2()
		{
			var filter = new Mock<ILogLineFilter>();
			filter.Setup(x => x.PassesFilter(It.IsAny<LogLine>())).Returns(false);
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, filter.Object, null))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 1));
				_taskScheduler.RunOnce();

				file.Count.Should().Be(0, "because the log line filter didn't pass the added line");

				_entries.Add(new LogLine(1, 0, "INFO: Something mundane", LevelFlags.Info));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(1, 0));
				_taskScheduler.RunOnce();
				file.Count.Should().Be(0, "because the log line filter didn't pass the added line");
			}
		}

		[Test]
		[Description("Verifies that the log line filter is used per log line")]
		public void TestSingleLineFilter3()
		{
			var filter = new EmptyLogLineFilter();
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, filter, null))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 0, "More stuff", LevelFlags.Debug));
				_entries.Add(new LogLine(2, 0, "", LevelFlags.Debug));
				_entries.Add(new LogLine(3, 0, "And even more stuff", LevelFlags.Debug));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 4));
				_taskScheduler.RunOnce();

				file.Count.Should().Be(3, "because the log file should've filtered out the one log line that is empty");
			}
		}

		[Test]
		[Description("Verifies that the filter adjusts log entry indices to be consecutive once again")]
		public void TestSingleLineFilter4()
		{
			var filter = new EmptyLogLineFilter();
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, filter, null))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 1, "More stuff", LevelFlags.Debug));
				_entries.Add(new LogLine(2, 2, "", LevelFlags.Debug));
				_entries.Add(new LogLine(3, 3, "And even more stuff", LevelFlags.Debug));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 4));
				_taskScheduler.RunOnce();

				file.Count.Should().Be(3, "because the log file should've filtered out the one log line that is empty");

				const string reason = "because log entry indices are supposed to be consecutive for a data source";
				file.GetLine(0).LogEntryIndex.Should().Be(0, reason);
				file.GetLine(1).LogEntryIndex.Should().Be(1, reason);
				file.GetLine(2).LogEntryIndex.Should().Be(2, reason);
			}
		}

		[Test]
		[Description("Verifies that the filter adjusts log entry indices to be consecutive once again")]
		public void TestSingleLineFilter5()
		{
			var filter = new EmptyLogLineFilter();
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, filter, null))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 1, "More stuff", LevelFlags.Debug));
				_entries.Add(new LogLine(2, 2, "", LevelFlags.Debug));
				_entries.Add(new LogLine(3, 3, "And even more stuff", LevelFlags.Debug));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 4));
				_taskScheduler.RunOnce();

				file.OnLogFileModified(_logFile.Object, LogFileSection.Reset);
				_taskScheduler.RunOnce();

				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 1));
				_taskScheduler.RunOnce();

				file.Count.Should().Be(1, "because only one line remains in the source");
				file.GetLine(0).LogEntryIndex.Should().Be(0, "because log entry indices should always start at 0");
			}
		}

		[Test]
		[Description("Verifies that the filter adjusts log entry indices to be consecutive once again")]
		public void TestSingleLineFilter6()
		{
			var filter = new EmptyLogLineFilter();
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, filter, null))
			{
				_entries.Add(new LogLine(0, 0, "DEBUG: This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 1, "More stuff", LevelFlags.Debug));
				_entries.Add(new LogLine(2, 2, "", LevelFlags.Debug));
				_entries.Add(new LogLine(3, 3, "And even more stuff", LevelFlags.Debug));
				_entries.Add(new LogLine(4, 4, "And even more stuff", LevelFlags.Debug));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 5));
				_taskScheduler.RunOnce();

				file.OnLogFileModified(_logFile.Object, LogFileSection.Invalidate(3, 2));
				_taskScheduler.RunOnce();

				file.OnLogFileModified(_logFile.Object, new LogFileSection(3, 2));
				_taskScheduler.RunOnce();
				
				file.Count.Should().Be(4, "because the source represents 4 lines (of which the last two changed over its lifetime)");

				const string reason = "because log entry indices are supposed to be consecutive for a data source";
				file.GetLine(0).LogEntryIndex.Should().Be(0, reason);
				file.GetLine(1).LogEntryIndex.Should().Be(1, reason);
				file.GetLine(2).LogEntryIndex.Should().Be(2, reason);
				file.GetLine(3).LogEntryIndex.Should().Be(3, reason);
			}
		}

		[Test]
		[Description("Verifies that the log file unregisters itself as a listener from the source upon being removed")]
		public void TestDispose1()
		{
			var filter = new EmptyLogLineFilter();
			FilteredLogFile file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, filter, null);
			_logFile.Verify(x => x.AddListener(It.Is<ILogFileListener>(y => Equals(y, file)),
				It.IsAny<TimeSpan>(),
				It.IsAny<int>()), Times.Once, "because the filtered log file should register itself as a listener with its source");

			new Action(() => file.Dispose()).ShouldNotThrow("because Dispose() must always succeed");

			_logFile.Verify(x => x.RemoveListener(It.Is<ILogFileListener>(y => Equals(y, file))), Times.Once,
				"because the filtered log file should unregister itself as a listener from its source when being disposed of");
		}

		[Test]
		public void TestGetLogLineIndexOfOriginalLineIndex1()
		{
			var filter = new LevelFilter(LevelFlags.Info);
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, filter, null))
			{
				_entries.Add(new LogLine(0, 0, "This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 1, "This is a test", LevelFlags.Info));
				_entries.Add(new LogLine(2, 2, "This is a test", LevelFlags.Error));
				_entries.Add(new LogLine(3, 3, "This is a test", LevelFlags.Info));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 4));
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
			var filter = new LevelFilter(LevelFlags.Info);
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, filter, null))
			{
				_entries.Add(new LogLine(0, 0, "This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 1, "This is a test", LevelFlags.Info));
				_entries.Add(new LogLine(2, 2, "This is a test", LevelFlags.Error));
				_entries.Add(new LogLine(3, 3, "This is a test", LevelFlags.Info));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 4));
				_taskScheduler.RunOnce();

				var originalIndices = file.GetColumn(new LogFileSection(0, 2), LogFileColumns.OriginalIndex);
				originalIndices.Should().Equal(new LogLineIndex(1), new LogLineIndex(3));
			}
		}

		[Test]
		public void TestGetOriginalIndicesFrom4()
		{
			var filter = new LevelFilter(LevelFlags.Info);
			using (var file = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, _logFile.Object, filter, null))
			{
				_entries.Add(new LogLine(0, 0, "This is a test", LevelFlags.Debug));
				_entries.Add(new LogLine(1, 1, "This is a test", LevelFlags.Info));
				_entries.Add(new LogLine(2, 2, "This is a test", LevelFlags.Error));
				_entries.Add(new LogLine(3, 3, "This is a test", LevelFlags.Info));
				_entries.Add(new LogLine(4, 4, "This is a test", LevelFlags.Error));
				_entries.Add(new LogLine(5, 5, "This is a test", LevelFlags.Info));
				file.OnLogFileModified(_logFile.Object, new LogFileSection(0, 6));
				_taskScheduler.RunOnce();

				var originalIndices = file.GetColumn(new LogLineIndex[] {0, 2}, LogFileColumns.OriginalIndex);
				originalIndices.Should().Equal(new LogLineIndex(1), new LogLineIndex(5));
			}
		}

		#endregion

		#region Line Number

		[Test]
		public void TestGetLineNumbersBySection()
		{
			var filter = new SubstringFilter("B", true);
			var source = new InMemoryLogFile();
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "B" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "B" });
			var filteredLogFile = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null);
			_taskScheduler.RunOnce();

			filteredLogFile.Count.Should().Be(2);
			var lineNumbers = filteredLogFile.GetColumn(new LogFileSection(0, 2), LogFileColumns.LineNumber);
			lineNumbers[0].Should().Be(1);
			lineNumbers[1].Should().Be(2);
		}

		[Test]
		public void TestGetLineNumbersByIndices1()
		{
			var filter = new SubstringFilter("B", true);
			var source = new InMemoryLogFile();
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "B" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "B" });
			var filteredLogFile = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null);
			_taskScheduler.RunOnce();

			filteredLogFile.Count.Should().Be(2);
			var lineNumbers = filteredLogFile.GetColumn(new LogLineIndex[] {1, 0}, LogFileColumns.LineNumber);
			lineNumbers[0].Should().Be(2);
			lineNumbers[1].Should().Be(1);

			lineNumbers = filteredLogFile.GetColumn(new LogLineIndex[] {1}, LogFileColumns.LineNumber);
			lineNumbers[0].Should().Be(2);
		}

		#endregion

		#region Original Line Number

		[Test]
		public void TestGetOriginalLineNumbersBySection()
		{
			var filter = new SubstringFilter("B", true);
			var source = new InMemoryLogFile();
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "B" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "B" });
			var filteredLogFile = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null);
			_taskScheduler.RunOnce();

			filteredLogFile.Count.Should().Be(2);
			var lineNumbers = filteredLogFile.GetColumn(new LogFileSection(0, 2), LogFileColumns.OriginalLineNumber);
			lineNumbers[0].Should().Be(2);
			lineNumbers[1].Should().Be(4);
		}

		[Test]
		public void TestGetOriginalLineNumbersByIndices()
		{
			var filter = new SubstringFilter("B", true);
			var source = new InMemoryLogFile();
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "B" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "A" });
			source.Add(new LogEntry2(LogFileColumns.Minimum) { RawContent = "B" });
			var filteredLogFile = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null);
			_taskScheduler.RunOnce();

			filteredLogFile.Count.Should().Be(2);
			var lineNumbers = filteredLogFile.GetColumn(new LogLineIndex[] {1, 0}, LogFileColumns.OriginalLineNumber);
			lineNumbers[0].Should().Be(4);
			lineNumbers[1].Should().Be(2);

			lineNumbers = filteredLogFile.GetColumn(new LogLineIndex[] {1}, LogFileColumns.OriginalLineNumber);
			lineNumbers[0].Should().Be(4);
		}

		#endregion

		#region Delta Time

		[Test]
		[Description("Verifies that the first entry doesn't have delta time")]
		public void TestGetDeltaTime1()
		{
			var filter = new NoFilter();
			var source = new InMemoryLogFile();
			using (var logFile = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("", LevelFlags.None, DateTime.MinValue);

				var deltas = logFile.GetColumn(new LogFileSection(0, 1), LogFileColumns.DeltaTime);
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
			var source = new InMemoryLogFile();
			using (var logFile = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("", LevelFlags.None, new DateTime(2017, 12, 11, 19, 34, 0));
				source.AddEntry("", LevelFlags.None, new DateTime(2017, 12, 11, 19, 35, 0));
				_taskScheduler.RunOnce();

				var deltas = logFile.GetColumn(new LogFileSection(0, 2), LogFileColumns.DeltaTime);
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
			var source = new InMemoryLogFile();
			using (var logFile = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("", LevelFlags.Info, new DateTime(2017, 12, 11, 19, 34, 0));
				source.AddEntry("", LevelFlags.Debug, new DateTime(2017, 12, 11, 19, 35, 0));
				source.AddEntry("", LevelFlags.Info, new DateTime(2017, 12, 11, 19, 36, 0));
				_taskScheduler.RunOnce();

				var deltas = logFile.GetColumn(new LogFileSection(0, 2), LogFileColumns.DeltaTime);
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
			var source = new InMemoryLogFile();
			using (var logFile = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				source.AddEntry("", LevelFlags.Info, new DateTime(2017, 12, 11, 19, 34, 0));
				source.AddEntry("", LevelFlags.Debug, new DateTime(2017, 12, 11, 19, 35, 0));
				source.AddEntry("", LevelFlags.Info, new DateTime(2017, 12, 11, 19, 36, 0));
				_taskScheduler.RunOnce();

				var deltas = logFile.GetColumn(new LogLineIndex[] {1}, LogFileColumns.DeltaTime);
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
			var source = new InMemoryLogFile();
			using (var logFile = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				var timestamp = new DateTime(2017, 12, 11, 20, 46, 0);
				source.AddEntry("", LevelFlags.None, timestamp);
				_taskScheduler.RunOnce();

				var timestamps = logFile.GetColumn(new LogFileSection(0, 1), LogFileColumns.Timestamp);
				timestamps.Should().NotBeNull();
				timestamps.Should().Equal(new object[] {timestamp});
			}
		}

		[Test]
		public void TestGetTimestamp2()
		{
			var filter = new LevelFilter(LevelFlags.Error);
			var source = new InMemoryLogFile();
			using (var logFile = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null))
			{
				var timestamp1 = new DateTime(2017, 12, 11, 20, 46, 0);
				source.AddEntry("", LevelFlags.Warning, timestamp1);

				var timestamp2 = new DateTime(2017, 12, 11, 20, 50, 0);
				source.AddEntry("", LevelFlags.Error, timestamp2);
				_taskScheduler.RunOnce();

				var timestamps = logFile.GetColumn(new LogFileSection(0, 1), LogFileColumns.Timestamp);
				timestamps.Should().NotBeNull();
				timestamps.Should().Equal(new object[] { timestamp2 }, "because the first entry doesn't match the filter and thus the timestamp of the 2nd one should've been returned");
			}
		}

		#endregion

		#endregion

		protected override ILogFile CreateEmpty()
		{
			var filter = new NoFilter();
			var source = new InMemoryLogFile();
			return new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null);
		}

		protected override ILogFile CreateFromContent(IReadOnlyLogEntries content)
		{
			var source = new InMemoryLogFile(content);
			var filter = new NoFilter();
			var filtered = new FilteredLogFile(_taskScheduler, TimeSpan.Zero, source, filter, null);
			_taskScheduler.RunOnce();
			return filtered;
		}
	}
}