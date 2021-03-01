using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Core.Sources.Text.Simple;

namespace Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text.Simple
{
	[TestFixture]
	public sealed class TextLogSourceAcceptanceTest
		: AbstractTextLogSourceAcceptanceTest
	{
		protected override ILogSource Create(ITaskScheduler taskScheduler, string fileName, ILogFileFormat format, Encoding encoding)
		{
			return new TextLogSource(taskScheduler, fileName, format, encoding);
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
				logFile.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);

				logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1);

				using (var stream = new FileStream(fname, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
				using (var writer = new StreamWriter(stream))
				{
					stream.SetLength(0);

					logFile.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(0);
					logFile.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0);

					writer.WriteLine("Hello World!");
					writer.Flush();

					logFile.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);
					var entries = logFile.GetEntries();
					entries.Count.Should().Be(1);
					entries[0].Index.Should().Be(0);
					entries[0].RawContent.Should().Be("Hello World!");
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
				var listener = new Mock<ILogSourceListener>();
				var modifications = new List<LogSourceModification>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
				        .Callback((ILogSource log, LogSourceModification modification) => modifications.Add(modification));
				logFile.AddListener(listener.Object, TimeSpan.Zero, 2);

				logFile.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(1);

				using (var stream = new FileStream(fname, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
				{
					stream.SetLength(0);

					logFile.Property(x => x.GetProperty(GeneralProperties.LogEntryCount)).ShouldAfter(TimeSpan.FromSeconds(5)).Be(0);
					modifications.Should().EndWith(LogSourceModification.Reset());
				}
			}
		}

		[Test]
		public void TestRead2Lines()
		{
			using (var file = Create(File2Lines))
			{
				var listener = new Mock<ILogSourceListener>();
				var modifications = new List<LogSourceModification>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
				        .Callback((ILogSource logFile, LogSourceModification modification) => modifications.Add(modification));

				file.AddListener(listener.Object, TimeSpan.Zero, 1);

				modifications.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(3);
				modifications.Should().Equal(new[]
					{
						LogSourceModification.Reset(),
						LogSourceModification.Appended(0, 1),
						LogSourceModification.Appended(1, 1)
					});
			}
		}

		[Test]
		public void TestRead2LogEntries()
		{
			using (var file = Create( File2Entries))
			{
				var listener = new Mock<ILogSourceListener>();
				var modifications = new List<LogSourceModification>();
				listener.Setup(x => x.OnLogFileModified(It.IsAny<ILogSource>(), It.IsAny<LogSourceModification>()))
				        .Callback((ILogSource logFile, LogSourceModification modification) => modifications.Add(modification));

				file.AddListener(listener.Object, TimeSpan.Zero, 1);

				modifications.Property(x => x.Count).ShouldAfter(TimeSpan.FromSeconds(5)).Be(7);

				modifications.Should().Equal(new[]
					{
						LogSourceModification.Reset(),
						LogSourceModification.Appended(0, 1),
						LogSourceModification.Appended(1, 1),
						LogSourceModification.Appended(2, 1),
						LogSourceModification.Appended(3, 1),
						LogSourceModification.Appended(4, 1),
						LogSourceModification.Appended(5, 1)
					});


				var entries = file.GetEntries(new LogFileSection(0, 6));
				entries[0].Index.Should().Be(0);
				entries[0].LogEntryIndex.Should().Be(0);
				entries[0].RawContent.Should().Be("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");
				
				entries[1].Index.Should().Be(1);
				entries[1].LogEntryIndex.Should().Be(1);
				entries[1].RawContent.Should().Be("Foobar");
				
				entries[2].Index.Should().Be(2);
				entries[2].LogEntryIndex.Should().Be(2);
				entries[2].RawContent.Should().Be("Some more info");
				
				entries[3].Index.Should().Be(3);
				entries[3].LogEntryIndex.Should().Be(3);
				entries[3].RawContent.Should().Be("2015-10-07 19:50:58,998 [8092, 1] DEBUG SharpRemote.Hosting.OutOfProcessSiloServer (null) - Args.Length: 1");
				
				entries[4].Index.Should().Be(4);
				entries[4].LogEntryIndex.Should().Be(4);
				entries[4].RawContent.Should().Be("Hey look at me");
				
				entries[5].Index.Should().Be(5);
				entries[5].LogEntryIndex.Should().Be(5);
				entries[5].RawContent.Should().Be("dwadawdadw");
			}
		}

		[Test]
		public void TestReadAll1()
		{
			using (var file = Create(File20Mb))
			{
				file.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(20)).Be(Percentage.HundredPercent);

				var entries = file.GetEntries();
				entries.Count.Should().Be(165342);
				entries[0].Index.Should().Be(0);
				entries[0].RawContent.Should().Be("2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver");

				entries[entries.Count - 1].Index.Should().Be(165341);
				entries[entries.Count - 1].RawContent.Should()
				                          .Be("2015-10-07 19:51:42,483 [8092, 6] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Parent process terminated unexpectedly (exit code: -1), shutting down...");
			}
		}
	}
}