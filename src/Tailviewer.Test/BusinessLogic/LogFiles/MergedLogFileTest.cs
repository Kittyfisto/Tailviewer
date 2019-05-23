using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class MergedLogFileTest
		: AbstractLogFileTest
	{
		private ManualTaskScheduler _taskScheduler;

		private static Mock<ILogFile> CreateLogFile(List<LogLine> lines)
		{
			var source = new Mock<ILogFile>();
			source.Setup(x => x.GetLine(It.IsAny<int>()))
			      .Returns((int index) => lines[index]);
			source.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
			      .Callback((LogFileSection section, LogLine[] data) => lines.CopyTo((int) section.Index, data, 0, section.Count));
			source.Setup(x => x.EndOfSourceReached).Returns(true);
			return source;
		}

		private static List<LogFileSection> ListenToChanges(ILogFile logFile, int maximumLineCount)
		{
			var changes = new List<LogFileSection>();
			var listener = new Mock<ILogFileListener>();
			listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
			        .Callback((ILogFile file, LogFileSection section) => { changes.Add(section); });
			logFile.AddListener(listener.Object, TimeSpan.Zero, maximumLineCount);
			return changes;
		}

		private static List<LogLine> Listen(ILogFile logFile)
		{
			var data = new List<LogLine>();
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
						        data.AddRange(file.GetSection(section));
					        }
				        });
			logFile.AddListener(listener.Object, TimeSpan.Zero, 1);
			return data;
		}

		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new ManualTaskScheduler();
		}

		[Test]
		[Description("Verifies that creating a merged log file from two sources is possible")]
		public void TestCtor1()
		{
			var source1 = new Mock<ILogFile>();
			var source2 = new Mock<ILogFile>();

			MergedLogFile logFile = null;
			new Action(() => logFile = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), source1.Object, source2.Object))
				.ShouldNotThrow();
			logFile.Should().NotBeNull();
		}

		[Test]
		[Description("Verifies that a merged log file can be created using the maximum number of supported sources")]
		public void TestCtor2()
		{
			var sources = Enumerable.Range(0, LogLineSourceId.MaxSources)
				.Select(unused => new Mock<ILogFile>().Object).ToArray();
			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), sources);
			logFile.Sources.Should().Equal(sources);
		}

		[Test]
		[Description("Verifies that the ctor complains if too many sources are merged")]
		public void TestCtor3()
		{
			var sources = Enumerable.Range(0, LogLineSourceId.MaxSources+1)
				.Select(unused => new Mock<ILogFile>().Object).ToArray();

			new Action(() => new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), sources))
				.ShouldThrow<ArgumentException>("because the a merged log file can only support so many sources");
		}

		[Test]
		[Description("Verifies that disposing a logfile works")]
		public void TestDispose1()
		{
			var source1 = new Mock<ILogFile>();
			var source2 = new Mock<ILogFile>();

			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), source1.Object, source2.Object);
			new Action(logFile.Dispose).ShouldNotThrow();
		}

		[Test]
		[Description("Verifies that EndOfSourceReached tests if the inner sources are too")]
		public void TestEndOfSourceReached1()
		{
			var source = new Mock<ILogFile>();
			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), source.Object);
			source.Verify(x => x.EndOfSourceReached, Times.Never);
			logFile.EndOfSourceReached.Should().BeFalse("because the original source hasn't reached its end");
			source.Verify(x => x.EndOfSourceReached, Times.Once);
		}

		[Test]
		public void TestMerge1()
		{
			var source = new List<LogLine>();
			Mock<ILogFile> source1 = CreateLogFile(source);
			var source2 = new Mock<ILogFile>();
			source2.Setup(x => x.EndOfSourceReached).Returns(true);
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), source1.Object, source2.Object);
			IEnumerable<LogLine> data = Listen(merged);

			source.Add(new LogLine(0, 0, "foobar", LevelFlags.Info, DateTime.Now));
			merged.OnLogFileModified(source1.Object, new LogFileSection(0, 1));

			_taskScheduler.RunOnce();
			merged.Count.Should().Be(1);
			data.Should().Equal(source);
		}

		[Test]
		public void TestMerge2()
		{
			var source = new List<LogLine>();
			Mock<ILogFile> source1 = CreateLogFile(source);
			var source2 = new Mock<ILogFile>();
			source2.Setup(x => x.EndOfSourceReached).Returns(true);
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), source1.Object, source2.Object);
			IEnumerable<LogLine> data = Listen(merged);

			source.Add(new LogLine(0, "a", LevelFlags.Info, DateTime.Now));
			source.Add(new LogLine(1, "b", LevelFlags.Debug, DateTime.Now));
			merged.OnLogFileModified(source1.Object, new LogFileSection(0, 2));

			_taskScheduler.RunOnce();
			merged.Count.Should().Be(2);
			data.Should().Equal(source);
		}

		[Test]
		[Description("Verifies that the order of OnLogFileModified invocations is preserved when invoked from 2 data sources")
		]
		public void TestMerge3()
		{
			var source0 = new List<LogLine>();
			Mock<ILogFile> logFile0 = CreateLogFile(source0);

			var source1 = new List<LogLine>();
			Mock<ILogFile> logFile1 = CreateLogFile(source1);

			var merged = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), logFile0.Object, logFile1.Object);
			var data = Listen(merged);

			DateTime timestamp = DateTime.Now;
			source0.Add(new LogLine(0, "a", LevelFlags.Info, timestamp));
			merged.OnLogFileModified(logFile0.Object, new LogFileSection(0, 1));
			_taskScheduler.RunOnce();
			merged.EndOfSourceReached.Should().BeTrue();

			source1.Add(new LogLine(1, "b", LevelFlags.Debug, timestamp));
			merged.OnLogFileModified(logFile1.Object, new LogFileSection(0, 1));

			_taskScheduler.RunOnce();
			merged.EndOfSourceReached.Should().BeTrue();
			merged.Count.Should().Be(2);
			data.Should().Equal(new object[]
			{
				new LogLine(new LogLineSourceId(0), source0[0]),
				new LogLine(new LogLineSourceId(1), source1[0])
			});
		}

		[Test]
		[Description("Verifies that log lines without timestamp are ignored")]
		public void TestMerge4()
		{
			var source1Lines = new List<LogLine>();
			Mock<ILogFile> source1 = CreateLogFile(source1Lines);
			var source2 = new Mock<ILogFile>();
			source2.Setup(x => x.EndOfSourceReached).Returns(true);
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), source1.Object, source2.Object);
			var data = Listen(merged);

			source1Lines.Add(new LogLine(0, "a", LevelFlags.Warning, DateTime.Now));
			source1Lines.Add(new LogLine(1, "b", LevelFlags.Info));
			source1Lines.Add(new LogLine(2, "c", LevelFlags.Error, DateTime.Now));
			merged.OnLogFileModified(source1.Object, new LogFileSection(0, 3));

			_taskScheduler.RunOnce();
			merged.Count.Should().Be(2);
			data.Should().Equal(new LogLine(0, 0, new LogLineSourceId(0), source1Lines[0]),
								new LogLine(1, 1, new LogLineSourceId(0), source1Lines[2]));
		}

		[Test]
		[Description("Verifies that log messages from different sources are ordered correctly, even when arring out of order")]
		public void TestMerge5()
		{
			var source0 = new List<LogLine>();
			Mock<ILogFile> logFile1 = CreateLogFile(source0);

			var source1 = new List<LogLine>();
			Mock<ILogFile> logFile2 = CreateLogFile(source1);

			var merged = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), logFile1.Object, logFile2.Object);
			var data = Listen(merged);

			var later = new DateTime(2016, 2, 16);
			var earlier = new DateTime(2016, 2, 15);

			source0.Add(new LogLine(0, "a", LevelFlags.Warning, later));
			merged.OnLogFileModified(logFile1.Object, new LogFileSection(0, 1));

			_taskScheduler.RunOnce();
			merged.EndOfSourceReached.Should().BeTrue();

			source1.Add(new LogLine(0, "c", LevelFlags.Error, earlier));
			merged.OnLogFileModified(logFile2.Object, new LogFileSection(0, 1));

			_taskScheduler.RunOnce();
			merged.EndOfSourceReached.Should().BeTrue();
			merged.Count.Should().Be(2);
			data.Should().Equal(new object[]
				{
					new LogLine(0, 0, new LogLineSourceId(1), source1[0]),
					new LogLine(1, 1, new LogLineSourceId(0), source0[0])
				});
		}

		[Test]
		[Description(
			"Verifies that Reset() events from an always empty data source do not result in reset events from the merged log file"
			)]
		public void TestMerge6()
		{
			var source0 = new List<LogLine>();
			Mock<ILogFile> logFile0 = CreateLogFile(source0);

			var source1 = new List<LogLine>();
			DateTime timestamp = DateTime.Now;
			source1.Add(new LogLine(0, 0, "Hello World", LevelFlags.Info, timestamp));
			Mock<ILogFile> logFile1 = CreateLogFile(source1);

			var merged = new MergedLogFile(_taskScheduler, TimeSpan.FromMilliseconds(1), logFile0.Object, logFile1.Object);
			var data = Listen(merged);
			var changes = ListenToChanges(merged, 1);

			merged.OnLogFileModified(logFile0.Object, LogFileSection.Reset);
			merged.OnLogFileModified(logFile0.Object, LogFileSection.Reset);
			merged.OnLogFileModified(logFile0.Object, LogFileSection.Reset);
			merged.OnLogFileModified(logFile0.Object, LogFileSection.Reset);
			merged.OnLogFileModified(logFile0.Object, LogFileSection.Reset);
			merged.OnLogFileModified(logFile0.Object, LogFileSection.Reset);
			merged.OnLogFileModified(logFile1.Object, LogFileSection.Reset);
			merged.OnLogFileModified(logFile1.Object, new LogFileSection(0, 1));

			_taskScheduler.RunOnce();
			merged.EndOfSourceReached.Should().BeTrue();

			data.Should().Equal(new LogLine(new LogLineSourceId(1), source1[0]));

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
			merged.Count.Should().Be(4);
			merged.GetLine(0).Should().Be(new LogLine(0, 0, source1Id, "Foo", LevelFlags.Info, t1));
			merged.GetLine(1).Should().Be(new LogLine(1, 1, source2Id, "Hello,", LevelFlags.Debug, t2));
			merged.GetLine(2).Should().Be(new LogLine(2, 1, source2Id, "World!", LevelFlags.Debug, t2));
			merged.GetLine(3).Should().Be(new LogLine(3, 2, source1Id, "bar", LevelFlags.Warning, t3));
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
			merged.Count.Should().Be(4);
			merged.GetLine(0).Should().Be(new LogLine(0, 0, source1Id, "Foo", LevelFlags.Info, t1));
			merged.GetLine(1).Should().Be(new LogLine(1, 1, source2Id, "Hello,", LevelFlags.Debug, t2));
			merged.GetLine(2).Should().Be(new LogLine(2, 1, source2Id, "World!", LevelFlags.Debug, t2));
			merged.GetLine(3).Should().Be(new LogLine(3, 2, source1Id, "bar", LevelFlags.Warning, t3));
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

			merged.Count.Should().Be(4);
			merged.GetLine(0).Should().Be(new LogLine(0, 0, source1Id, "Foo", LevelFlags.Info, t1));
			merged.GetLine(1).Should().Be(new LogLine(1, 1, source2Id, "Hello,", LevelFlags.Debug, t2));
			merged.GetLine(2).Should().Be(new LogLine(2, 1, source2Id, "World!", LevelFlags.Debug, t2));
			merged.GetLine(3).Should().Be(new LogLine(3, 2, source1Id, "bar", LevelFlags.Warning, t3));
		}

		[Test]
		public void TestMergeMultiline4()
		{
			var source = new InMemoryLogFile();
			var merged = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source);

			source.AddMultilineEntry(LevelFlags.None, new DateTime(2017, 12, 3, 11, 59, 30), new []
			{
				"2017-12-03 11:59:30 Hello, ",
				"World!"
			});
			_taskScheduler.RunOnce();

			merged.Count.Should().Be(2);

			var entries = merged.GetEntries(new[] { new LogLineIndex(0), new LogLineIndex(1) },
				new ILogFileColumn[]
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
		[Ignore("Not yet implemented")]
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
			merged.Count.Should().Be(sourceCount, "because every source added one line");
			for (byte i = 0; i < sourceCount; ++i)
			{
				merged.GetLine(i).Should().Be(new LogLine(i, i, new LogLineSourceId(i),
				                                          i.ToString(), LevelFlags.Info,
				                                          start + TimeSpan.FromSeconds(i)));
			}

			// But then it should also have fired as few changes as possible!
			changes.Should().Equal(new object[]
			{
				LogFileSection.Reset,
				new LogFileSection(0, sourceCount)
			});
		}

		[Test]
		[Ignore("Not yet implemented")]
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
			merged.Count.Should().Be(sourceCount, "because every source added one line");
			for (int i = 0; i < sourceCount; ++i)
			{
				int idx = sourceCount - i - 1;
				merged.GetLine(i).Should().Be(new LogLine(i, i, new LogLineSourceId((byte)idx),
				                                          idx.ToString(), LevelFlags.Info,
				                                          end - TimeSpan.FromSeconds(idx)));
			}

			// But then it should also have fired as few changes as possible!
			changes.Should().Equal(new object[]
			{
				LogFileSection.Reset,
				new LogFileSection(0, sourceCount)
			});
		}

		[Test]
		[Description("Verifies that starting a merged log file causes it to add listeners with the source files")]
		public void TestStart1()
		{
			var source = new Mock<ILogFile>();
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
			source1.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 20, 23, 1, 0)});

			var source2 = new InMemoryLogFile();
			source1.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 20, 23, 0, 0)});

			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);
			_taskScheduler.RunOnce();
			logFile.Count.Should().Be(2);

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
			source1.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 20, 23, 1, 0)});

			var source2 = new InMemoryLogFile();
			source1.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 20, 23, 0, 0)});

			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);
			_taskScheduler.RunOnce();
			logFile.Count.Should().Be(2);

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
			source1.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 20, 23, 1, 0)});

			var source2 = new InMemoryLogFile();
			source1.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 20, 23, 0, 0)});

			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);
			_taskScheduler.RunOnce();
			logFile.Count.Should().Be(2);

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
			source1.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 20, 23, 1, 0)});

			var source2 = new InMemoryLogFile();
			source1.Add(new LogEntry2 {Timestamp = new DateTime(2017, 12, 20, 23, 0, 0)});

			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source1, source2);
			_taskScheduler.RunOnce();
			logFile.Count.Should().Be(2);

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

			source.AddEntry("", LevelFlags.None, new DateTime(2017, 12, 14, 23, 27, 0));
			source.AddEntry("", LevelFlags.None, new DateTime(2017, 12, 14, 23, 28, 23));
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
				buffer[offset + i].Should().Be(source.GetLine(i).Timestamp);
			}
		}

		[Test]
		[Description("Verifies that column values can be retrieved by indices")]
		public void TestGetTimestampsOneSource2([Range(0, 3)] int offset)
		{
			var source = new InMemoryLogFile();
			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source);

			source.AddEntry("", LevelFlags.None, new DateTime(2017, 12, 14, 23, 27, 0));
			source.AddEntry("", LevelFlags.None, new DateTime(2017, 12, 14, 23, 28, 23));
			source.AddEntry("", LevelFlags.None, new DateTime(2017, 12, 14, 23, 29, 1));
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
			buffer[offset + 0].Should().Be(source.GetLine(2).Timestamp);
			buffer[offset + 1].Should().Be(source.GetLine(1).Timestamp);
		}

		#endregion

		#region Delta Time

		[Test]
		public void TestGetDeltaTimesOneSource1()
		{
			var source = new InMemoryLogFile();
			var logFile = new MergedLogFile(_taskScheduler, TimeSpan.Zero, source);

			source.AddEntry("", LevelFlags.None, new DateTime(2017, 12, 14, 23, 27, 0));
			source.AddEntry("", LevelFlags.None, new DateTime(2017, 12, 14, 23, 28, 23));
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

			source.AddEntry("", LevelFlags.None, new DateTime(2017, 12, 14, 23, 27, 0));
			source.AddEntry("", LevelFlags.None, new DateTime(2017, 12, 14, 23, 28, 23));
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