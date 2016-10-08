using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Scheduling;
using Tailviewer.BusinessLogic.Searches;

namespace Tailviewer.Test.BusinessLogic.Searches
{
	[TestFixture]
	public sealed class LogFileSearchProxyAcceptanceTest
	{
		private Mock<ILogFile> _logFile;
		private LogFileListenerCollection _listeners;
		private List<LogLine> _entries;
		private TaskScheduler _scheduler;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_scheduler = new TaskScheduler();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_scheduler.Dispose();
		}

		[SetUp]
		public void Setup()
		{
			_entries = new List<LogLine>();
			_logFile = new Mock<ILogFile>();
			_listeners = new LogFileListenerCollection(_logFile.Object);
			_logFile.Setup(x => x.Wait(It.IsAny<TimeSpan>())).Returns(true);
			_logFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
					.Callback(
						(LogFileSection section, LogLine[] entries) =>
						_entries.CopyTo((int)section.Index, entries, 0, section.Count));
			_logFile.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
					.Callback((ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount) => _listeners.AddListener(listener, maximumWaitTime, maximumLineCount));
			_logFile.Setup(x => x.RemoveListener(It.IsAny<ILogFileListener>()))
			        .Callback((ILogFileListener listener) => _listeners.RemoveListener(listener));
			_logFile.Setup(x => x.GetLine(It.IsAny<int>())).Returns((int index) => _entries[index]);
			_logFile.Setup(x => x.Count).Returns(() => _entries.Count);
			_logFile.Setup(x => x.Wait(It.IsAny<TimeSpan>())).Returns(true);
		}

		[Test]
		[Description("Verifies that the search delivers correct results when the file is completely available before the search is started")]
		public void TestSearch1()
		{
			AddEntry("Hello World!");
			AddEntry("Foobar");

			using (var search = new LogFileSearch(_scheduler, _logFile.Object, "Foobar", TimeSpan.Zero))
			{
				var proxy = new LogFileSearchProxy(search);
				proxy.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue("because we should be able to search through the file in a few seconds");

				proxy.Matches.Should().Equal(new[]
					{
						new LogMatch(1, new LogLineMatch(0, 6))
					});
				proxy.Count.Should().Be(1);
			}
		}

		[Test]
		[Description("Verifies that the search delivers correct results when the file is modified while the search is performed")]
		public void TestSearch2()
		{
			using (var search = new LogFileSearch(_scheduler, _logFile.Object, "Foobar", TimeSpan.Zero))
			{
				var proxy = new LogFileSearchProxy(search);

				AddEntry("Hello World!");
				AddEntry("Foobar");
				proxy.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue("because we should be able to search through the file in a few seconds");

				proxy.Matches.Should().Equal(new[]
					{
						new LogMatch(1, new LogLineMatch(0, 6))
					});
				proxy.Count.Should().Be(1);
			}
		}

		private void AddEntry(string message)
		{
			var index = _entries.Count;
			var entry = new LogLine(index, index, message, LevelFlags.None);
			_entries.Add(entry);
			_listeners.OnRead(_entries.Count);
		}
	}
}