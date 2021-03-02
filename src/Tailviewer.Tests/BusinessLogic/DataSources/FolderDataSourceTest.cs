using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Api.Tests;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core;
using Tailviewer.Settings;

namespace Tailviewer.Tests.BusinessLogic.DataSources
{
	[TestFixture]
	[Issue("https://github.com/Kittyfisto/Tailviewer/issues/125")]
	public sealed class FolderDataSourceTest
	{
		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new ManualTaskScheduler();
			_logSourceFactory = new SimplePluginLogSourceFactory(_taskScheduler);
			_filesystem = new InMemoryFilesystem();
			_settings = new DataSource
			{
				Id = DataSourceId.CreateNew(),
				MergedDataSourceDisplayMode = DataSourceDisplayMode.CharacterCode
			};
		}

		private ManualTaskScheduler _taskScheduler;
		private DataSource _settings;
		private ILogSourceFactory _logSourceFactory;
		private InMemoryFilesystem _filesystem;

		[Test]
		public void TearDown()
		{
		}

		[Test]
		public void TestChange([Values(null, "", @"C:\temp")] string folderPath,
		                       [Values(null, "", "*.log")] string logFileRegex,
		                       [Values(true, false)] bool recursive)
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);
			dataSource.Change(folderPath, logFileRegex, recursive);
			dataSource.LogFileFolderPath.Should().Be(folderPath);
			dataSource.LogFileSearchPattern.Should().Be(logFileRegex);
			dataSource.Recursive.Should().Be(recursive);

			_settings.LogFileFolderPath.Should().Be(folderPath);
			_settings.LogFileSearchPattern.Should().Be(logFileRegex);
			_settings.Recursive.Should().Be(recursive);
		}

		[Test]
		public void TestNoSuchFolder()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);
			var path = Path.Combine(_filesystem.Roots.First().FullName, "logs");
			dataSource.Change(path, "*", false);

			_taskScheduler.RunOnce();
			dataSource.OriginalSources.Should().BeEmpty();
		}

		[Test]
		public void TestNoSuchDrive()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var path = Path.Combine("Z:\\", "logs");
			dataSource.Change(path, "*", false);

			dataSource.OriginalSources.Should().BeEmpty();
		}

		[Test]
		public void TestEmptyFolder()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var path = Path.Combine(_filesystem.Roots.First().FullName, "logs");
			_filesystem.CreateDirectory(path);

			dataSource.Change(path, "*", false);

			dataSource.OriginalSources.Should().BeEmpty();
		}

		[Test]
		public void TestOneMatchingLogFile()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var path = Path.Combine(_filesystem.Roots.First().FullName, "logs");
			_filesystem.CreateDirectory(path);
			_filesystem.WriteAllBytes(Path.Combine(path, "foo.log"), new byte[0]);
			_filesystem.WriteAllBytes(Path.Combine(path, "foo.txt"), new byte[0]);

			dataSource.Change(path, "*.log", false);
			_taskScheduler.RunOnce();

			dataSource.Property(x => (IEnumerable<IDataSource>)x.OriginalSources).ShouldEventually().HaveCount(1);
			var child = dataSource.OriginalSources[0];
			child.FullFileName.Should().Be(Path.Combine(path, "foo.log"));
		}

		[Test]
		public void TestMultiplePatterns()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var path = Path.Combine(_filesystem.Roots.First().FullName, "logs");
			_filesystem.CreateDirectory(path);
			_filesystem.WriteAllBytes(Path.Combine(path, "foo.log"), new byte[0]);
			_filesystem.WriteAllBytes(Path.Combine(path, "foo.bar"), new byte[0]);
			_filesystem.WriteAllBytes(Path.Combine(path, "foo.txt"), new byte[0]);

			dataSource.Change(path, "*.log;*.txt", false);
			_taskScheduler.RunOnce();

			dataSource.Property(x => (IEnumerable<IDataSource>)x.OriginalSources).ShouldEventually().HaveCount(2);
			dataSource.OriginalSources[0].FullFileName.Should().Be(Path.Combine(path, "foo.log"));
			dataSource.OriginalSources[1].FullFileName.Should().Be(Path.Combine(path, "foo.txt"));
		}

		[Test]
		public void TestTooManySources()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var path = Path.Combine(_filesystem.Roots.First().FullName, "logs");
			_filesystem.CreateDirectory(path);
			for (int i = 0; i < 257; ++i)
			{
				_filesystem.WriteAllBytes(Path.Combine(path, $"{i}.txt"), new byte[0]);
			}

			dataSource.Change(path, "*.log;*.txt", false);
			_taskScheduler.RunOnce();

			dataSource.Property(x => (IEnumerable<IDataSource>)x.OriginalSources).ShouldEventually().HaveCount(255,
			                                                                                                   "because merged log file cannot merge more than 256 logs");
			dataSource.UnfilteredFileCount.Should().Be(257);
			dataSource.FilteredFileCount.Should().Be(257);
		}

		[Test]
		[Description("Verifies that the FolderDataSource always exposes the same Unfiltered- and FilteredLogFile object")]
		public void TestChangeFilter()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var unfiltered = dataSource.UnfilteredLogSource;
			var filtered = dataSource.FilteredLogSource;

			dataSource.QuickFilterChain = new List<ILogEntryFilter>();
			dataSource.UnfilteredLogSource.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogSource.Should().BeSameAs(filtered);

			dataSource.QuickFilterChain = null;
			dataSource.UnfilteredLogSource.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogSource.Should().BeSameAs(filtered);
		}

		[Test]
		[Description("Verifies that the FolderDataSource always exposes the same Unfiltered- and FilteredLogFile object")]
		public void TestChangeSingleLine()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var unfiltered = dataSource.UnfilteredLogSource;
			var filtered = dataSource.FilteredLogSource;

			dataSource.IsSingleLine = !dataSource.IsSingleLine;
			dataSource.UnfilteredLogSource.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogSource.Should().BeSameAs(filtered);

			dataSource.IsSingleLine = !dataSource.IsSingleLine;
			dataSource.UnfilteredLogSource.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogSource.Should().BeSameAs(filtered);
		}

		[Test]
		[Description("Verifies that the FolderDataSource always exposes the same Unfiltered- and FilteredLogFile object")]
		public void TestChangeLevelFilter()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var unfiltered = dataSource.UnfilteredLogSource;
			var filtered = dataSource.FilteredLogSource;

			dataSource.LevelFilter = LevelFlags.Error;
			dataSource.UnfilteredLogSource.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogSource.Should().BeSameAs(filtered);

			dataSource.LevelFilter = LevelFlags.All;
			dataSource.UnfilteredLogSource.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogSource.Should().BeSameAs(filtered);
		}

		[Test]
		public void TestDisposeFilesystemWatcher()
		{
			var filesystem = new Mock<IFilesystem>();
			var watchdog = new Mock<IFilesystemWatchdog>();
			var watchers = new List<Mock<IFilesystemWatcher>>();
			watchdog.Setup(x => x.StartDirectoryWatch(It.IsAny<string>(),
			                                          It.IsAny<TimeSpan>(),
			                                          It.IsAny<string>(),
			                                          It.IsAny<SearchOption>()))
			        .Returns(() =>
			        {
				        var watcher = new Mock<IFilesystemWatcher>();
				        watchers.Add(watcher);
				        return watcher.Object;
			        });
			filesystem.Setup(x => x.Watchdog).Returns(watchdog.Object);

			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      filesystem.Object,
			                                      _settings,
			                                      TimeSpan.Zero);
			watchers.Should().HaveCount(1, "because the data source should've created one filesystem watcher");

			dataSource.Dispose();
			watchers.Should().HaveCount(1, "because the data source should not have created anymore watchers");
			watchers[0].Verify(x => x.Dispose(), Times.Once, "because the data source should've disposed of the watcher");
		}

		[Test]
		public void TestDisposeChildDataSources()
		{
			var path = Path.Combine(_filesystem.Roots.First().FullName, "logs");
			_settings.LogFileFolderPath = path;
			_settings.LogFileSearchPattern = "*.log";

			var folderDataSource = new FolderDataSource(_taskScheduler,
			                                      _logSourceFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			_filesystem.CreateDirectory(path);
			_filesystem.WriteAllBytes(Path.Combine(path, "foo.log"), new byte[0]);
			_taskScheduler.RunOnce();

			var children = folderDataSource.OriginalSources;
			children.Should().HaveCount(1);
			var childDataSource = children[0];
			childDataSource.IsDisposed.Should().BeFalse();

			folderDataSource.Dispose();
			childDataSource.IsDisposed.Should().BeTrue("because the folder data source should dispose of its children upon being disposed of itself");
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/215")]
		public void TestClearAllShowAll()
		{
			var folderDataSource = new FolderDataSource(_taskScheduler,
			                                            _logSourceFactory,
			                                            _filesystem,
			                                            _settings,
			                                            TimeSpan.Zero);
			var merged = GetInnerDataSource(folderDataSource);

			folderDataSource.ScreenCleared.Should().BeFalse();
			folderDataSource.ClearScreen();
			folderDataSource.ScreenCleared.Should().BeTrue();
			merged.ScreenCleared.Should().BeTrue();

			folderDataSource.ShowAll();
			folderDataSource.ScreenCleared.Should().BeFalse();
			merged.ScreenCleared.Should().BeFalse();
		}

		private static IDataSource GetInnerDataSource(FolderDataSource folderDataSource)
		{
			var inner = (IDataSource)folderDataSource.GetType().GetField("_mergedDataSource", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(folderDataSource);
			inner.Should().NotBeNull();
			return inner;
		}
	}
}