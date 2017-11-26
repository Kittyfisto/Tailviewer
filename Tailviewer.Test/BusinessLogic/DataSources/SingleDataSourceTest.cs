using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Test.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class SingleDataSourceTest
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new PluginLogFileFactory(_scheduler);
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

		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
		}

		private Mock<ILogFile> _logFile;
		private LogFileListenerCollection _listeners;
		private List<LogLine> _entries;
		private ManualTaskScheduler _scheduler;
		private ILogFileFactory _logFileFactory;

		[Test]
		public void TestConstruction1()
		{
			using (var source = new SingleDataSource(_logFileFactory, _scheduler, new DataSource(@"E:\somelogfile.txt") { Id = DataSourceId.CreateNew() }))
			{
				source.FullFileName.Should().Be(@"E:\somelogfile.txt");
				source.LevelFilter.Should().Be(LevelFlags.All);
				source.SearchTerm.Should().BeNull();
				source.FollowTail.Should().BeFalse();
			}
		}

		[Test]
		public void TestConstruction2()
		{
			var settings = new DataSource(@"E:\somelogfile.txt")
			{
				Id = DataSourceId.CreateNew(),
				SelectedLogLines = new HashSet<LogLineIndex> {1, 2}
			};
			using (var source = new SingleDataSource(_logFileFactory, _scheduler, settings))
			{
				source.SelectedLogLines.Should().BeEquivalentTo(new LogLineIndex[] {1, 2});
			}
		}

		[Test]
		[Description("Verifies that the data source disposes of all of its resources")]
		public void TestDispose1()
		{
			LogFileProxy permanentLogFile;
			LogFileSearchProxy permanentSearch;

			SingleDataSource source;
			using (source = new SingleDataSource(_logFileFactory, _scheduler, new DataSource(@"E:\somelogfile.txt") {Id = DataSourceId.CreateNew()}))
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
		[Description("Verifies that the data source stops all periodic tasks upon being disposed of")]
		public void TestDispose2()
		{
			SingleDataSource source = new SingleDataSource(_logFileFactory, _scheduler,
				new DataSource(@"E:\somelogfile.txt") {Id = DataSourceId.CreateNew()});
			_scheduler.PeriodicTaskCount.Should().BeGreaterThan(0);
			source.Dispose();
			_scheduler.PeriodicTaskCount.Should().Be(0, "because all tasks should've been removed");
		}

		[Test]
		[Description("Verifies that the levels are counted correctly")]
		public void TestLevelCount1()
		{
			using (var dataSource = new SingleDataSource(_logFileFactory, _scheduler, new DataSource(@"TestData\LevelCounts.txt") { Id = DataSourceId.CreateNew() }))
			{
				_scheduler.Run(2);
				dataSource.UnfilteredLogFile.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();

				dataSource.TotalCount.Should().Be(27, "because the data source contains that many lines");
				dataSource.DebugCount.Should().Be(1, "because the data source contains one debug line");
				dataSource.InfoCount.Should().Be(2, "because the data source contains two info lines");
				dataSource.WarningCount.Should().Be(3, "because the data source contains three warnings");
				dataSource.ErrorCount.Should().Be(4, "because the data source contains four errors");
				dataSource.FatalCount.Should().Be(5, "because the data source contains five fatal lines");
				dataSource.TraceCount.Should().Be(6, "because the data source contains six trace lines");
				dataSource.NoLevelCount.Should().Be(0, "because all non-matching lines are assumed to belong to the previous line");
			}
		}

		[Test]
		[Description("Verifies that the level of a log line is unambigously defined")]
		public void TestLevelPrecedence()
		{
			using (var dataSource = new SingleDataSource(_logFileFactory, _scheduler, new DataSource(@"TestData\DifferentLevels.txt") { Id = DataSourceId.CreateNew() }))
			{
				_scheduler.Run(count: 3);

				dataSource.FilteredLogFile.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue();
				dataSource.FilteredLogFile.Count.Should().Be(6);

				LogLine[] lines = dataSource.FilteredLogFile.GetSection(new LogFileSection(0, 6));
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

				dataSource.HideEmptyLines = false;
				settings.HideEmptyLines.Should().BeFalse("because the data source should modify the settings object when changed");
			}
		}

		[Test]
		public void TestIsSingleLine()
		{
			var settings = CreateDataSource();
			using (var dataSource = new SingleDataSource(_scheduler, settings, _logFile.Object, TimeSpan.Zero))
			{
				dataSource.IsSingleLine.Should().BeFalse();
				dataSource.IsSingleLine = true;
				settings.IsSingleLine.Should().BeTrue("because the data source should modify the settings object when changed");

				dataSource.IsSingleLine = false;
				settings.IsSingleLine.Should().BeFalse("because the data source should modify the settings object when changed");
			}
		}

		[Test]
		public void TestAnalyse()
		{
			var settings = CreateDataSource();
			using (var dataSource = new SingleDataSource(_scheduler, settings, _logFile.Object, TimeSpan.Zero))
			{
				var analysisId = AnalysisId.CreateNew();
				dataSource.IsAnalysisActive(analysisId).Should().BeFalse("because the file isn't part of any analysis");

				dataSource.EnableAnalysis(analysisId);
				dataSource.IsAnalysisActive(analysisId).Should().BeTrue("because we've just activated that");

				dataSource.DisableAnalysis(analysisId);
				dataSource.IsAnalysisActive(analysisId).Should().BeFalse("because we've just disabled that");
			}
		}

		private DataSource CreateDataSource()
		{
			return new DataSource("ffff") {Id = DataSourceId.CreateNew()};
		}
	}
}