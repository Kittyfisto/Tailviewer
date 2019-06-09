using System;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Bookmarks;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Settings.Bookmarks;

namespace Tailviewer.Test.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class DataSourcesTest
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new PluginLogFileFactory(_scheduler);
			_filesystem = new InMemoryFilesystem();
		}

		[SetUp]
		public void SetUp()
		{
			_settings = new Tailviewer.Settings.DataSourceSettings();
			_bookmarks = new Mock<IBookmarks>();
			_dataSources = new Tailviewer.BusinessLogic.DataSources.DataSources(_logFileFactory, _scheduler, _filesystem, _settings, _bookmarks.Object);
		}

		[TearDown]
		public void TearDown()
		{
			_dataSources.Dispose();
		}

		private Tailviewer.Settings.DataSourceSettings _settings;
		private Tailviewer.BusinessLogic.DataSources.DataSources _dataSources;
		private ManualTaskScheduler _scheduler;
		private ILogFileFactory _logFileFactory;
		private Mock<IBookmarks> _bookmarks;
		private InMemoryFilesystem _filesystem;

		[Test]
		public void TestAddFile()
		{
			SingleDataSource source = _dataSources.AddFile(@"E:\Code\test.log");
			source.Should().NotBeNull();
			source.FullFileName.Should().Be(@"E:\Code\test.log");
			source.FollowTail.Should().BeFalse();
			source.Id.Should().NotBe(Guid.Empty, "Because a newly added data source should have a unique id");

			_settings.Count.Should().Be(1);
			_settings[0].File.Should().Be(@"E:\Code\test.log");
		}

		[Test]
		public void TestAddFolder([Values(true, false)] bool recursive,
		                          [Values("", "*.log", "*.txt|*.foo")] string pattern)
		{
			_settings.FolderDataSourcePattern = pattern;
			_settings.FolderDataSourceRecursive = recursive;

			var source = _dataSources.AddFolder(@"E:\Code\");
			source.Should().NotBeNull();
			source.FullFileName.Should().Be(@"E:\Code\");
			source.FollowTail.Should().BeFalse();
			source.Id.Should().NotBe(Guid.Empty, "Because a newly added data source should have a unique id");
			source.Recursive.Should().Be(recursive);
			source.LogFileSearchPattern.Should().Be(pattern);

			_settings.Count.Should().Be(1);
			_settings[0].File.Should().BeNull();
			_settings[0].LogFileFolderPath.Should().Be(@"E:\Code\");
		}

		[Test]
		[Description("Verifies that adding a group also creates and adds a corresponding settings object")]
		public void TestAddGroup1()
		{
			var settings = new DataSourceSettings();
			using (var dataSources = new Tailviewer.BusinessLogic.DataSources.DataSources(_logFileFactory, _scheduler, _filesystem, settings, _bookmarks.Object))
			{
				MergedDataSource group = dataSources.AddGroup();
				group.Should().NotBeNull();
				group.Settings.Should().NotBeNull();
				settings.Should().Equal(group.Settings);
			}
		}

		[Test]
		[Description("Verifies that a newly added group has a unique id")]
		public void TestAddGroup2()
		{
			MergedDataSource group = _dataSources.AddGroup();
			group.Id.Should().NotBe(Guid.Empty);
		}

		[Test]
		[Description("Verifies that a newly added group has a display name")]
		public void TestAddGroup3()
		{
			var group = _dataSources.AddGroup();
			group.DisplayName.Should().Be("Merged Data Source");
		}

		[Test]
		[Description("Verifies that a business object is created for every data source")]
		public void TestCtor1()
		{
			var settings = new Tailviewer.Settings.DataSourceSettings
				{
					new DataSource(@"E:\Code\test.log")
						{
							Id = DataSourceId.CreateNew()
						}
				};
			using (var dataSources = new Tailviewer.BusinessLogic.DataSources.DataSources(_logFileFactory, _scheduler, _filesystem, settings, _bookmarks.Object))
			{
				dataSources.Count.Should().Be(1);
				IDataSource dataSource = dataSources.Sources.First();
				dataSource.FullFileName.Should().Be(settings[0].File);
				dataSource.Id.Should().Be(settings[0].Id);
			}
		}

		[Test]
		[Description("Verifies that data sources are added to a parent, when specified")]
		public void TestCtor2()
		{
			var settings = new Tailviewer.Settings.DataSourceSettings
				{
					new DataSource("test1.log")
						{
							Id = DataSourceId.CreateNew()
						},
					new DataSource("test2.log")
						{
							Id = DataSourceId.CreateNew()
						},
					new DataSource("test.log")
						{
							Id = DataSourceId.CreateNew()
						}
				};
			var merged = new DataSource {Id = DataSourceId.CreateNew()};
			settings.Add(merged);
			settings[0].ParentId = merged.Id;
			settings[1].ParentId = merged.Id;

			using (var dataSources = new Tailviewer.BusinessLogic.DataSources.DataSources(_logFileFactory, _scheduler, _filesystem, settings, _bookmarks.Object))
			{
				dataSources.Count.Should().Be(4, "Because we've loaded 4 data sources");
				var mergedDataSource = dataSources[3] as MergedDataSource;
				mergedDataSource.Should().NotBeNull();
				mergedDataSource.DataSourceCount.Should().Be(2, "Because 2 of the data sources are part of this group");
				IDataSource dataSource1 = dataSources[0];
				IDataSource dataSource2 = dataSources[1];

				mergedDataSource.OriginalSources.Should().Equal(new object[] {dataSource1, dataSource2});
				dataSource1.ParentId.Should().Be(merged.Id);
				dataSource2.ParentId.Should().Be(merged.Id);
			}
		}

		[Test]
		[Description("Verifies that data sources are added to the correct parent")]
		public void TestCtor3()
		{
			var settings = new Tailviewer.Settings.DataSourceSettings
				{
					new DataSource("test1.log")
						{
							Id = DataSourceId.CreateNew()
						},
					new DataSource("test2.log")
						{
							Id = DataSourceId.CreateNew()
						},
					new DataSource("test.log")
						{
							Id = DataSourceId.CreateNew()
						}
				};
			var merged1 = new DataSource {Id = DataSourceId.CreateNew()};
			settings.Add(merged1);
			var merged2 = new DataSource {Id = DataSourceId.CreateNew()};
			settings.Add(merged2);
			var merged3 = new DataSource {Id = DataSourceId.CreateNew()};
			settings.Add(merged3);
			settings[0].ParentId = merged1.Id;
			settings[1].ParentId = merged2.Id;
			settings[2].ParentId = merged3.Id;

			using (var dataSources = new Tailviewer.BusinessLogic.DataSources.DataSources(_logFileFactory, _scheduler, _filesystem, settings, _bookmarks.Object))
			{
				dataSources.Count.Should().Be(6, "Because we've loaded 6 data sources");
				var mergedDataSource1 = dataSources[3] as MergedDataSource;
				mergedDataSource1.Should().NotBeNull();
				mergedDataSource1.OriginalSources.Should().Equal(new object[] {dataSources[0]});

				var mergedDataSource2 = dataSources[4] as MergedDataSource;
				mergedDataSource2.Should().NotBeNull();
				mergedDataSource2.OriginalSources.Should().Equal(new object[] {dataSources[1]});

				var mergedDataSource3 = dataSources[5] as MergedDataSource;
				mergedDataSource3.Should().NotBeNull();
				mergedDataSource3.OriginalSources.Should().Equal(new object[] {dataSources[2]});
			}
		}

		[Test]
		public void TestCtor4()
		{
			var settings = new Tailviewer.Settings.DataSourceSettings
			{
				new DataSource
				{
					Id = DataSourceId.CreateNew()
				},
				new DataSource
				{
					Id = DataSourceId.CreateNew(),
					DisplayName = "My custom name"
				}
			};

			using (var dataSources = new Tailviewer.BusinessLogic.DataSources.DataSources(_logFileFactory, _scheduler, _filesystem, settings, _bookmarks.Object))
			{
				var group1 = dataSources.Sources.First() as IMergedDataSource;
				group1.Should().NotBeNull();
				group1.DisplayName.Should().Be("Merged Data Source", "because merged data sources which don't have a display name shall be assigned a default one");

				var group2 = dataSources.Sources.Last() as IMergedDataSource;
				group2.Should().NotBeNull();
				group2.DisplayName.Should().Be("My custom name", "because a custom name was supplied");
			}
		}

		[Test]
		public void TestCtor5()
		{
			var settings = new Tailviewer.Settings.DataSourceSettings
			{
				new DataSource
				{
					Id = DataSourceId.CreateNew(),
					LogFileFolderPath = @"F:\logs"
				},
			};

			using (var dataSources = new Tailviewer.BusinessLogic.DataSources.DataSources(_logFileFactory, _scheduler, _filesystem, settings, _bookmarks.Object))
			{
				var folder = dataSources.Sources.First() as IFolderDataSource;
				folder.Should().NotBeNull();
			}
		}

		[Test]
		public void TestRemove()
		{
			SingleDataSource source1 = _dataSources.AddFile(@"E:\Code\test1.log");
			SingleDataSource source2 = _dataSources.AddFile(@"E:\Code\test2.log");

			_dataSources.Remove(source1);
			_settings.Count.Should().Be(1);
			_settings[0].File.Should().Be(@"E:\Code\test2.log");

			_dataSources.Remove(source2);
			_settings.Should().BeEmpty();
		}

		[Test]
		public void TestTestTryAddBookmark1()
		{
			_dataSources.Bookmarks.Should().BeEmpty();

			_dataSources.TryAddBookmark(null, 1).Should().BeNull("because no data source was given");
			_dataSources.TryAddBookmark(new Mock<IDataSource>().Object, 2).Should().BeNull("because the data source hasn't been added");
			_dataSources.Bookmarks.Should().BeEmpty("because no boomark should've been added in the process");
		}

		[Test]
		public void TestRemoveBookmark1()
		{
			_dataSources.Bookmarks.Should().BeEmpty();

			new Action(() => _dataSources.RemoveBookmark(new Bookmark(new Mock<IDataSource>().Object, 1)))
				.Should().NotThrow("because removing a bookmark that was never there shouldn't be considered an error");
			_dataSources.Bookmarks.Should().BeEmpty("because no boomark should've been added in the process");
		}
	}
}