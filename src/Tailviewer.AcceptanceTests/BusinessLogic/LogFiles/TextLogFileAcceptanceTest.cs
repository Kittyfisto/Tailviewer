using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Test;
using log4net;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.LogFiles.Text;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class TextLogFileAcceptanceTest
	{
		public const string File1Mb_1Line = @"TestData\1Mb_1Line.txt";
		public const string File1Mb_2Lines = @"TestData\1Mb_2Lines.txt";
		public const string File2Mb = @"TestData\2Mb.txt";
		public const string File20Mb = @"TestData\20Mb.txt";
		public const string File2Lines = @"TestData\2Lines.txt";
		public const string File2Entries = @"TestData\2LogEntries.txt";
		public const string FileTestLive1 = @"TestData\TestLive1.txt";
		public const string FileTestLive2 = @"TestData\TestLive2.txt";
		public const string MultilineNoLogLevel1 = @"TestData\Multiline\Log1.txt";
		public const string MultilineNoLogLevel2 = @"TestData\Multiline\Log2.txt";

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private DefaultTaskScheduler _scheduler;

		[SetUp]
		public void SetUp()
		{
			_scheduler = new DefaultTaskScheduler();
		}

		[TearDown]
		public void TearDown()
		{
			_scheduler.Dispose();
		}

		private TextLogFile Create(string fileName)
		{
			var serviceContainer = new ServiceContainer();
			serviceContainer.RegisterInstance<ITaskScheduler>(_scheduler);
			serviceContainer.RegisterInstance<ILogFileFormatMatcher>(new SimpleLogFileFormatMatcher(LogFileFormats.GenericText));
			serviceContainer.RegisterInstance<ITextLogFileParserPlugin>(new SimpleTextLogFileParserPlugin());
			return new TextLogFile(serviceContainer, fileName);
		}

		[Test]
		public void TestClear1()
		{
			const string fname = "TestClear1.log";
			using (FileStream stream = File.OpenWrite(fname))
			using (var writer = new StreamWriter(stream))
			{
				stream.SetLength(0);
				writer.WriteLine("Test");
			}

			using (var logFile = Create(fname))
			{
				logFile.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);

				logFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(1);

				using (var stream = new FileStream(fname, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
				using (var writer = new StreamWriter(stream))
				{
					stream.SetLength(0);

					logFile.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(0);
					logFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(0);

					writer.WriteLine("Hello World!");
					writer.Flush();

					logFile.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);
					var entries = logFile.GetEntries();
					entries.Count.Should().Be(1);
					entries[0].Index.Should().Be(0);
					entries[0].RawContent.Should().Be("Hello World!");
					entries[0].LogLevel.Should().Be(LevelFlags.Other);
				}
			}
		}

		[Test]
		public void TestClear2()
		{
			const string fname = "TestClear2.log";
			using (FileStream stream = File.OpenWrite(fname))
			using (var writer = new StreamWriter(stream))
			{
				stream.SetLength(0);
				writer.WriteLine("Test");
			}

			using (var logFile = Create(fname))
			{
				var listener = new Mock<ILogFileListener>();
				var sections = new List<LogFileSection>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
				        .Callback((ILogFile log, LogFileSection section) => sections.Add(section));
				logFile.AddListener(listener.Object, TimeSpan.Zero, 2);

				logFile.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);

				using (var stream = new FileStream(fname, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
				{
					stream.SetLength(0);

					logFile.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(0);
					sections.Should().EndWith(LogFileSection.Reset);
				}
			}
		}

		[Test]
		public void TestDelete1()
		{
			const string fname = "TestDelete1.log";
			File.WriteAllText(fname, "Test");

			using (var logFile = Create(fname))
			{
				logFile.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);

				new Action(() =>
					{
						for (int i = 0; i < 10; ++i)
						{
							try
							{
								File.Delete(fname);
								return;
							}
							catch (IOException)
							{
							}
						}

						File.Delete(fname);
					}).Should().NotThrow();
			}
		}
		
		[Test]
		[Description("Verifies that creating a LogFile for a file that doesn't exist works")]
		public void TestDoesNotExist()
		{
			TextLogFile logFile = null;
			try
			{
				new Action(() => logFile = Create( "dadwdawdw")).Should().NotThrow();

				logFile.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				logFile.Property(x => x.GetValue(LogFileProperties.EmptyReason)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(ErrorFlags.SourceDoesNotExist);

				logFile.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.SourceDoesNotExist, "Because the specified file doesn't exist");
			}
			finally
			{
				if (logFile != null)
					logFile.Dispose();
			}
		}

		[Test]
		public void TestExists()
		{
			TextLogFile logFile = null;
			try
			{
				new Action(() => logFile = Create( File2Lines)).Should().NotThrow();

				logFile.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				logFile.Property(x => x.GetValue(LogFileProperties.EmptyReason)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(ErrorFlags.None);

				logFile.GetValue(LogFileProperties.EmptyReason).Should().Be(ErrorFlags.None, "Because the specified file does exist");
			}
			finally
			{
				if (logFile != null)
					logFile.Dispose();
			}
		}

		[Test]
		public void TestGetEntries1()
		{
			using (var file = Create( File20Mb))
			{
				file.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(15)).Be(Percentage.HundredPercent, "because we should be able to read the entire file in a few seconds");
				file.GetValue(LogFileProperties.LogEntryCount).Should().Be(165342);
				file.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

				var buffer = file.GetEntries(new LogFileSection(0, 10));


				buffer[0].Index.Should().Be(0);
				buffer[0].RawContent.Should()
				         .Be("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");
				buffer[0].LogLevel.Should().Be(LevelFlags.Info);
				buffer[0].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

				buffer[1].Index.Should().Be(1);
				buffer[1].RawContent.Should()
				         .Be("2015-10-07 19:50:58,998 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
				buffer[1].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[1].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 998));

				buffer[2].Index.Should().Be(2);
				buffer[2].RawContent.Should()
				         .Be("2015-10-07 19:50:59,013 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551613) 'SharpRemote.Heartbeat' implementing 'SharpRemote.IHeartbeat'");
				buffer[2].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[2].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 013));

				buffer[3].Index.Should().Be(3);
				buffer[3].RawContent.Should()
				         .Be("2015-10-07 19:50:59,062 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551614) 'SharpRemote.Latency' implementing 'SharpRemote.ILatency'");
				buffer[3].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[3].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 062));

				buffer[4].Index.Should().Be(4);
				buffer[4].RawContent.Should()
				         .Be("2015-10-07 19:50:59,067 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551615) 'SharpRemote.Hosting.SubjectHost' implementing 'SharpRemote.Hosting.ISubjectHost'");
				buffer[4].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[4].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 067));

				buffer[5].Index.Should().Be(5);
				buffer[5].RawContent.Should()
				         .Be("2015-10-07 19:50:59,081 [8092, 1] INFO  SharpRemote.SocketRemotingEndPointServer (null) - EndPoint '<Unnamed>' listening on 0.0.0.0:49152");
				buffer[5].LogLevel.Should().Be(LevelFlags.Info);
				buffer[5].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 081));

				buffer[6].Index.Should().Be(6);
				buffer[6].RawContent.Should()
				         .Be("2015-10-07 19:50:59,141 [8092, 6] DEBUG SharpRemote.SocketRemotingEndPointServer (null) - Incoming connection from '127.0.0.1:10348', starting handshake...");
				buffer[6].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[6].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 141));

				buffer[7].Index.Should().Be(7);
				buffer[7].RawContent.Should()
				         .Be("2015-10-07 19:50:59,171 [8092, 6] INFO  SharpRemote.AbstractIPSocketRemotingEndPoint (null) - <Unnamed>: Connected to 127.0.0.1:10348");
				buffer[7].LogLevel.Should().Be(LevelFlags.Info);
				buffer[7].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 171));

				buffer[8].Index.Should().Be(8);
				buffer[8].RawContent.Should()
				         .Be("2015-10-07 19:50:59,181 [8092, 10] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - 0.0.0.0:49152 to 127.0.0.1:10348: sending RPC #1 to 18446744073709551611.Beat");
				buffer[8].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[8].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 181));

				buffer[9].Index.Should().Be(9);
				buffer[9].RawContent.Should()
				         .Be("2015-10-07 19:50:59,182 [8092, 11] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - 0.0.0.0:49152 to 127.0.0.1:10348: sending RPC #2 to 18446744073709551612.Roundtrip");
				buffer[9].LogLevel.Should().Be(LevelFlags.Debug);
				buffer[9].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 182));
			}
		}

		[Test]
		public void TestLive1()
		{
			const string fname = "TestLive1.log";

			if (File.Exists(fname))
				File.Delete(fname);

			using (var logger = new FileLogger(fname))
			using (var logFile = Create(fname))
			{
				logFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(0);

				Log.Info("Test");

				logFile.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);
			}
		}

		[Test]
		public void TestLive2()
		{
			const string fname = "TestLive2.log";

			if (File.Exists(fname))
				File.Delete(fname);

			using (var logger = new FileLogger(fname))
			using (var logFile = Create(fname))
			{
				logFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(0);

				Log.Info("Hello");
				logFile.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);

				Log.Info("world!");
				logFile.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(2);
			}
		}

		[Test]
		[Description("Verifies that opening a log file before the file is created works and that its contents can be read")]
		public void TestOpenBeforeCreate()
		{
			TextLogFile logFile = null;
			try
			{
				string fileName = PathEx.GetTempFileName();
				if (File.Exists(fileName))
					File.Delete(fileName);

				new Action(() => logFile = Create(fileName)).Should().NotThrow();

				logFile.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				logFile.Property(x => x.GetValue(LogFileProperties.EmptyReason)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(ErrorFlags.SourceDoesNotExist,
				                                                           "Because the specified file doesn't exist");

				File.WriteAllText(fileName, "Hello World!");

				logFile.Property(x => x.GetValue(LogFileProperties.EmptyReason)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(ErrorFlags.None,
				                                                          "Because the file has been created now");
				logFile.Property(x => x.GetValue(LogFileProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1, "Because one line was written to the file");

				var entry = logFile.GetEntry(0);
				entry.Index.Should().Be(0);
				entry.LogEntryIndex.Should().Be(0);
				entry.RawContent.Should().Be("Hello World!");
				entry.LogLevel.Should().Be(LevelFlags.Other);
			}
			finally
			{
				if (logFile != null)
					logFile.Dispose();
			}
		}

		[Test]
		public void TestRead2Lines()
		{
			using (var file = Create(File2Lines))
			{
				var listener = new Mock<ILogFileListener>();
				var changes = new List<LogFileSection>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
				        .Callback((ILogFile logFile, LogFileSection section) => changes.Add(section));

				file.AddListener(listener.Object, TimeSpan.Zero, 1);

				changes.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(3);
				changes.Should().Equal(new[]
					{
						LogFileSection.Reset,
						new LogFileSection(0, 1),
						new LogFileSection(1, 1)
					});
			}
		}

		[Test]
		public void TestRead2LogEntries()
		{
			using (var file = Create( File2Entries))
			{
				var listener = new Mock<ILogFileListener>();
				var changes = new List<LogFileSection>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
				        .Callback((ILogFile logFile, LogFileSection section) => changes.Add(section));

				file.AddListener(listener.Object, TimeSpan.Zero, 1);

				changes.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(7);

				changes.Should().Equal(new[]
					{
						LogFileSection.Reset,
						new LogFileSection(0, 1),
						new LogFileSection(1, 1),
						new LogFileSection(2, 1),
						new LogFileSection(3, 1),
						new LogFileSection(4, 1),
						new LogFileSection(5, 1)
					});

				file.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

				var entries = file.GetEntries(new LogFileSection(0, 6));
				entries[0].Index.Should().Be(0);
				entries[0].LogEntryIndex.Should().Be(0);
				entries[0].LogLevel.Should().Be(LevelFlags.Info);
				entries[0].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));
				entries[0].RawContent.Should().Be("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");
				
				entries[1].Index.Should().Be(1);
				entries[1].LogEntryIndex.Should().Be(1);
				entries[1].LogLevel.Should().Be(LevelFlags.Other);
				entries[1].Timestamp.Should().Be(null);
				entries[1].RawContent.Should().Be("Foobar");
				
				entries[2].Index.Should().Be(2);
				entries[2].LogEntryIndex.Should().Be(2);
				entries[2].LogLevel.Should().Be(LevelFlags.Other);
				entries[2].Timestamp.Should().Be(null);
				entries[2].RawContent.Should().Be("Some more info");
				
				entries[3].Index.Should().Be(3);
				entries[3].LogEntryIndex.Should().Be(3);
				entries[3].LogLevel.Should().Be(LevelFlags.Debug);
				entries[3].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 998));
				entries[3].RawContent.Should().Be("2015-10-07 19:50:58,998 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
				
				entries[4].Index.Should().Be(4);
				entries[4].LogEntryIndex.Should().Be(4);
				entries[4].LogLevel.Should().Be(LevelFlags.Other);
				entries[4].Timestamp.Should().Be(null);
				entries[4].RawContent.Should().Be("Hey look at me");
				
				entries[5].Index.Should().Be(5);
				entries[5].LogEntryIndex.Should().Be(5);
				entries[5].LogLevel.Should().Be(LevelFlags.Other);
				entries[5].Timestamp.Should().Be(null);
				entries[5].RawContent.Should().Be("dwadawdadw");
			}
		}

		[Test]
		public void TestReadAll1()
		{
			using (var file = Create(File20Mb))
			{
				file.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(20)).Be(Percentage.HundredPercent);

				file.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

				var entries = file.GetEntries();
				entries.Count.Should().Be(165342);
				entries[0].Index.Should().Be(0);
				entries[0].LogLevel.Should().Be(LevelFlags.Info);
				entries[0].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));
				entries[0].RawContent.Should().Be("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");

				entries[entries.Count - 1].Index.Should().Be(165341);
				entries[entries.Count - 1].LogLevel.Should().Be(LevelFlags.Info);
				entries[entries.Count - 1].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 51, 42, 483));
				entries[entries.Count - 1].RawContent.Should()
				                          .Be("2015-10-07 19:51:42,483 [8092, 6] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Parent process terminated unexpectedly (exit code: -1), shutting down...");
			}
		}

		[Test]
		public void TestReadAll2()
		{
			using (var file = Create(File20Mb))
			{
				var listener = new Mock<ILogFileListener>();
				var sections = new List<LogFileSection>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
				        .Callback((ILogFile logFile, LogFileSection section) => sections.Add(section));

				file.AddListener(listener.Object, TimeSpan.Zero, 1);

				file.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(20)).Be(Percentage.HundredPercent);
				file.GetValue(LogFileProperties.LogEntryCount).Should().Be(165342);

				sections[0].Should().Equal(LogFileSection.Reset);
				for (int i = 1; i < sections.Count; ++i)
				{
					LogFileSection change = sections[i];
					change.Index.Should().Be((LogLineIndex) (i - 1));
					change.Count.Should().Be(1);
				}
			}
		}

		[Test]
		[Description("Verifies that the maximum number of characters for all lines is determined correctly")]
		public void TestReadAll3()
		{
			using (var file = Create(File20Mb))
			{
				file.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(20)).Be(Percentage.HundredPercent);

				file.GetValue(LogFileProperties.LogEntryCount).Should().Be(165342);
				file.GetValue(TextLogFileProperties.MaxCharactersInLine).Should().Be(218);
			}
		}

		[Test]
		public void TestTimestampFormat1()
		{
			using (var file = Create(@"TestData\Timestamps\yyyy-MM-dd HH_mm_ss_fff.txt"))
			{
				file.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetValue(LogFileProperties.LogEntryCount).Should().Be(1);

				var entry = file.GetEntry(0);
				entry.Timestamp.Should().Be(new DateTime(2017, 5, 10, 20, 40, 3, 143, DateTimeKind.Unspecified));
			}
		}

		[Test]
		public void TestTimestampFormat2()
		{
			using (var file = Create(@"TestData\Timestamps\yyyy-MM-dd HH_mm_ss.txt"))
			{
				file.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetValue(LogFileProperties.LogEntryCount).Should().Be(1);

				var entry = file.GetEntry(0);
				entry.Timestamp.Should().Be(new DateTime(2017, 5, 10, 20, 40, 3, DateTimeKind.Unspecified));
			}
		}

		[Test]
		public void TestTimestampFormat3()
		{
			using (var file = Create( @"TestData\Timestamps\HH_mm_ss.txt"))
			{
				file.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetValue(LogFileProperties.LogEntryCount).Should().Be(1);

				var entry = file.GetEntry(0);
				var today = DateTime.Today;
				entry.Timestamp.Should().Be(new DateTime(today.Year, today.Month, today.Day, 21, 04, 33, DateTimeKind.Unspecified));
			}
		}

		[Test]
		public void TestTimestampFormat4()
		{
			using (var file = Create(@"TestData\Timestamps\ddd MMM dd HH_mm_ss.fff yyyy.txt"))
			{
				file.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetValue(LogFileProperties.LogEntryCount).Should().Be(1);

				var entry = file.GetEntry(0);
				entry.Timestamp.Should().Be(new DateTime(2017, 5, 5, 8, 46, 44, 257, DateTimeKind.Unspecified));
			}
		}

		[Test]
		public void TestTimestampFormat5()
		{
			using (var file = Create(@"TestData\Timestamps\yyyy MMM dd HH_mm_ss.fff.txt"))
			{
				file.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);

				var entry = file.GetEntry(1);
				entry.Timestamp.Should().Be(new DateTime(2017, 5, 9, 6, 51, 57, 583, DateTimeKind.Unspecified));
			}
		}

		[Test]
		public void TestTimestampFormat6()
		{
			using (var file = Create(@"TestData\Timestamps\HH_mm_ss;s.txt"))
			{
				file.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
				file.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);

				var today = DateTime.Today;
				var entry = file.GetEntry(0);
				entry.Timestamp.Should().Be(new DateTime(today.Year, today.Month, today.Day, 6, 51, 57, 135, DateTimeKind.Unspecified));
				entry = file.GetEntry(1);
				entry.Timestamp.Should().Be(new DateTime(today.Year, today.Month, today.Day, 6, 53, 06, 341, DateTimeKind.Unspecified));
			}
		}
	}
}