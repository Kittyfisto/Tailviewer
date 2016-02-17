using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using log4net;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class LogFileTest
		: AbstractTest
	{
		public const string File20Mb = @"TestData\20Mb.txt";
		public const string File2Lines = @"TestData\2Lines.txt";
		public const string File2Entries = @"TestData\2LogEntries.txt";
		public const string FileTestLive1 = @"TestData\TestLive1.txt";
		public const string FileTestLive2 = @"TestData\TestLive2.txt";

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[Test]
		public void TestParse()
		{
			DateTime dateTime;
			DateTime.TryParseExact("2015-10-07 19:50:58,998",
			                       "yyyy-MM-dd HH:mm:ss,fff",
			                       CultureInfo.InvariantCulture,
			                       DateTimeStyles.AllowWhiteSpaces,
			                       out dateTime)
			        .Should().BeTrue();

			dateTime.Year.Should().Be(2015);
			dateTime.Month.Should().Be(10);
			dateTime.Day.Should().Be(7);
			dateTime.Hour.Should().Be(19);
			dateTime.Minute.Should().Be(50);
			dateTime.Second.Should().Be(58);
			dateTime.Millisecond.Should().Be(998);
		}

		[Test]
		public void TestDetermineDateTime()
		{
			int? column;
			int? length;
			LogFile.DetermineDateTimePart("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver",
				out column,
				out length);
			column.Should().Be(0);
			length.Should().Be(23);
			// This is not ideal yet because we don't detect that the next 4 characters is the amount of MS, but it's a start...
		}

		[Test]
		public void TestReadAll1()
		{
			using (var file = new LogFile(File20Mb))
			{
				file.Start();
				file.Wait();
				file.Dispose();

				file.StartTimestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

				var entries = file.Entries.ToList();
				entries.Count.Should().Be(165342);
				entries[0].Should().Be(new LogLine(0, "2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver", LevelFlags.Info, new DateTime(2015, 10, 7, 19, 50, 58, 982)));
				entries[entries.Count - 1].Should().Be(new LogLine(165341, "2015-10-07 19:51:42,483 [8092, 6] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Parent process terminated unexpectedly (exit code: -1), shutting down...", LevelFlags.Info, new DateTime(2015, 10, 7, 19, 51, 42, 483)));

				file.DebugCount.Should().Be(165337);
				file.InfoCount.Should().Be(5);
				file.WarningCount.Should().Be(0);
				file.ErrorCount.Should().Be(0);
				file.FatalCount.Should().Be(0);
			}
		}

		[Test]
		public void TestReadAll2()
		{
			using (var file = new LogFile(File20Mb))
			{
				var listener = new Mock<ILogFileListener>();
				var sections = new List<LogFileSection>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
				        .Callback((ILogFile logFile, LogFileSection section) => sections.Add(section));

				file.AddListener(listener.Object, TimeSpan.Zero, 1);
				file.Start();
				file.Wait();

				sections.Count.Should().Be(165343);
				sections[0].Should().Be(LogFileSection.Reset);
				for (int i = 1; i < sections.Count; ++i)
				{
					var change = sections[i];
					change.Index.Should().Be((LogLineIndex)(i-1));
					change.Count.Should().Be(1);
				}
			}
		}

		[Test]
		public void TestRead2Lines()
		{
			using (var file = new LogFile(File2Lines))
			{
				var listener = new Mock<ILogFileListener>();
				var changes = new List<LogFileSection>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
						.Callback((ILogFile logFile, LogFileSection section) => changes.Add(section));

				file.AddListener(listener.Object, TimeSpan.Zero, 1);
				file.Start();
				file.Wait();

				changes.Should().Equal(new[]
					{
						LogFileSection.Reset, 
						new LogFileSection(0, 1),
						new LogFileSection(1, 1)
					});

				file.DebugCount.Should().Be(1);
				file.InfoCount.Should().Be(1);
				file.WarningCount.Should().Be(0);
				file.ErrorCount.Should().Be(0);
				file.FatalCount.Should().Be(0);
			}
		}

		[Test]
		public void TestRead2LogEntries()
		{
			using (var file = new LogFile(File2Entries))
			{
				var listener = new Mock<ILogFileListener>();
				var changes = new List<LogFileSection>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
						.Callback((ILogFile logFile, LogFileSection section) => changes.Add(section));

				file.AddListener(listener.Object, TimeSpan.Zero, 1);
				file.Start();
				file.Wait();

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

				file.StartTimestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

				var lines = file.GetSection(new LogFileSection(0, 6));
				lines.Should().Equal(new[]
					{
						new LogLine(0, 0, "2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver", LevelFlags.Info, new DateTime(2015, 10, 7, 19, 50, 58, 982)),
						new LogLine(1, 0, "Foobar", LevelFlags.Info, new DateTime(2015, 10, 7, 19, 50, 58, 982)),
						new LogLine(2, 0, "Some more info", LevelFlags.Info, new DateTime(2015, 10, 7, 19, 50, 58, 982)),
						new LogLine(3, 1, "2015-10-07 19:50:58,998 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1", LevelFlags.Debug, new DateTime(2015, 10, 7, 19, 50, 58, 998)),
						new LogLine(4, 1, "Hey look at me", LevelFlags.Debug, new DateTime(2015, 10, 7, 19, 50, 58, 998)),
						new LogLine(5, 1, "dwadawdadw", LevelFlags.Debug, new DateTime(2015, 10, 7, 19, 50, 58, 998))
					});

				file.DebugCount.Should().Be(1);
				file.InfoCount.Should().Be(1);
				file.WarningCount.Should().Be(0);
				file.ErrorCount.Should().Be(0);
				file.FatalCount.Should().Be(0);
			}
		}

		[Test]
		public void TestGetSection1()
		{
			using (var file = new LogFile(File20Mb))
			{
				file.Start();
				file.Wait();

				file.StartTimestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

				var section = file.GetSection(new LogFileSection(0, 10));
				section.Should().Equal(new[]
					{
						new LogLine(0,
						            "2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver",
						            LevelFlags.Info,
									new DateTime(2015, 10, 7, 19, 50, 58, 982)),
						new LogLine(1,
						            "2015-10-07 19:50:58,998 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1",
						            LevelFlags.Debug,
									new DateTime(2015, 10, 7, 19, 50, 58, 998)),
						new LogLine(2,
						            "2015-10-07 19:50:59,013 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551613) 'SharpRemote.Heartbeat' implementing 'SharpRemote.IHeartbeat'",
						            LevelFlags.Debug,
									new DateTime(2015, 10, 7, 19, 50, 59, 013)),
						new LogLine(3,
						            "2015-10-07 19:50:59,062 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551614) 'SharpRemote.Latency' implementing 'SharpRemote.ILatency'",
						            LevelFlags.Debug,
									new DateTime(2015, 10, 7, 19, 50, 59, 062)),
						new LogLine(4,
						            "2015-10-07 19:50:59,067 [8092, 1] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - Creating new servant (#18446744073709551615) 'SharpRemote.Hosting.SubjectHost' implementing 'SharpRemote.Hosting.ISubjectHost'",
						            LevelFlags.Debug,
									new DateTime(2015, 10, 7, 19, 50, 59, 067)),
						new LogLine(5,
						            "2015-10-07 19:50:59,081 [8092, 1] INFO  SharpRemote.SocketRemotingEndPointServer (null) - EndPoint '<Unnamed>' listening on 0.0.0.0:49152",
						            LevelFlags.Info,
									new DateTime(2015, 10, 7, 19, 50, 59, 081)),
						new LogLine(6,
						            "2015-10-07 19:50:59,141 [8092, 6] DEBUG SharpRemote.SocketRemotingEndPointServer (null) - Incoming connection from '127.0.0.1:10348', starting handshake...",
						            LevelFlags.Debug,
									new DateTime(2015, 10, 7, 19, 50, 59, 141)),
						new LogLine(7,
						            "2015-10-07 19:50:59,171 [8092, 6] INFO  SharpRemote.AbstractIPSocketRemotingEndPoint (null) - <Unnamed>: Connected to 127.0.0.1:10348",
						            LevelFlags.Info,
									new DateTime(2015, 10, 7, 19, 50, 59, 171)),
						new LogLine(8,
						            "2015-10-07 19:50:59,181 [8092, 10] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - 0.0.0.0:49152 to 127.0.0.1:10348: sending RPC #1 to 18446744073709551611.Beat",
						            LevelFlags.Debug,
									new DateTime(2015, 10, 7, 19, 50, 59, 181)),
						new LogLine(9,
						            "2015-10-07 19:50:59,182 [8092, 11] DEBUG SharpRemote.AbstractSocketRemotingEndPoint (null) - 0.0.0.0:49152 to 127.0.0.1:10348: sending RPC #2 to 18446744073709551612.Roundtrip",
						            LevelFlags.Debug,
									new DateTime(2015, 10, 7, 19, 50, 59, 182))
					});
			}
		}

		[Test]
		public void TestFilter1()
		{
			using (var file = new LogFile(File20Mb))
			{
				file.Start();
				file.Wait();
				file.Count.Should().Be(165342);

				using (var filtered = file.AsFiltered(Filter.Create("info")))
				{
					filtered.Wait();
					filtered.Count.Should().Be(5);
					filtered.StartTimestamp.Should().Be(new DateTime(2015, 10, 7, 19, 50, 58, 982));

					var section = filtered.GetSection(new LogFileSection(0, 5));
					section.Should().Equal(new[]
						{
							new LogLine(0,
							            "2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver",
							            LevelFlags.Info,
										new DateTime(2015, 10, 7, 19, 50, 58, 982, DateTimeKind.Unspecified)),
							new LogLine(5,
							            "2015-10-07 19:50:59,081 [8092, 1] INFO  SharpRemote.SocketRemotingEndPointServer (null) - EndPoint '<Unnamed>' listening on 0.0.0.0:49152",
							            LevelFlags.Info,
										new DateTime(2015, 10, 7, 19, 50, 59, 081)),
							new LogLine(7,
							            "2015-10-07 19:50:59,171 [8092, 6] INFO  SharpRemote.AbstractIPSocketRemotingEndPoint (null) - <Unnamed>: Connected to 127.0.0.1:10348",
							            LevelFlags.Info,
										new DateTime(2015, 10, 7, 19, 50, 59, 171)),
							new LogLine(165340,
							            "2015-10-07 19:51:42,481 [8092, EndPoint '<Unnamed>' Socket Reading] INFO  SharpRemote.AbstractSocketRemotingEndPoint (null) - Disconnecting socket '<Unnamed>' from 127.0.0.1:10348: ReadFailure",
							            LevelFlags.Info,
										new DateTime(2015, 10, 7, 19, 51, 42, 481)),
							new LogLine(165341,
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
			using (var file = new LogFile(File20Mb))
			{
				file.Start();
				file.Wait();
				file.Count.Should().Be(165342);

				using (var filtered = file.AsFiltered(Filter.Create("info")))
				{
					var listener = new Mock<ILogFileListener>();
					var sections = new List<LogFileSection>();
					listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
							.Callback((ILogFile logFile, LogFileSection section) => sections.Add(section));
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
		public void TestFilter3()
		{
			const string fname = "TestFilter3.log";
			using (var stream = File.OpenWrite(fname))
			using (var writer = new StreamWriter(stream))
			{
				stream.SetLength(0);
				writer.WriteLine("INFO - Test");
			}

			using (var file = new LogFile(fname))
			{
				file.Start();
				file.Wait();
				file.Count.Should().Be(1);

				using (var filtered = file.AsFiltered(Filter.Create("e", LevelFlags.All), TimeSpan.Zero))
				{
					filtered.Wait();
					filtered.GetSection(new LogFileSection(0, filtered.Count)).Should().Equal(new[]
						{
							new LogLine(0, "INFO - Test", LevelFlags.Info)
						});

					var listener = new Mock<ILogFileListener>();
					var sections = new List<LogFileSection>();
					listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
							.Callback((ILogFile logFile, LogFileSection section) => sections.Add(section));
					filtered.AddListener(listener.Object, TimeSpan.FromHours(1), 1000);

					using (var stream = new FileStream(fname, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
					{
						stream.SetLength(0);
					}

					WaitUntil(() => filtered.Count == 0, TimeSpan.FromSeconds(1)).Should().BeTrue();
					sections.Should().Equal(new[] {LogFileSection.Reset});
				}
			}
		}

		[Test]
		public void TestLive1()
		{
			const string fname = "TestLive1.log";
			using (var logger = new Logger(fname))
			using (var logFile = new LogFile(fname))
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
			using (var logFile = new LogFile(fname))
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

			using (var logFile = new LogFile(fname))
			{
				logFile.Start();
				logFile.Wait();
				logFile.Count.Should().Be(1);
				File.Delete(fname);
			}
		}

		[Test]
		public void TestClear1()
		{
			const string fname = "TestClear1.log";
			using (var stream = File.OpenWrite(fname))
			using (var writer = new StreamWriter(stream))
			{
				stream.SetLength(0);
				writer.WriteLine("Test");
			}

			using (var logFile = new LogFile(fname))
			{
				logFile.Start();
				logFile.Wait();
				logFile.Count.Should().Be(1);

				using (var stream = new FileStream(fname, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
				using (var writer = new StreamWriter(stream))
				{
					stream.SetLength(0);

					WaitUntil(() => logFile.Count == 0, TimeSpan.FromSeconds(1)).Should().BeTrue();
					logFile.Count.Should().Be(0);

					writer.WriteLine("Hello World!");
					writer.Flush();

					WaitUntil(() => logFile.Count == 1, TimeSpan.FromSeconds(1)).Should().BeTrue();
					logFile.Entries.Should().Equal(new[]
						{
							new LogLine(0, "Hello World!", LevelFlags.None)
						});
				}
			}
		}

		[Test]
		public void TestClear2()
		{
			const string fname = "TestClear2.log";
			using (var stream = File.OpenWrite(fname))
			using (var writer = new StreamWriter(stream))
			{
				stream.SetLength(0);
				writer.WriteLine("Test");
			}

			using (var logFile = new LogFile(fname))
			{
				var listener = new Mock<ILogFileListener>();
				var sections = new List<LogFileSection>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogFile>(), It.IsAny<LogFileSection>()))
						.Callback((ILogFile log, LogFileSection section) => sections.Add(section));
				logFile.AddListener(listener.Object, TimeSpan.Zero, 2);

				logFile.Start();
				logFile.Wait();
				logFile.Count.Should().Be(1);

				using (var stream = new FileStream(fname, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
				{
					stream.SetLength(0);

					WaitUntil(() => logFile.Count == 0, TimeSpan.FromSeconds(1)).Should().BeTrue();
					logFile.Count.Should().Be(0);
					sections.Should().Equal(new[]
						{
							new LogFileSection(-1, 0),
							new LogFileSection(0, 1),
							new LogFileSection(-1, 0)
						});
				}
			}
		}
	}
}