using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core.Properties;
using Tailviewer.Core.Sources;
using Tailviewer.Settings;
using Tailviewer.Test;

namespace Tailviewer.AcceptanceTests.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class FileDataSourceTest
	{
		[SetUp]
		public void SetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logSourceFactory = new SimplePluginLogSourceFactory(_scheduler);
		}

		private ManualTaskScheduler _scheduler;
		private ILogSourceFactory _logSourceFactory;

		[Test]
		public void TestConstruction1()
		{
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, new DataSource(@"E:\somelogfile.txt") { Id = DataSourceId.CreateNew() }))
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
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, settings))
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
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, settings))
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
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, settings))
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
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, settings))
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
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, settings))
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
			LogSourceProxy permanentLogSource;
			LogSourceSearchProxy permanentSearch;

			LogSourceProxy permanentFindAllLogSource;
			LogSourceSearchProxy permanentFindAllSearch;

			FileDataSource source;
			using (source = new FileDataSource(_logSourceFactory, _scheduler, new DataSource(@"E:\somelogfile.txt") {Id = DataSourceId.CreateNew()}))
			{
				permanentLogSource = (LogSourceProxy) source.FilteredLogSource;
				permanentLogSource.IsDisposed.Should().BeFalse();

				permanentSearch = (LogSourceSearchProxy) source.Search;
				permanentSearch.IsDisposed.Should().BeFalse();

				permanentFindAllLogSource = (LogSourceProxy) source.FindAllLogSource;
				permanentFindAllLogSource.IsDisposed.Should().BeFalse();

				permanentFindAllSearch = (LogSourceSearchProxy) source.FindAllSearch;
				permanentFindAllSearch.IsDisposed.Should().BeFalse();
			}
			source.IsDisposed.Should().BeTrue();
			permanentLogSource.IsDisposed.Should().BeTrue();
			permanentSearch.IsDisposed.Should().BeTrue();
			permanentFindAllLogSource.IsDisposed.Should().BeTrue();
			permanentFindAllSearch.IsDisposed.Should().BeTrue();
		}

		[Test]
		[Description("Verifies that the data source stops all periodic tasks upon being disposed of")]
		public void TestDispose2()
		{
			FileDataSource source = new FileDataSource(_logSourceFactory, _scheduler,
				new DataSource(@"E:\somelogfile.txt") {Id = DataSourceId.CreateNew()});
			_scheduler.PeriodicTaskCount.Should().BeGreaterThan(0);
			source.Dispose();
			_scheduler.PeriodicTaskCount.Should().Be(0, "because all tasks should've been removed");
		}

		[Test]
		public void TestSearch1()
		{
			var logFile = new InMemoryLogSource();
			using (var dataSource = new FileDataSource(_scheduler, CreateDataSource(), logFile, TimeSpan.Zero))
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
			var logFile = new InMemoryLogSource();
			var settings = CreateDataSource();
			using (var dataSource = new FileDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
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
			var logFile = new InMemoryLogSource();
			var settings = CreateDataSource();
			using (var dataSource = new FileDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
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
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("Foo");
			logFile.AddEntry("Bar");
			using (var dataSource = new FileDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				_scheduler.Run(3);
				dataSource.FilteredLogSource.GetProperty(GeneralProperties.LogEntryCount).Should().Be(2);

				dataSource.ClearScreen();
				_scheduler.Run(3);
				dataSource.FilteredLogSource.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0, "because we've just cleared the screen");

				logFile.AddEntry("Hello!");
				_scheduler.Run(3);
				dataSource.FilteredLogSource.GetProperty(GeneralProperties.LogEntryCount).Should().Be(1, "because newer log entries should still appear");
				dataSource.FilteredLogSource.GetEntry(0).RawContent.Should().Be("Hello!");
			}
		}

		[Test]
		[Description("Verifies that ShowAll() shows all entries of the log file again")]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/215")]
		public void TestClearScreenShowAll()
		{
			var settings = CreateDataSource();
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("Foo");
			logFile.AddEntry("Bar");
			using (var dataSource = new FileDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				_scheduler.RunOnce();

				dataSource.ClearScreen();
				_scheduler.RunOnce();
				dataSource.FilteredLogSource.GetProperty(GeneralProperties.LogEntryCount).Should().Be(0, "because we've just cleared the screen");

				dataSource.ShowAll();
				_scheduler.RunOnce();
				dataSource.FilteredLogSource.GetProperty(GeneralProperties.LogEntryCount).Should().Be(2, "because we've just shown everything again");
			}
		}

		private DataSource CreateDataSource()
		{
			return new DataSource("ffff") {Id = DataSourceId.CreateNew()};
		}
	}
}