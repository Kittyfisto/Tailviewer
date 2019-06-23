using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Filters;
using Tailviewer.Settings;

namespace Tailviewer.Test.BusinessLogic.DataSources
{
	[TestFixture]
	[Issue("https://github.com/Kittyfisto/Tailviewer/issues/125")]
	public sealed class FolderDataSourceTest
	{
		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new ManualTaskScheduler();
			_logFileFactory = new PluginLogFileFactory(_taskScheduler);
			_filesystem = new InMemoryFilesystem();
			_settings = new DataSource
			{
				Id = DataSourceId.CreateNew(),
				MergedDataSourceDisplayMode = DataSourceDisplayMode.CharacterCode
			};
		}

		private ManualTaskScheduler _taskScheduler;
		private DataSource _settings;
		private ILogFileFactory _logFileFactory;
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
			                                      _logFileFactory,
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
			                                      _logFileFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);
			var path = Path.Combine(_filesystem.Roots.Result.First().FullName, "logs");
			dataSource.Change(path, "*", false);

			_taskScheduler.RunOnce();
			dataSource.OriginalSources.Should().BeEmpty();
		}

		[Test]
		public void TestNoSuchDrive()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logFileFactory,
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
			                                      _logFileFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var path = Path.Combine(_filesystem.Roots.Result.First().FullName, "logs");
			_filesystem.CreateDirectory(path).Wait();

			dataSource.Change(path, "*", false);

			dataSource.OriginalSources.Should().BeEmpty();
		}

		[Test]
		public void TestOneMatchingLogFile()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logFileFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var path = Path.Combine(_filesystem.Roots.Result.First().FullName, "logs");
			_filesystem.CreateDirectory(path);
			_filesystem.WriteAllBytes(Path.Combine(path, "foo.log"), new byte[0]);
			_filesystem.WriteAllBytes(Path.Combine(path, "foo.txt"), new byte[0]).Wait();

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
			                                      _logFileFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var path = Path.Combine(_filesystem.Roots.Result.First().FullName, "logs");
			_filesystem.CreateDirectory(path);
			_filesystem.WriteAllBytes(Path.Combine(path, "foo.log"), new byte[0]);
			_filesystem.WriteAllBytes(Path.Combine(path, "foo.bar"), new byte[0]);
			_filesystem.WriteAllBytes(Path.Combine(path, "foo.txt"), new byte[0]).Wait();

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
			                                      _logFileFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var path = Path.Combine(_filesystem.Roots.Result.First().FullName, "logs");
			_filesystem.CreateDirectory(path);
			for (int i = 0; i < 257; ++i)
			{
				_filesystem.WriteAllBytes(Path.Combine(path, $"{i}.txt"), new byte[0]).Wait();
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
			                                      _logFileFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var unfiltered = dataSource.UnfilteredLogFile;
			var filtered = dataSource.FilteredLogFile;

			dataSource.QuickFilterChain = new List<ILogEntryFilter>();
			dataSource.UnfilteredLogFile.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogFile.Should().BeSameAs(filtered);

			dataSource.QuickFilterChain = null;
			dataSource.UnfilteredLogFile.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogFile.Should().BeSameAs(filtered);
		}

		[Test]
		[Description("Verifies that the FolderDataSource always exposes the same Unfiltered- and FilteredLogFile object")]
		public void TestChangeSingleLine()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logFileFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var unfiltered = dataSource.UnfilteredLogFile;
			var filtered = dataSource.FilteredLogFile;

			dataSource.IsSingleLine = !dataSource.IsSingleLine;
			dataSource.UnfilteredLogFile.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogFile.Should().BeSameAs(filtered);

			dataSource.IsSingleLine = !dataSource.IsSingleLine;
			dataSource.UnfilteredLogFile.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogFile.Should().BeSameAs(filtered);
		}

		[Test]
		[Description("Verifies that the FolderDataSource always exposes the same Unfiltered- and FilteredLogFile object")]
		public void TestChangeLevelFilter()
		{
			var dataSource = new FolderDataSource(_taskScheduler,
			                                      _logFileFactory,
			                                      _filesystem,
			                                      _settings,
			                                      TimeSpan.Zero);

			var unfiltered = dataSource.UnfilteredLogFile;
			var filtered = dataSource.FilteredLogFile;

			dataSource.LevelFilter = LevelFlags.Error;
			dataSource.UnfilteredLogFile.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogFile.Should().BeSameAs(filtered);

			dataSource.LevelFilter = LevelFlags.All;
			dataSource.UnfilteredLogFile.Should().BeSameAs(unfiltered);
			dataSource.FilteredLogFile.Should().BeSameAs(filtered);
		}
	}
}