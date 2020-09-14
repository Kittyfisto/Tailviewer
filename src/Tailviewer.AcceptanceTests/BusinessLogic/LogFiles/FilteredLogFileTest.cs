using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.Filters;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.AcceptanceTests.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class FilteredLogFileTest
	{
		private const string File20Mb = TextLogFileAcceptanceTest.File20Mb;

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
			return new TextLogFile(serviceContainer, fileName);
		}

		[Test]
		public void TestFilter1()
		{
			using (var file = Create(File20Mb))
			{
				file.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(165342);

				using (FilteredLogFile filtered = file.AsFiltered(_scheduler, null, Filter.Create("info")))
				{
					filtered.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(5);
					filtered.GetValue(LogFileProperties.StartTimestamp).Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

					LogLine[] section = filtered.GetSection(new LogFileSection(0, 5));
					section.Should().Equal(new[]
						{
							new LogLine(0, 0,
							            "2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver",
							            LevelFlags.Info,
							            new DateTime(2015, 10, 7, 19, 50, 58, 982, DateTimeKind.Unspecified)),
							new LogLine(1, 1,
							            "2015-10-07 19:50:59,081 [8092, 1] INFO  SharpRemote.SocketRemotingEndPointServer (null) - EndPoint '<Unnamed>' listening on 0.0.0.0:49152",
							            LevelFlags.Info,
							            new DateTime(2015, 10, 7, 19, 50, 59, 081)),
							new LogLine(2, 2,
							            "2015-10-07 19:50:59,171 [8092, 6] INFO  SharpRemote.AbstractIPSocketRemotingEndPoint (null) - <Unnamed>: Connected to 127.0.0.1:10348",
							            LevelFlags.Info,
							            new DateTime(2015, 10, 7, 19, 50, 59, 171)),
							new LogLine(3, 3,
							            "2015-10-07 19:51:42,481 [8092, EndPoint '<Unnamed>' Socket Reading] INFO  SharpRemote.AbstractSocketRemotingEndPoint (null) - Disconnecting socket '<Unnamed>' from 127.0.0.1:10348: ReadFailure",
							            LevelFlags.Info,
							            new DateTime(2015, 10, 7, 19, 51, 42, 481)),
							new LogLine(4, 4,
							            "2015-10-07 19:51:42,483 [8092, 6] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Parent process terminated unexpectedly (exit code: -1), shutting down...",
							            LevelFlags.Info,
							            new DateTime(2015, 10, 7, 19, 51, 42, 483))
						});
				}
			}
		}

		[Test]
		public void TestFilter2()
		{
			using (var file = Create(File20Mb))
			{
				file.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(165342);

				using (FilteredLogFile filtered = file.AsFiltered(_scheduler, null, Filter.Create("info")))
				{
					var listener = new Mock<ILogFileListener>();
					var sections = new List<LogFileSection>();
					listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
							.Callback((ILogFile logFile, LogFileSection section) => sections.Add(section));

					filtered.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(5);

					filtered.AddListener(listener.Object, TimeSpan.Zero, 1);

					sections.Should().Equal(new object[]
						{
							LogFileSection.Reset,
							new LogFileSection(0, 1),
							new LogFileSection(1, 1),
							new LogFileSection(2, 1),
							new LogFileSection(3, 1),
							new LogFileSection(4, 1)
						});
				}
			}
		}

		[Test]
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
				file.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);

				file.Property(x=> x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);

				using (FilteredLogFile filtered = file.AsFiltered(_scheduler, null, Filter.Create("e", LevelFlags.All), TimeSpan.Zero))
				{
					var listener = new Mock<ILogFileListener>();
					var sections = new List<LogFileSection>();
					listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
							.Callback((ILogFile logFile, LogFileSection section) => sections.Add(section));
					filtered.AddListener(listener.Object, TimeSpan.FromHours(1), 1000);

					filtered.Property(x => x.EndOfSourceReached).ShouldAfter(TimeSpan.FromSeconds(5)).BeTrue();
					filtered.GetSection(new LogFileSection(0, filtered.Count)).Should().Equal(new[]
						{
							new LogLine(0, "INFO - Test", LevelFlags.Info)
						});

					using (var stream = new FileStream(fname, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
					{
						stream.SetLength(0);
					}

					filtered.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(0);
					filtered.Property(x => x.EndOfSourceReached).ShouldAfter(TimeSpan.FromSeconds(5)).BeTrue();
					sections.Should().EndWith(LogFileSection.Reset);
				}
			}
		}

	}
}
