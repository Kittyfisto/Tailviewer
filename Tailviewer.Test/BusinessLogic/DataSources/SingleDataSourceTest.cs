using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Settings;

namespace Tailviewer.Test.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class SingleDataSourceTest
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_scheduler = new ManualTaskScheduler();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
		}

		private Mock<ILogFile> _logFile;
		private LogFileListenerCollection _listeners;
		private List<LogLine> _entries;
		private ManualTaskScheduler _scheduler;

		[Test]
		public void TestCtor()
		{
			using (var source = new SingleDataSource(_scheduler, new DataSource(@"E:\somelogfile.txt") { Id = Guid.NewGuid() }))
			{
				source.FullFileName.Should().Be(@"E:\somelogfile.txt");
				source.LevelFilter.Should().Be(LevelFlags.All);
				source.SearchTerm.Should().BeNull();
				source.FollowTail.Should().BeFalse();
			}
		}

		[Test]
		[Description("Verifies that the data source disposes of all of its resources")]
		public void TestDispose()
		{
			LogFileProxy permanentLogFile;
			LogFileSearchProxy permanentSearch;

			SingleDataSource source;
			using (source = new SingleDataSource(_scheduler, new DataSource(@"E:\somelogfile.txt") {Id = Guid.NewGuid()}))
			{
				permanentLogFile = (LogFileProxy) source.FilteredLogFile;
				permanentSearch = (LogFileSearchProxy) source.Search;

				permanentLogFile.IsDisposed.Should().BeFalse();
				permanentSearch.IsDisposed.Should().BeFalse();
			}
			source.IsDisposed.Should().BeTrue();
			permanentLogFile.IsDisposed.Should().BeTrue();
			permanentSearch.IsDisposed.Should().BeTrue();
		}

		[Test]
		[Description("Verifies that the levels are counted correctly")]
		public void TestLevelCount1()
		{
			using (var dataSource = new SingleDataSource(_scheduler, new DataSource(@"TestData\LevelCounts.txt") { Id = Guid.NewGuid() }))
			{
				_scheduler.RunOnce();
				dataSource.UnfilteredLogFile.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();

				dataSource.TotalCount.Should().Be(21);
				dataSource.DebugCount.Should().Be(1);
				dataSource.InfoCount.Should().Be(2);
				dataSource.WarningCount.Should().Be(3);
				dataSource.ErrorCount.Should().Be(4);
				dataSource.FatalCount.Should().Be(5);
				dataSource.NoLevelCount.Should().Be(0);
			}
		}

		[Test]
		[Description("Verifies that the level of a log line is unambigously defined")]
		public void TestLevelPrecedence()
		{
			using (var dataSource = new SingleDataSource(_scheduler, new DataSource(@"TestData\DifferentLevels.txt") { Id = Guid.NewGuid() }))
			{
				_scheduler.RunOnce();
				dataSource.UnfilteredLogFile.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();
				dataSource.UnfilteredLogFile.Count.Should().Be(6);
				LogLine[] lines = dataSource.UnfilteredLogFile.GetSection(new LogFileSection(0, 6));
				lines[0].Message.Should().Be("DEBUG ERROR WARN FATAL INFO");
				lines[0].Level.Should().Be(LevelFlags.Debug, "Because DEBUG is the first level to appear in the line");

				lines[1].Message.Should().Be("INFO DEBUG ERROR WARN FATAL");
				lines[1].Level.Should().Be(LevelFlags.Info, "Because INFO is the first level to appear in the line");

				lines[2].Message.Should().Be("WARN ERROR FATAL INFO DEBUG");
				lines[2].Level.Should().Be(LevelFlags.Warning, "Because WARN is the first level to appear in the line");

				lines[3].Message.Should().Be("ERROR INFO DEBUG FATAL WARN");
				lines[3].Level.Should().Be(LevelFlags.Error, "Because ERROR is the first level to appear in the line");

				lines[4].Message.Should().Be("FATAL ERROR INFO WARN DEBUG");
				lines[4].Level.Should().Be(LevelFlags.Fatal, "Because FATAL is the first level to appear in the line");

				lines[5].Message.Should().Be("fatal error info warn debug");
				lines[5].Level.Should()
				        .Be(LevelFlags.Fatal,
				            "Because this line belongs to the previous log entry and thus is marked as fatal as well");
				lines[5].LogEntryIndex.Should().Be(lines[4].LogEntryIndex);

				dataSource.DebugCount.Should().Be(1);
				dataSource.InfoCount.Should().Be(1);
				dataSource.WarningCount.Should().Be(1);
				dataSource.ErrorCount.Should().Be(1);
				dataSource.FatalCount.Should().Be(1);
				dataSource.NoLevelCount.Should().Be(0);
			}
		}

		[SetUp]
		public void SetUp()
		{
			_logFile = new Mock<ILogFile>();
			_entries = new List<LogLine>();
			_listeners = new LogFileListenerCollection(_logFile.Object);
			_logFile.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
					.Callback((ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount) => _listeners.AddListener(listener, maximumWaitTime, maximumLineCount));
			_logFile.Setup(x => x.RemoveListener(It.IsAny<ILogFileListener>()))
					.Callback((ILogFileListener listener) => _listeners.RemoveListener(listener));
			_logFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
					.Callback(
						(LogFileSection section, LogLine[] entries) =>
						_entries.CopyTo((int)section.Index, entries, 0, section.Count));
			_logFile.Setup(x => x.GetLine(It.IsAny<int>())).Returns((int index) => _entries[index]);
			_logFile.Setup(x => x.Count).Returns(() => _entries.Count);
			_logFile.Setup(x => x.EndOfSourceReached).Returns(true);
		}

		[Test]
		public void TestSearch1()
		{
			using (var dataSource = new SingleDataSource(_scheduler, CreateDataSource(), _logFile.Object, TimeSpan.Zero))
			{
				_entries.Add(new LogLine(0, 0, "Hello foobar world!", LevelFlags.None));
				_listeners.OnRead(1);
				_scheduler.RunOnce();
				dataSource.SearchTerm = "foobar";
				_scheduler.Run(10);
				dataSource.Search.Count.Should().Be(1);
				var matches = dataSource.Search.Matches.ToList();
				matches.Should().Equal(new LogMatch(0, new LogLineMatch(6, 6)));
			}
		}

		[Test]
		public void TestHideEmptyLines1()
		{
			var settings = CreateDataSource();
			using (var dataSource = new SingleDataSource(_scheduler, settings, _logFile.Object, TimeSpan.Zero))
			{
				dataSource.HideEmptyLines.Should().BeFalse();
				dataSource.HideEmptyLines = true;
				settings.HideEmptyLines.Should().BeTrue("because the data source should modify the settings object when changed");
			}
		}

		private DataSource CreateDataSource()
		{
			return new DataSource("ffff") {Id = Guid.NewGuid()};
		}
	}
}