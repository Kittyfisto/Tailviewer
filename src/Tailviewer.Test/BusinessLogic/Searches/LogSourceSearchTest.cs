using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core.Sources;

namespace Tailviewer.Test.BusinessLogic.Searches
{
	[TestFixture]
	public sealed class LogSourceSearchTest
	{
		private List<LogLine> _entries;
		private List<LogMatch> _matches;
		private Mock<ILogFileSearchListener> _listener;
		private ManualTaskScheduler _scheduler;

		[SetUp]
		public void SetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_entries = new List<LogLine>();

			_matches = new List<LogMatch>();
			_listener = new Mock<ILogFileSearchListener>();
			_listener.Setup(x => x.OnSearchModified(It.IsAny<ILogSourceSearch>(), It.IsAny<List<LogMatch>>()))
					 .Callback((ILogSourceSearch sender, IEnumerable<LogMatch> matches) =>
						 {
							 _matches.Clear();
							 _matches.AddRange(matches);
						 });
		}

		[Test]
		public void TestCtor1()
		{
			var logFile = new InMemoryLogSource();
			using (var search = new LogSourceSearch(_scheduler, logFile, "foobar", TimeSpan.Zero))
			{
				search.Matches.Should().BeEmpty("because the source is empty");
			}
		}

		[Test]
		public void TestDispose()
		{
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("What's a foobar?");
			LogSourceSearch search;
			using (search = new LogSourceSearch(_scheduler, logFile, "foobar", TimeSpan.Zero))
			{
				search.IsDisposed.Should().BeFalse();
				_scheduler.PeriodicTaskCount.Should().Be(1);

				_scheduler.RunOnce();
				search.Count.Should().Be(1);
			}

			search.IsDisposed.Should().BeTrue();
			search.Count.Should().Be(0);
			_scheduler.PeriodicTaskCount.Should().Be(0);
		}

		[Test]
		public void TestCtor2()
		{
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("Hello World!");
			using (var search = new LogSourceSearch(_scheduler, logFile, "l", TimeSpan.Zero))
			{
				_scheduler.RunOnce();

				search.Count.Should().Be(3);
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
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("Hello World!");
			using (var search = new LogSourceSearch(_scheduler, logFile, "l", TimeSpan.Zero))
			{
				search.AddListener(_listener.Object);

				_scheduler.RunOnce();

				_matches.Should().Equal(new[]
					{
						new LogMatch(0, new LogLineMatch(2, 1)),
						new LogMatch(0, new LogLineMatch(3, 1)),
						new LogMatch(0, new LogLineMatch(9, 1))
					});
			}
		}

		[Test]
		[Description("Verifies that the search handles a partial invalidation of the log source correctly")]
		public void TestInvalidate1()
		{
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("What's up people?");
			logFile.AddEntry("Hello World!");
			logFile.AddEntry("Looks like a bug");
			using (var search = new LogSourceSearch(_scheduler, logFile, "l", TimeSpan.Zero))
			{
				_scheduler.RunOnce();
				search.Matches.Should().HaveCount(6);

				logFile.RemoveFrom(1);
				search.Matches.Should().HaveCount(6);

				_scheduler.RunOnce();
				search.Matches.Should().HaveCount(1, "because we've removed the second and third line, thus removing 5 of the 6 hits in the log source");
				search.Matches.Should().Equal(new LogMatch(0, new LogLineMatch(14, 1)));
			}
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/288")]
		[Description("Verifies that the search handles a complete invalidation of the log source correctly")]
		public void TestInvalidate2()
		{
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("What's up people?");
			logFile.AddEntry("Hello World!");
			logFile.AddEntry("Looks like a bug");
			using (var search = new LogSourceSearch(_scheduler, logFile, "l", TimeSpan.Zero))
			{
				_scheduler.RunOnce();
				search.Matches.Should().HaveCount(6);

				logFile.RemoveFrom(0);
				search.Matches.Should().HaveCount(6);

				_scheduler.RunOnce();
				search.Matches.Should().HaveCount(0, "because we've removed all lines");
			}
		}

		[Test]
		[Description("Verifies that the search handles a Reset of the log source correctly")]
		public void TestReset()
		{
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("What's up people?");
			logFile.AddEntry("Hello World!");
			logFile.AddEntry("Looks like a bug");
			using (var search = new LogSourceSearch(_scheduler, logFile, "l", TimeSpan.Zero))
			{
				_scheduler.RunOnce();
				search.Matches.Should().HaveCount(6);

				logFile.Clear();
				search.Matches.Should().HaveCount(6);

				_scheduler.RunOnce();
				search.Matches.Should().HaveCount(0, "because we've cleared the log source");
			}
		}
	}
}