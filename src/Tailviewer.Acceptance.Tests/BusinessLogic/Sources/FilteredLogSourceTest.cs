﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Acceptance.Tests.BusinessLogic.Sources.Text;
using Tailviewer.Api;
using Tailviewer.Core;

namespace Tailviewer.Acceptance.Tests.BusinessLogic.Sources
{
	[TestFixture]
	public sealed class FilteredLogSourceTest
	{
		private const string File20Mb = AbstractTextLogSourceAcceptanceTest.File20Mb;

		private DefaultTaskScheduler _taskScheduler;
		private Filesystem _filesystem;

		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new DefaultTaskScheduler();
			_filesystem = new Filesystem(_taskScheduler);
		}

		[TearDown]
		public void TearDown()
		{
			_taskScheduler.Dispose();
		}

		private TextLogSource Create(string fileName)
		{
			return new TextLogSource(_filesystem, _taskScheduler, fileName, LogFileFormats.GenericText, Encoding.Default);
		}

		[Test]
		[Ignore("I fucked this one up")]
		public void TestFilter1()
		{
			using (var file = Create(File20Mb))
			{
				file.Property(x => x.GetProperty(Properties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(165342);

				using (FilteredLogSource filtered = file.AsFiltered(_taskScheduler, null, Filter.Create("info")))
				{
					filtered.Property(x => x.GetProperty(Properties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);
					filtered.Property(x => x.GetProperty(Properties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(5);
					filtered.GetProperty(Properties.StartTimestamp).Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

					var entries = filtered.GetEntries(new LogSourceSection(0, 5));
					entries[0].Index.Should().Be(0);
					entries[0].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982, DateTimeKind.Unspecified));
					entries[0].LogLevel.Should().Be(LevelFlags.Info);
					entries[0].RawContent.Should().Be("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");
					entries[1].Index.Should().Be(1);
					entries[1].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 081));
					entries[1].LogLevel.Should().Be(LevelFlags.Info);
					entries[1].RawContent.Should().Be("2015-10-07 19:50:59,081 [8092, 1] INFO  SharpRemote.SocketRemotingEndPointServer (null) - EndPoint '<Unnamed>' listening on 0.0.0.0:49152");
					entries[2].Index.Should().Be(2);
					entries[2].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 59, 171));
					entries[2].LogLevel.Should().Be(LevelFlags.Info);
					entries[2].RawContent.Should().Be("2015-10-07 19:50:59,171 [8092, 6] INFO  SharpRemote.AbstractIPSocketRemotingEndPoint (null) - <Unnamed>: Connected to 127.0.0.1:10348");
					entries[3].Index.Should().Be(3);
					entries[3].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 51, 42, 481));
					entries[3].LogLevel.Should().Be(LevelFlags.Info);
					entries[3].RawContent.Should().Be("2015-10-07 19:51:42,481 [8092, EndPoint '<Unnamed>' Socket Reading] INFO  SharpRemote.AbstractSocketRemotingEndPoint (null) - Disconnecting socket '<Unnamed>' from 127.0.0.1:10348: ReadFailure");
					entries[4].Index.Should().Be(4);
					entries[4].Timestamp.Should().Be(new DateTime(2015, 10, 7, 19, 51, 42, 483));
					entries[4].LogLevel.Should().Be(LevelFlags.Info);
					entries[4].RawContent.Should().Be("2015-10-07 19:51:42,483 [8092, 6] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Parent process terminated unexpectedly (exit code: -1), shutting down...");
				}
			}
		}

		[Test]
		[Ignore("I fucked this one up")]
		public void TestFilter2()
		{
			using (var file = Create(File20Mb))
			{
				file.Property(x => x.GetProperty(Properties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(10)).Be(165342);

				using (FilteredLogSource filtered = file.AsFiltered(_taskScheduler, null, Filter.Create("info")))
				{
					var listener = new Mock<ILogSourceListener>();
					var modifications = new List<LogSourceModification>();
					listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
							.Callback((ILogSource logFile, LogSourceModification modification) => modifications.Add(modification));

					filtered.Property(x => x.GetProperty(Properties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(5);

					filtered.AddListener(listener.Object, TimeSpan.Zero, 1);

					modifications.Should().Equal(new object[]
						{
							LogSourceModification.Reset(),
							LogSourceModification.Appended(0, 1),
							LogSourceModification.Appended(1, 1),
							LogSourceModification.Appended(2, 1),
							LogSourceModification.Appended(3, 1),
							LogSourceModification.Appended(4, 1)
						});
				}
			}
		}

		[Test]
		[Ignore("I fucked this one up")]
		public void TestFilter3()
		{
			const string fname = "TestFilter3.log";
			using (FileStream stream = File.OpenWrite(fname))
			using (var writer = new StreamWriter(stream))
			{
				stream.SetLength(0);
				writer.WriteLine("INFO - Test");
			}

			using (var file = Create(fname))
			{
				file.Property(x => x.GetProperty(Properties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);

				file.Property(x=> x.GetProperty(Properties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);

				using (FilteredLogSource filtered = file.AsFiltered(_taskScheduler, null, Filter.Create("e", LevelFlags.All), TimeSpan.Zero))
				{
					var listener = new Mock<ILogSourceListener>();
					var modifications = new List<LogSourceModification>();
					listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
							.Callback((ILogSource logFile, LogSourceModification section) => modifications.Add(section));
					filtered.AddListener(listener.Object, TimeSpan.FromHours(1), 1000);

					filtered.Property(x => x.GetProperty(Properties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
					var entries = filtered.GetEntries(new LogSourceSection(0, filtered.GetProperty(Properties.LogEntryCount)));
					entries.Count.Should().Be(1);
					entries[0].Index.Should().Be(0);
					entries[0].RawContent.Should().Be("INFO - Test");
					entries[0].LogLevel.Should().Be(LevelFlags.Info);

					using (var stream = new FileStream(fname, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
					{
						stream.SetLength(0);
					}

					filtered.Property(x => x.GetProperty(Properties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(0);
					filtered.Property(x => x.GetProperty(Properties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(Percentage.HundredPercent);
					modifications.Should().EndWith(LogSourceModification.Reset());
				}
			}
		}

	}
}
