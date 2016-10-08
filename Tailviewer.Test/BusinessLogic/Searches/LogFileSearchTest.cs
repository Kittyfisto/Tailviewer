using System;
using System.Collections.Generic;
using System.Linq;
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
	public sealed class LogFileSearchTest
		: AbstractTest
	{
		private List<LogLine> _entries;
		private Mock<ILogFile> _logFile;
		private List<LogMatch> _matches;
		private Mock<ILogFileSearchListener> _listener;
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
		public void SetUp()
		{
			_entries = new List<LogLine>();
			_logFile = new Mock<ILogFile>();
			_logFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
					.Callback(
						(LogFileSection section, LogLine[] entries) =>
						_entries.CopyTo((int)section.Index, entries, 0, section.Count));
			_logFile.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			        .Callback((ILogFileListener listener, TimeSpan unused, int max) =>
				        {
					        for (int i = 0; i < _entries.Count/max+1; ++i)
					        {
						        int from = i*max;
						        int to = Math.Min((i + 1)*max, _entries.Count);
						        listener.OnLogFileModified(_logFile.Object, new LogFileSection(from, to - from));
					        }
				        });
			_logFile.Setup(x => x.GetLine(It.IsAny<int>())).Returns((int index) => _entries[index]);
			_logFile.Setup(x => x.Count).Returns(() => _entries.Count);
			_logFile.Setup(x => x.Wait(It.IsAny<TimeSpan>())).Returns(true);

			_matches = new List<LogMatch>();
			_listener = new Mock<ILogFileSearchListener>();
			_listener.Setup(x => x.OnSearchModified(It.IsAny<ILogFileSearch>(), It.IsAny<List<LogMatch>>()))
					 .Callback((ILogFileSearch sender, List<LogMatch> matches) =>
						 {
							 _matches.Clear();
							 _matches.AddRange(matches);
						 });
		}

		[Test]
		public void TestCtor1()
		{
			using (var search = new LogFileSearch(_scheduler, _logFile.Object, "foobar"))
			{
				search.Matches.Should().BeEmpty("because the source is empty");
			}
		}

		[Test]
		public void TestCtor2()
		{
			Add("Hello World!");
			using (var search = new LogFileSearch(_scheduler, _logFile.Object, "l"))
			{
				WaitUntil(() => search.Count >= 3, TimeSpan.FromSeconds(5)).Should().BeTrue();
				var matches = search.Matches.ToList();
				matches.Should().Equal(new[]
					{
						new LogMatch(0, new LogLineMatch(2, 1)),
						new LogMatch(0, new LogLineMatch(3, 1)),
						new LogMatch(0, new LogLineMatch(9, 1))
					});
			}
		}

		[Test]
		public void TestAddListener1()
		{
			Add("Hello World!");
			using (var search = new LogFileSearch(_scheduler, _logFile.Object, "l"))
			{
				search.AddListener(_listener.Object);
				WaitUntil(() => search.Count >= 3, TimeSpan.FromSeconds(5)).Should().BeTrue();
				_matches.Should().Equal(new[]
					{
						new LogMatch(0, new LogLineMatch(2, 1)),
						new LogMatch(0, new LogLineMatch(3, 1)),
						new LogMatch(0, new LogLineMatch(9, 1))
					});
			}
		}

		private void Add(string message)
		{
			var index = _entries.Count;
			var entry = new LogLine(index, index, message, LevelFlags.None);
			_entries.Add(entry);
		}
	}
}