using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class MergedLogFileTest
	{
		[Test]
		[Description("Verifies that creating a merged log file from two sources is possible")]
		public void TestCtor1()
		{
			var source1 = new Mock<ILogFile>();
			var source2 = new Mock<ILogFile>();

			MergedLogFile logFile = null;
			new Action(() => logFile = new MergedLogFile(source1.Object, source2.Object))
				.ShouldNotThrow();
			logFile.Should().NotBeNull();
		}

		[Test]
		[Description("Verifies that starting a merged log file causes it to add listeners with the source files")]
		public void TestStart1()
		{
			var source = new Mock<ILogFile>();
			var listeners = new List<Tuple<ILogFileListener, TimeSpan, int>>();
			source.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			      .Callback((ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount) => listeners.Add(Tuple.Create(listener, maximumWaitTime, maximumLineCount)));

			var logFile = new MergedLogFile(source.Object);
			var waitTime = TimeSpan.FromSeconds(1);
			new Action(() => logFile.Start(waitTime)).ShouldNotThrow();
			listeners.Count.Should().Be(1, "Because the merged file should have registered exactly 1 listener with the source file");
			listeners[0].Item1.Should().NotBeNull();
			listeners[0].Item2.Should().Be(waitTime);
			listeners[0].Item3.Should().BeGreaterThan(0);
		}

		[Test]
		[Description("Verifies that disposing a non-started logfile works")]
		public void TestDispose1()
		{
			var source1 = new Mock<ILogFile>();
			var source2 = new Mock<ILogFile>();

			var logFile = new MergedLogFile(source1.Object, source2.Object);
			new Action(logFile.Dispose).ShouldNotThrow();
		}

		[Test]
		[Description("Verifies that disposing a started logfile works")]
		public void TestDispose2()
		{
			var source1 = new Mock<ILogFile>();
			var source2 = new Mock<ILogFile>();

			var logFile = new MergedLogFile(source1.Object, source2.Object);
			logFile.Start(TimeSpan.FromMilliseconds(1));
			new Action(logFile.Dispose).ShouldNotThrow();
		}

		private static Mock<ILogFile> CreateLogFile(List<LogLine> lines)
		{
			var source = new Mock<ILogFile>();
			source.Setup(x => x.GetLine(It.IsAny<int>()))
			      .Returns((int index) => lines[index]);
			source.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
				   .Callback((LogFileSection section, LogLine[] data) => lines.CopyTo((int) section.Index, data, 0, section.Count));
			return source;
		}

		[Test]
		public void TestMerge1()
		{
			var source = new List<LogLine>();
			var source1 = CreateLogFile(source);
			var source2 = new Mock<ILogFile>();
			var merged = new MergedLogFile(source1.Object, source2.Object);
			var data = Listen(merged);
			merged.Start(TimeSpan.FromMilliseconds(1));

			source.Add(new LogLine(0, 0, "foobar", LevelFlags.Info, DateTime.Now));
			merged.OnLogFileModified(source1.Object, new LogFileSection(0, 1));
			merged.Wait();

			merged.Count.Should().Be(1);
			merged.DebugCount.Should().Be(0);
			merged.InfoCount.Should().Be(1);
			merged.WarningCount.Should().Be(0);
			merged.ErrorCount.Should().Be(0);
			merged.FatalCount.Should().Be(0);

			data.Should().Equal(source);
		}

		[Test]
		public void TestMerge2()
		{
			var source = new List<LogLine>();
			var source1 = CreateLogFile(source);
			var source2 = new Mock<ILogFile>();
			var merged = new MergedLogFile(source1.Object, source2.Object);
			var data = Listen(merged);
			merged.Start(TimeSpan.FromMilliseconds(1));

			source.Add(new LogLine(0, "a", LevelFlags.Info, DateTime.Now));
			source.Add(new LogLine(1, "b", LevelFlags.Debug, DateTime.Now));
			merged.OnLogFileModified(source1.Object, new LogFileSection(0, 2));
			merged.Wait();

			merged.Count.Should().Be(2);
			merged.DebugCount.Should().Be(1);
			merged.InfoCount.Should().Be(1);
			merged.WarningCount.Should().Be(0);
			merged.ErrorCount.Should().Be(0);
			merged.FatalCount.Should().Be(0);

			data.Should().Equal(source);
		}

		[Test]
		[Description("Verifies that the order of OnLogFileModified invocations is preserved when invoked from 2 data sources")]
		public void TestMerge3()
		{
			var source1 = new List<LogLine>();
			var logFile1 = CreateLogFile(source1);

			var source2 = new List<LogLine>();
			var logFile2 = CreateLogFile(source2);

			var merged = new MergedLogFile(logFile1.Object, logFile2.Object);
			var data = Listen(merged);
			merged.Start(TimeSpan.FromMilliseconds(1));

			var timestamp = DateTime.Now;
			source1.Add(new LogLine(0, "a", LevelFlags.Info, timestamp));
			merged.OnLogFileModified(logFile1.Object, new LogFileSection(0, 1));
			merged.Wait();

			source2.Add(new LogLine(1, "b", LevelFlags.Debug, timestamp));
			merged.OnLogFileModified(logFile2.Object, new LogFileSection(0, 1));
			merged.Wait();

			merged.Count.Should().Be(2);
			merged.DebugCount.Should().Be(1);
			merged.InfoCount.Should().Be(1);
			merged.WarningCount.Should().Be(0);
			merged.ErrorCount.Should().Be(0);
			merged.FatalCount.Should().Be(0);

			data.Should().Equal(new object[]{source1[0], source2[0]});
		}

		[Test]
		[Description("Verifies that log lines without timestamp are ignored")]
		public void TestMerge4()
		{
			var source = new List<LogLine>();
			var source1 = CreateLogFile(source);
			var source2 = new Mock<ILogFile>();
			var merged = new MergedLogFile(source1.Object, source2.Object);
			var data = Listen(merged);
			merged.Start(TimeSpan.FromMilliseconds(1));

			source.Add(new LogLine(0, "a", LevelFlags.Warning, DateTime.Now));
			source.Add(new LogLine(1, "b", LevelFlags.Info));
			source.Add(new LogLine(2, "c", LevelFlags.Error, DateTime.Now));
			merged.OnLogFileModified(source1.Object, new LogFileSection(0, 3));
			merged.Wait();

			merged.Count.Should().Be(2);
			merged.DebugCount.Should().Be(0);
			merged.InfoCount.Should().Be(0);
			merged.WarningCount.Should().Be(1);
			merged.ErrorCount.Should().Be(1);
			merged.FatalCount.Should().Be(0);

			data.Should().Equal(new object[] {source[0], source[2]});
		}

		[Test]
		[Description("Verifies that log messages from different sources are ordered correctly, even when arring out of order")]
		public void TestMerge5()
		{
			var source1 = new List<LogLine>();
			var logFile1 = CreateLogFile(source1);

			var source2 = new List<LogLine>();
			var logFile2 = CreateLogFile(source2);

			var merged = new MergedLogFile(logFile1.Object, logFile2.Object);
			var data = Listen(merged);
			merged.Start(TimeSpan.FromMilliseconds(1));

			var later = new DateTime(2016, 2, 16);
			var earlier = new DateTime(2016, 2, 15);

			source1.Add(new LogLine(0, "a", LevelFlags.Warning, later));
			merged.OnLogFileModified(logFile1.Object, new LogFileSection(0, 1));
			merged.Wait();

			source2.Add(new LogLine(2, "c", LevelFlags.Error, earlier));
			merged.OnLogFileModified(logFile2.Object, new LogFileSection(0, 1));
			merged.Wait();

			merged.Count.Should().Be(2);
			merged.DebugCount.Should().Be(0);
			merged.InfoCount.Should().Be(0);
			merged.WarningCount.Should().Be(1);
			merged.ErrorCount.Should().Be(1);
			merged.FatalCount.Should().Be(0);

			data.Should().Equal(new object[] { source2[0], source1[0] });
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
							else if (section.InvalidateSection)
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
	}
}