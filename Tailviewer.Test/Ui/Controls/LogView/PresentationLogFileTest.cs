using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.LogFiles;
using Tailviewer.Ui.Controls.LogView;

namespace Tailviewer.Test.Ui.Controls.LogView
{
	[TestFixture]
	public sealed class PresentationLogFileTest
	{
		private ManualTaskScheduler _scheduler;

		[SetUp]
		public void Setup()
		{
			_scheduler = new ManualTaskScheduler();
		}

		[Test]
		public void TestEmptyFile()
		{
			var index = new PresentationLogFile(_scheduler, new InMemoryLogFile());
			_scheduler.RunOnce();
			index.MaximumWidth.Should().Be(0);
			index.LineCount.Should().Be(0);
		}

		[Test]
		public void TestOneLine()
		{
			var logFile = new InMemoryLogFile();
			var index = new PresentationLogFile(_scheduler, logFile, TimeSpan.Zero);
			logFile.Add(new LogEntry2 {RawContent = "Hello, World!"});

			_scheduler.RunOnce();
			index.MaximumWidth.Should().BeApproximately(85, 1);
			index.LineCount.Should().Be(1);
		}

		[Test]
		public void TestTwoLines()
		{
			var logFile = new InMemoryLogFile();
			var index = new PresentationLogFile(_scheduler, logFile, TimeSpan.Zero);
			logFile.Add(new LogEntry2 {RawContent = "Hello,\r\nWorld!"});

			_scheduler.RunOnce();
			index.MaximumWidth.Should().BeApproximately(46, 1);
			index.LineCount.Should().Be(2);
		}
	}
}