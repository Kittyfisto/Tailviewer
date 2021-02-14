﻿using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Test.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class SingleDataSourceTest
	{
		[SetUp]
		public void SetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new SimplePluginLogFileFactory(_scheduler);
		}

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
		public void TestConstruction3([Values(true, false)] bool showDeltaTimes)
		{
			var settings = new DataSource(@"E:\somelogfile.txt")
			{
				Id = DataSourceId.CreateNew(),
				ShowDeltaTimes = showDeltaTimes
			};
			using (var source = new SingleDataSource(_logFileFactory, _scheduler, settings))
			{
				source.ShowDeltaTimes.Should().Be(showDeltaTimes);
			}
		}
		
		[Test]
		public void TestConstruction4([Values(true, false)] bool showElapsedTime)
		{
			var settings = new DataSource(@"E:\somelogfile.txt")
			{
				Id = DataSourceId.CreateNew(),
				ShowElapsedTime = showElapsedTime
			};
			using (var source = new SingleDataSource(_logFileFactory, _scheduler, settings))
			{
				source.ShowElapsedTime.Should().Be(showElapsedTime);
			}
		}

		[Test]
		public void TestChangeShowElapsedTime([Values(true, false)] bool showElapsedTime)
		{
			var settings = new DataSource(@"E:\somelogfile.txt")
			{
				Id = DataSourceId.CreateNew()
			};
			using (var source = new SingleDataSource(_logFileFactory, _scheduler, settings))
			{
				source.ShowElapsedTime = showElapsedTime;
				settings.ShowElapsedTime.Should().Be(showElapsedTime);

				source.ShowElapsedTime = !showElapsedTime;
				settings.ShowElapsedTime.Should().Be(!showElapsedTime);
			}
		}

		[Test]
		public void TestChangeShowDeltaTimes([Values(true, false)] bool showDeltaTimes)
		{
			var settings = new DataSource(@"E:\somelogfile.txt")
			{
				Id = DataSourceId.CreateNew()
			};
			using (var source = new SingleDataSource(_logFileFactory, _scheduler, settings))
			{
				source.ShowDeltaTimes = showDeltaTimes;
				settings.ShowDeltaTimes.Should().Be(showDeltaTimes);

				source.ShowDeltaTimes = !showDeltaTimes;
				settings.ShowDeltaTimes.Should().Be(!showDeltaTimes);
			}
		}

		[Test]
		[Description("Verifies that the data source disposes of all of its resources")]
		public void TestDispose1()
		{
			LogFileProxy permanentLogFile;
			LogFileSearchProxy permanentSearch;

			LogFileProxy permanentFindAllLogFile;
			LogFileSearchProxy permanentFindAllSearch;

			SingleDataSource source;
			using (source = new SingleDataSource(_logFileFactory, _scheduler, new DataSource(@"E:\somelogfile.txt") {Id = DataSourceId.CreateNew()}))
			{
				permanentLogFile = (LogFileProxy) source.FilteredLogFile;
				permanentLogFile.IsDisposed.Should().BeFalse();

				permanentSearch = (LogFileSearchProxy) source.Search;
				permanentSearch.IsDisposed.Should().BeFalse();

				permanentFindAllLogFile = (LogFileProxy) source.FindAllLogFile;
				permanentFindAllLogFile.IsDisposed.Should().BeFalse();

				permanentFindAllSearch = (LogFileSearchProxy) source.FindAllSearch;
				permanentFindAllSearch.IsDisposed.Should().BeFalse();
			}
			source.IsDisposed.Should().BeTrue();
			permanentLogFile.IsDisposed.Should().BeTrue();
			permanentSearch.IsDisposed.Should().BeTrue();
			permanentFindAllLogFile.IsDisposed.Should().BeTrue();
			permanentFindAllSearch.IsDisposed.Should().BeTrue();
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
				dataSource.UnfilteredLogFile.Property(x => x.GetValue(LogFileProperties.PercentageProcessed)).ShouldEventually().Be(Percentage.HundredPercent);

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
		public void TestSearch1()
		{
			var logFile = new InMemoryLogFile();
			using (var dataSource = new SingleDataSource(_scheduler, CreateDataSource(), logFile, TimeSpan.Zero))
			{
				logFile.AddEntry("Hello foobar world!");
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
			var logFile = new InMemoryLogFile();
			var settings = CreateDataSource();
			using (var dataSource = new SingleDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
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
			var logFile = new InMemoryLogFile();
			var settings = CreateDataSource();
			using (var dataSource = new SingleDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				dataSource.IsSingleLine.Should().BeFalse();
				dataSource.IsSingleLine = true;
				settings.IsSingleLine.Should().BeTrue("because the data source should modify the settings object when changed");

				dataSource.IsSingleLine = false;
				settings.IsSingleLine.Should().BeFalse("because the data source should modify the settings object when changed");
			}
		}

		[Test]
		[Description("Verifies that ClearScreen() filters all entries of the log file")]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/215")]
		public void TestClearScreen()
		{
			var settings = CreateDataSource();
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Foo");
			logFile.AddEntry("Bar");
			using (var dataSource = new SingleDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				_scheduler.Run(3);
				dataSource.FilteredLogFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(2);

				dataSource.ClearScreen();
				_scheduler.Run(3);
				dataSource.FilteredLogFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(0, "because we've just cleared the screen");

				logFile.AddEntry("Hello!");
				_scheduler.Run(3);
				dataSource.FilteredLogFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(1, "because newer log entries should still appear");
				dataSource.FilteredLogFile.GetEntry(0).RawContent.Should().Be("Hello!");
			}
		}

		[Test]
		[Description("Verifies that ShowAll() shows all entries of the log file again")]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/215")]
		public void TestClearScreenShowAll()
		{
			var settings = CreateDataSource();
			var logFile = new InMemoryLogFile();
			logFile.AddEntry("Foo");
			logFile.AddEntry("Bar");
			using (var dataSource = new SingleDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				_scheduler.RunOnce();

				dataSource.ClearScreen();
				_scheduler.RunOnce();
				dataSource.FilteredLogFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(0, "because we've just cleared the screen");

				dataSource.ShowAll();
				_scheduler.RunOnce();
				dataSource.FilteredLogFile.GetValue(LogFileProperties.LogEntryCount).Should().Be(2, "because we've just shown everything again");
			}
		}

		private DataSource CreateDataSource()
		{
			return new DataSource("ffff") {Id = DataSourceId.CreateNew()};
		}
	}
}