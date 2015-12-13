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
using log4net;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogFileTest
	{
		public const string File20Mb = @"TestData\20Mb.txt";
		public const string File2Lines = @"TestData\2Lines.txt";

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[Test]
		public void TestDetermineDateTime()
		{
			int? column;
			int? length;
			LogFile.DetermineDateTimePart("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver",
				out column,
				out length);
			column.Should().Be(0);
			length.Should().Be(20);
			// This is not ideal yet because we don't detect that the next 4 characters is the amount of MS, but it's a start...
		}

		[Test]
		public void TestReadAll1()
		{
			using (var file = LogFile.FromFile(File20Mb))
			{
				file.Start();
				file.Wait();
				file.Dispose();

				var entries = file.Entries.ToList();
				entries.Count.Should().Be(165342);
				entries[0].Should().Be("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");
				entries[entries.Count - 1].Should().Be("2015-10-07 19:51:42,483 [8092, 6] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Parent process terminated unexpectedly (exit code: -1), shutting down...");
			}
		}

		[Test]
		public void TestReadAll2()
		{
			using (var file = LogFile.FromFile(File20Mb))
			{
				var listener = new Mock<ILogFileListener>();
				var changes = new List<LogFileSection>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<LogFileSection>()))
				        .Callback((LogFileSection section) => changes.Add(section));

				file.AddListener(listener.Object, TimeSpan.Zero, 1);
				file.Start();
				file.Wait();

				changes.Count.Should().Be(165342);
				for (int i = 0; i < changes.Count; ++i)
				{
					var change = changes[i];
					change.Index.Should().Be(i);
					change.Count.Should().Be(1);
				}
			}
		}

		[Test]
		public void TestRead2Lines()
		{
			using (var file = LogFile.FromFile(File2Lines))
			{
				var listener = new Mock<ILogFileListener>();
				var changes = new List<LogFileSection>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<LogFileSection>()))
						.Callback((LogFileSection section) => changes.Add(section));

				file.AddListener(listener.Object, TimeSpan.Zero, 1);
				file.Start();
				file.Wait();

				changes.Count.Should().Be(2);
				changes.Should().Equal(new[]
					{
						new LogFileSection(0, 1),
						new LogFileSection(1, 1)
					});
			}
		}

		[Test]
		public void TestGetSection1()
		{
			using (var file = LogFile.FromFile(File20Mb))
			{
				file.Start();
				file.Wait();

				file.GetSection(new LogFileSection(0, 10))
					.Should().Equal(new[]
					    {
						    "2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver",
							"2015-10-07 19:50:58,998 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1",
						    "2015-10-07 19:50:59,013 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551613) 'SharpRemote.Heartbeat' implementing 'SharpRemote.IHeartbeat'",
						    "2015-10-07 19:50:59,062 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551614) 'SharpRemote.Latency' implementing 'SharpRemote.ILatency'",
						    "2015-10-07 19:50:59,067 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551615) 'SharpRemote.Hosting.SubjectHost' implementing 'SharpRemote.Hosting.ISubjectHost'",
						    "2015-10-07 19:50:59,081 [8092, 1] INFO  SharpRemote.SocketRemotingEndPointServer (null) - EndPoint '<Unnamed>' listening on 0.0.0.0:49152",
						    "2015-10-07 19:50:59,141 [8092, 6] DEBUG SharpRemote.SocketRemotingEndPointServer (null) - Incoming connection from '127.0.0.1:10348', starting handshake...",
						    "2015-10-07 19:50:59,171 [8092, 6] INFO  SharpRemote.AbstractIPSocketRemotingEndPoint (null) - <Unnamed>: Connected to 127.0.0.1:10348",
						    "2015-10-07 19:50:59,181 [8092, 10] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - 0.0.0.0:49152 to 127.0.0.1:10348: sending RPC #1 to 18446744073709551611.Beat",
						    "2015-10-07 19:50:59,182 [8092, 11] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - 0.0.0.0:49152 to 127.0.0.1:10348: sending RPC #2 to 18446744073709551612.Roundtrip"
						});
			}
		}

		[Test]
		public void TestFilter1()
		{
			using (var file = LogFile.FromFile(File20Mb))
			{
				file.Start();
				file.Wait();
				file.Count.Should().Be(165342);

				using (var filtered = file.Filter("info"))
				{
					filtered.Wait();
					filtered.Count.Should().Be(5);
					filtered.GetSection(new LogFileSection(0, 5))
					        .Should().Equal(new[]
						        {
							        "2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver",
							        "2015-10-07 19:50:59,081 [8092, 1] INFO  SharpRemote.SocketRemotingEndPointServer (null) - EndPoint '<Unnamed>' listening on 0.0.0.0:49152",
							        "2015-10-07 19:50:59,171 [8092, 6] INFO  SharpRemote.AbstractIPSocketRemotingEndPoint (null) - <Unnamed>: Connected to 127.0.0.1:10348",
							        "2015-10-07 19:51:42,481 [8092, EndPoint '<Unnamed>' Socket Reading] INFO  SharpRemote.AbstractSocketRemotingEndPoint (null) - Disconnecting socket '<Unnamed>' from 127.0.0.1:10348: ReadFailure",
							        "2015-10-07 19:51:42,483 [8092, 6] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Parent process terminated unexpectedly (exit code: -1), shutting down..."
						        });
				}
			}
		}

		[Test]
		public void TestFilter2()
		{
			using (var file = LogFile.FromFile(File20Mb))
			{
				file.Start();
				file.Wait();
				file.Count.Should().Be(165342);

				using (var filtered = file.Filter("info"))
				{
					var listener = new Mock<ILogFileListener>();
					var sections = new List<LogFileSection>();
					listener.Setup(x => x.OnLogFileModified(It.IsAny<LogFileSection>()))
							.Callback((LogFileSection section) => sections.Add(section));
					filtered.Wait();
					filtered.AddListener(listener.Object, TimeSpan.Zero, 1);

					sections.Should().Equal(new object[]
						{
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
		public void TestLive1()
		{
			const string fname = "TestLive1.log";
			using (var logger = new Logger(fname))
			using (var logFile = LogFile.FromFile(fname))
			{
				logFile.Start();
				logFile.Count.Should().Be(0);

				Log.Info("Test");
				WaitUntil(() => logFile.Count == 1, TimeSpan.FromSeconds(1));
			}
		}

		[Test]
		public void TestLive2()
		{
			const string fname = "TestLive2.log";
			using (var logger = new Logger(fname))
			using (var logFile = LogFile.FromFile(fname))
			{
				logFile.Start();
				logFile.Count.Should().Be(0);

				Log.Info("Hello");
				WaitUntil(() => logFile.Count == 1, TimeSpan.FromSeconds(1));

				Log.Info("world!");
				WaitUntil(() => logFile.Count == 2, TimeSpan.FromSeconds(1));
			}
		}

		[Test]
		[Ignore("Not yet implemented")]
		public void TestDelete1()
		{
			const string fname = "TestDelete1.log";
			using (new Logger(fname))
			{
				Log.Info("Test");
			}

			using (var logFile = LogFile.FromFile(fname))
			{
				logFile.Start();
				logFile.Wait();
				logFile.Count.Should().Be(1);

				WaitUntil(() => logFile.Count == 1, TimeSpan.FromSeconds(1));
				File.Delete(fname);
			}
		}

		public static bool WaitUntil(Func<bool> fn, TimeSpan timeout)
		{
			var started = DateTime.UtcNow;
			while ((DateTime.UtcNow - started) < timeout)
			{
				if (fn())
					return true;

				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			}

			return false;
		}
	}
}