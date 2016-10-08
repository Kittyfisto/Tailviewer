using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;

namespace Tailviewer.Test.BusinessLogic.Searches
{
	[TestFixture]
	public sealed class LogFileSearchProxyAcceptanceTest
	{
		private Mock<ILogFile> _logFile;
		private LogFileListenerCollection _listeners;
		private List<LogLine> _entries;

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
					.Callback((ILogFileListener listener, TimeSpan unused, int max) =>
					{
						for (int i = 0; i < _entries.Count / max + 1; ++i)
						{
							int from = i * max;
							int to = Math.Min((i + 1) * max, _entries.Count);
							listener.OnLogFileModified(_logFile.Object, new LogFileSection(from, to - from));
						}
					});
			_logFile.Setup(x => x.GetLine(It.IsAny<int>())).Returns((int index) => _entries[index]);
			_logFile.Setup(x => x.Count).Returns(() => _entries.Count);
			_logFile.Setup(x => x.Wait(It.IsAny<TimeSpan>())).Returns(true);
		}

		[Test]
		public void TestSearch1()
		{
			AddEntry("Hello World!");
			AddEntry("Foobar");

			using (var search = new LogFileSearch(_logFile.Object, "Foobar", TimeSpan.Zero))
			{
				var proxy = new LogFileSearchProxy(search);
				proxy.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue("because we should be able to search through the file in a few seconds");

				proxy.Matches.Should().Equal(new[]
					{
						new LogMatch(1, new LogLineMatch(0, 6))
					});
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