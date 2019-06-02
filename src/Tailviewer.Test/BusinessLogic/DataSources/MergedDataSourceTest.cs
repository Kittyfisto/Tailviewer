using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;

namespace Tailviewer.Test.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class MergedDataSourceTest
	{
		[SetUp]
		public void SetUp()
		{
			_taskScheduler = new ManualTaskScheduler();
			_logFileFactory = new PluginLogFileFactory(_taskScheduler);
			_settings = new DataSource
				{
					Id = DataSourceId.CreateNew(),
					MergedDataSourceDisplayMode = DataSourceDisplayMode.CharacterCode
				};
			_merged = new MergedDataSource(_taskScheduler, _settings, TimeSpan.Zero) {IsSingleLine = true};
		}

		private MergedDataSource _merged;
		private DataSource _settings;
		private ManualTaskScheduler _taskScheduler;
		private PluginLogFileFactory _logFileFactory;

		[Test]
		[Description("Verifies that adding a data source to a group sets the parent id of the settings object")]
		public void TestAdd1()
		{
			var settings = new DataSource("foo") {Id = DataSourceId.CreateNew()};
			var dataSource = new SingleDataSource(_logFileFactory, _taskScheduler, settings);
			_merged.Add(dataSource);
			settings.ParentId.Should()
			        .Be(_settings.Id, "Because the parent-child relationship should've been declared via ParentId");
		}

		[Test]
		[Description("Verifies that adding a data source to a group adds it's logfile to the group's one")]
		public void TestAdd2()
		{
			var settings = new DataSource("foo") {Id = DataSourceId.CreateNew()};
			var dataSource = new SingleDataSource(_logFileFactory, _taskScheduler, settings);
			_merged.Add(dataSource);
			_merged.UnfilteredLogFile.Should().NotBeNull();
			_merged.UnfilteredLogFile.Should().BeOfType<MergedLogFile>();
			((MergedLogFile) _merged.UnfilteredLogFile).Sources.Should().Equal(new object[] {dataSource.OriginalLogFile});
		}

		[Test]
		[Description("Verifies that the data source disposed of the merged log file")]
		public void TestAdd3()
		{
			var settings = new DataSource("foo") {Id = DataSourceId.CreateNew()};
			var dataSource = new SingleDataSource(_logFileFactory, _taskScheduler, settings);
			ILogFile logFile1 = _merged.UnfilteredLogFile;

			_merged.Add(dataSource);
			ILogFile logFile2 = _merged.UnfilteredLogFile;

			logFile2.Should().NotBeSameAs(logFile1);
			((AbstractLogFile) logFile1).IsDisposed.Should().BeTrue();
		}

		[Test]
		public void TestMultiline()
		{
			var settings = new DataSource("foo") { Id = DataSourceId.CreateNew() };
			var dataSource = new SingleDataSource(_logFileFactory, _taskScheduler, settings);
			ILogFile logFile1 = _merged.UnfilteredLogFile;

			_merged.Add(dataSource);
			ILogFile logFile2 = _merged.UnfilteredLogFile;

			logFile2.Should().NotBeSameAs(logFile1);
			((AbstractLogFile)logFile1).IsDisposed.Should().BeTrue();
		}

		[Test]
		[Description("Verifies that the data source disposed of the merged log file")]
		public void TestChangeFilter1()
		{
			ILogFile logFile1 = _merged.UnfilteredLogFile;
			_merged.SearchTerm = "foo";
			var settings1 = new DataSource("foo") {Id = DataSourceId.CreateNew()};
			var dataSource1 = new SingleDataSource(_logFileFactory, _taskScheduler, settings1);
			_merged.Add(dataSource1);
			ILogFile logFile2 = _merged.UnfilteredLogFile;

			logFile2.Should().NotBeSameAs(logFile1);
			((AbstractLogFile) logFile1).IsDisposed.Should().BeTrue();
		}

		[Test]
		[Description("Verifies that creating a data-source without specifying the settings object is not allowed")]
		public void TestCtor1()
		{
			new Action(() => new MergedDataSource(_taskScheduler, null))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		[Repeat(100)]
		[Description("Verifies that creating a data-source without assinging a GUID is not allowed")]
		public void TestCtor2()
		{
			new Action(() => new MergedDataSource(_taskScheduler, new DataSource()))
				.Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that the log-file of a newly created group is empty")]
		public void TestCtor3()
		{
			_merged.UnfilteredLogFile.Should().NotBeNull("Because a log file should be present at all times");
			_merged.UnfilteredLogFile.Count.Should().Be(0);
		}

		[Test]
		[Description("Verifies that the log-file of a newly created group is actually a MergedLogFile")]
		public void TestCtor4()
		{
			_merged.UnfilteredLogFile.Should().BeOfType<MergedLogFile>();
		}

		[Test]
		public void TestCtor5()
		{
			_merged.DisplayMode.Should().Be(_settings.MergedDataSourceDisplayMode);
		}

		[Test]
		public void TestDispose1()
		{
			_merged.Dispose();
			_taskScheduler.PeriodicTaskCount.Should().Be(0, "because all tasks should've been removed");
		}

		[Test]
		public void TestDispose2()
		{
			var settings = new DataSource("foo") { Id = DataSourceId.CreateNew() };
			var dataSource = new SingleDataSource(_logFileFactory, _taskScheduler, settings);
			_merged.Add(dataSource);
			_merged.Remove(dataSource);
			dataSource.Dispose();

			_merged.Dispose();
			_taskScheduler.PeriodicTaskCount.Should().Be(0, "because all tasks should've been removed");
		}

		[Test]
		[Description(
			"Verifies that removing a data source from a group sets the parent id of the settings object to Empty again")]
		public void TestRemove1()
		{
			var settings = new DataSource("foo") {Id = DataSourceId.CreateNew()};
			var dataSource = new SingleDataSource(_logFileFactory, _taskScheduler, settings);
			_merged.Add(dataSource);
			_merged.Remove(dataSource);
			dataSource.Settings.ParentId.Should().Be(DataSourceId.Empty);
		}

		[Test]
		[Description("Verifies that removing a data source from a group also removes its logfile from the merged logfile")]
		public void TestRemove2()
		{
			var settings1 = new DataSource("foo") {Id = DataSourceId.CreateNew()};
			var dataSource1 = new SingleDataSource(_logFileFactory, _taskScheduler, settings1);
			_merged.Add(dataSource1);

			var settings2 = new DataSource("bar") {Id = DataSourceId.CreateNew()};
			var dataSource2 = new SingleDataSource(_logFileFactory, _taskScheduler, settings2);
			_merged.Add(dataSource2);

			_merged.Remove(dataSource2);
			_merged.UnfilteredLogFile.Should().NotBeNull();
			_merged.UnfilteredLogFile.Should().BeOfType<MergedLogFile>();
			((MergedLogFile) _merged.UnfilteredLogFile).Sources.Should().Equal(new object[] {dataSource1.OriginalLogFile});
		}

		[Test]
		public void TestChangeDisplayMode()
		{
			_merged.DisplayMode = DataSourceDisplayMode.CharacterCode;
			_settings.MergedDataSourceDisplayMode.Should().Be(DataSourceDisplayMode.CharacterCode);

			_merged.DisplayMode = DataSourceDisplayMode.Filename;
			_settings.MergedDataSourceDisplayMode.Should().Be(DataSourceDisplayMode.Filename);
		}

		[Test]
		[Description("Verifies that MergedDataSource and SingleDataSource can work in conjunction to provide a merged view of multi line log files")]
		public void TestMergeMultiline1()
		{
			var logFile1 = new InMemoryLogFile();
			var source1Id = new LogLineSourceId(0);
			var source1 = new SingleDataSource(_taskScheduler,
			                                   new DataSource { Id = DataSourceId.CreateNew() },
			                                   logFile1,
			                                   TimeSpan.Zero);

			var logFile2 = new InMemoryLogFile();
			var source2Id = new LogLineSourceId(0);
			var source2 = new SingleDataSource(_taskScheduler,
			                                   new DataSource { Id = DataSourceId.CreateNew() },
			                                   logFile2,
			                                   TimeSpan.Zero);

			_merged.IsSingleLine = false;
			_merged.Add(source1);
			_merged.Add(source2);

			var t1 = new DateTime(2017, 11, 26, 14, 26, 0);
			logFile1.AddEntry("Hello, World!", LevelFlags.Info, t1);

			var t2 = new DateTime(2017, 11, 26, 14, 27, 0);
			logFile2.AddEntry("foo", LevelFlags.Trace, t2);
			logFile2.AddEntry("bar", LevelFlags.None);

			var t3 = new DateTime(2017, 11, 26, 14, 28, 0);
			logFile1.AddEntry("Houston, we have a problem", LevelFlags.Warning, t3);

			_taskScheduler.Run(4); //< There's a few proxies involved and thus one round won't do it
			_merged.FilteredLogFile.Count.Should().Be(4, "because there are four lines in total, each of which has a timestamp");
			_merged.FilteredLogFile.GetLine(0).Should().Be(new LogLine(0, 0, source1Id, "Hello, World!", LevelFlags.Info, t1));
			_merged.FilteredLogFile.GetLine(1).Should().Be(new LogLine(1, 1, source2Id, "foo", LevelFlags.Trace, t2));
			_merged.FilteredLogFile.GetLine(2).Should().Be(new LogLine(2, 1, source2Id, "bar", LevelFlags.Trace, t2));
			_merged.FilteredLogFile.GetLine(3).Should().Be(new LogLine(3, 2, source1Id, "Houston, we have a problem", LevelFlags.Warning, t3));
		}
	}
}