using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.Entries;
using Tailviewer.Core.Sources;
using Tailviewer.Settings;
using Tailviewer.Ui.LogView;

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
			var index = new PresentationLogSource(_scheduler, new InMemoryLogSource(), TextSettings.Default);
			_scheduler.RunOnce();
			index.MaximumWidth.Should().Be(0);
			index.LineCount.Should().Be(0);
		}

		[Test]
		public void TestOneLine()
		{
			var logFile = new InMemoryLogSource();
			var index = new PresentationLogSource(_scheduler, logFile, TimeSpan.Zero, TextSettings.Default);
			logFile.Add(new LogEntry {RawContent = "Hello, World!"});

			_scheduler.RunOnce();
			index.MaximumWidth.Should().BeApproximately(85, 1);
			index.LineCount.Should().Be(1);
		}

		[Test]
		public void TestTwoLines()
		{
			var logFile = new InMemoryLogSource();
			var index = new PresentationLogSource(_scheduler, logFile, TimeSpan.Zero, TextSettings.Default);
			logFile.Add(new LogEntry {RawContent = "Hello,\r\nWorld!"});

			_scheduler.RunOnce();
			index.MaximumWidth.Should().BeApproximately(46, 1);
			index.LineCount.Should().Be(2);
		}

		[Test]
		public void TestAddSeveralEntries()
		{
			var logFile = new InMemoryLogSource();
			var index = new PresentationLogSource(_scheduler, logFile, TimeSpan.Zero, TextSettings.Default);

			logFile.Add(new LogEntry {RawContent = "Foo"});
			_scheduler.RunOnce();
			index.MaximumWidth.Should().BeApproximately(19.8, 0.1);
			index.LineCount.Should().Be(1);

			logFile.Add(new LogEntry {RawContent = "Hello,\r\nWorld!"});
			_scheduler.RunOnce();
			index.MaximumWidth.Should().BeApproximately(46.2, 0.1);
			index.LineCount.Should().Be(3);

			logFile.Add(new LogEntry {RawContent = "Bar"});
			_scheduler.RunOnce();
			index.MaximumWidth.Should().BeApproximately(46.2, 0.1);
			index.LineCount.Should().Be(4);
		}

		[Test]
		public void TestPartialInvalidate()
		{
			var logFile = new InMemoryLogSource();
			var index = new PresentationLogSource(_scheduler, logFile, TimeSpan.Zero, TextSettings.Default);

			logFile.Add(new LogEntry {RawContent = "Foo"});
			_scheduler.RunOnce();
			index.MaximumWidth.Should().BeApproximately(19.8, 0.1);
			index.LineCount.Should().Be(1);

			logFile.Add(new LogEntry {RawContent = "Hello,\r\nWorld!"});
			_scheduler.RunOnce();
			index.MaximumWidth.Should().BeApproximately(46.2, 0.1);
			index.LineCount.Should().Be(3);

			logFile.RemoveFrom(1);
			_scheduler.RunOnce();
			index.MaximumWidth.Should().BeApproximately(19.8, 0.1);
			index.LineCount.Should().Be(1);
		}
	}
}