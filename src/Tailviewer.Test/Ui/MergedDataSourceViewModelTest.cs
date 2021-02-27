using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Settings;
using Tailviewer.Settings.Bookmarks;
using Tailviewer.Ui.DataSourceTree;
using DataSources = Tailviewer.BusinessLogic.DataSources.DataSources;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class MergedDataSourceViewModelTest
	{
		[SetUp]
		public void SetUp()
		{
			_settings = new DataSourceSettings();
			_bookmarks = new Mock<IBookmarks>();

			_scheduler = new ManualTaskScheduler();
			_logFileFactory = new SimplePluginLogFileFactory(_scheduler);
			_filesystem = new InMemoryFilesystem();
			_dataSources = new DataSources(_logFileFactory, _scheduler, _filesystem, _settings, _bookmarks.Object);
			_actionCenter = new Mock<IActionCenter>();
			_applicationSettings = new Mock<IApplicationSettings>();
		}

		[Pure]
		private MergedDataSourceViewModel CreateMergedViewModel(MergedDataSource dataSource)
		{
			return new MergedDataSourceViewModel(dataSource, _actionCenter.Object, _applicationSettings.Object);
		}

		[Pure]
		private FileDataSourceViewModel CreateFileViewModel(FileDataSource dataSource)
		{
			return new FileDataSourceViewModel(dataSource, _actionCenter.Object, _applicationSettings.Object);
		}

		private DataSources _dataSources;
		private Tailviewer.Settings.DataSourceSettings _settings;
		private ManualTaskScheduler _scheduler;
		private ILogFileFactory _logFileFactory;
		private Mock<IActionCenter> _actionCenter;
		private Mock<IApplicationSettings> _applicationSettings;
		private Mock<IBookmarks> _bookmarks;
		private InMemoryFilesystem _filesystem;

		[Test]
		public void TestConstruction1()
		{
			var model = CreateMergedViewModel(_dataSources.AddGroup());
			model.IsSelected.Should().BeFalse();
			model.IsExpanded.Should().BeTrue();
		}

		[Test]
		public void TestConstruction2([Values(DataSourceDisplayMode.Filename, DataSourceDisplayMode.CharacterCode)] DataSourceDisplayMode displayMode)
		{
			var dataSource = _dataSources.AddGroup();
			dataSource.DisplayMode = displayMode;

			var model = CreateMergedViewModel(dataSource);
			model.DisplayMode.Should().Be(displayMode);
		}

		[Test]
		public void TestConstruction3()
		{
			var dataSource = _dataSources.AddGroup();
			dataSource.DisplayName = "Some group";

			var model = CreateMergedViewModel(dataSource);
			model.DisplayName.Should().Be("Some group");
		}

		[Test]
		public void TestChangeDisplayName1()
		{
			var dataSource = _dataSources.AddGroup();
			var model = CreateMergedViewModel(dataSource);
			model.CanBeRenamed.Should().BeTrue("because this implementation should support renaming...");
			model.DisplayName.Should().Be("Merged Data Source");

			using (var monitor = model.Monitor())
			{
				model.DisplayName = "Foobar";
				model.DisplayName.Should().Be("Foobar");
				model.DataSourceOrigin.Should().Be("Foobar");
				monitor.Should().RaisePropertyChangeFor(x => x.DisplayName, "because we've just changed the name");
				monitor.Should().RaisePropertyChangeFor(x => x.DataSourceOrigin, "because we've just changed the name");
			}
		}

		[Test]
		public void TestChangeDisplayName2()
		{
			var dataSource = _dataSources.AddGroup();
			dataSource.DisplayName = "Some group";

			var model = CreateMergedViewModel(dataSource);

			model.DisplayName = "Foobar";
			dataSource.DisplayName.Should().Be("Foobar");
		}

		[Test]
		public void TestExpand()
		{
			var dataSource = _dataSources.AddGroup();
			var model = CreateMergedViewModel(dataSource);
			model.IsExpanded = false;
			model.IsExpanded.Should().BeFalse();
			dataSource.IsExpanded.Should().BeFalse();

			model.IsExpanded = true;
			model.IsExpanded.Should().BeTrue();
			dataSource.IsExpanded.Should().BeTrue();
		}

		[Test]
		public void TestAddChild1()
		{
			var model = CreateMergedViewModel(_dataSources.AddGroup());
			FileDataSource source = _dataSources.AddFile("foo");
			var sourceViewModel = CreateFileViewModel(source);
			model.AddChild(sourceViewModel);
			model.Observable.Should().Equal(sourceViewModel);
			sourceViewModel.Parent.Should().BeSameAs(model);
		}

		[Test]
		public void TestAddChild2()
		{
			var dataSource = _dataSources.AddGroup();
			var model = CreateMergedViewModel(dataSource);

			FileDataSource source = _dataSources.AddFile("foo");
			var sourceViewModel = CreateFileViewModel(source);

			model.AddChild(sourceViewModel);
			sourceViewModel.CharacterCode.Should().Be("A", "because the merged data source is responsible for providing unique character codes");
		}

		[Test]
		[Description("Verifies that there cannot be more than 255 children in a single group")]
		public void TestAddChild3()
		{
			var dataSource = _dataSources.AddGroup();
			var model = CreateMergedViewModel(dataSource);

			var sources = new List<FileDataSourceViewModel>();
			for (int i = 0; i < LogEntrySourceId.MaxSources; ++i)
			{
				var source = _dataSources.AddFile(i.ToString());
				var sourceViewModel = CreateFileViewModel(source);
				sources.Add(sourceViewModel);

				model.AddChild(sourceViewModel).Should().BeTrue("because the child should've been added");
				model.Observable.Should().Equal(sources, "because all previously added children should be there");
			}

			var tooMuch = CreateFileViewModel(_dataSources.AddFile("dadw"));
			model.AddChild(tooMuch).Should().BeFalse("because no more children can be added");
			model.Observable.Should().Equal(sources, "because only those sources which could be added should be present");
		}

		[Test]
		public void TestInsertChild1()
		{
			var dataSource = _dataSources.AddGroup();
			var model = CreateMergedViewModel(dataSource);

			FileDataSource source = _dataSources.AddFile("foo");
			var sourceViewModel = CreateFileViewModel(source);

			model.Insert(0, sourceViewModel);
			sourceViewModel.CharacterCode.Should().Be("A", "because the merged data source is responsible for providing unique character codes");
		}

		[Test]
		public void TestInsertChild2()
		{
			var dataSource = _dataSources.AddGroup();
			var model = CreateMergedViewModel(dataSource);

			var child1 = CreateFileViewModel(_dataSources.AddFile("foo"));
			model.AddChild(child1);
			child1.CharacterCode.Should().Be("A");

			var child2 = CreateFileViewModel(_dataSources.AddFile("bar"));
			model.Insert(0, child2);
			model.Observable.Should().Equal(new object[]
			{
				child2, child1
			});

			const string reason = "because the merged data source is responsible for providing unique character codes";
			child2.CharacterCode.Should().Be("A", reason);
			child1.CharacterCode.Should().Be("B", reason);
		}

		[Test]
		public void TestRemoveChild1()
		{
			var dataSource = _dataSources.AddGroup();
			var model = CreateMergedViewModel(dataSource);

			var child1 = CreateFileViewModel(_dataSources.AddFile("foo"));
			model.AddChild(child1);

			var child2 = CreateFileViewModel(_dataSources.AddFile("bar"));
			model.AddChild(child2);
			model.Observable.Should().Equal(new object[]
			{
				child1, child2
			});

			child1.CharacterCode.Should().Be("A");
			child2.CharacterCode.Should().Be("B");

			model.RemoveChild(child1);
			model.Observable.Should().Equal(new object[] {child2});
			child2.CharacterCode.Should().Be("A");
		}

		[Test]
		public void TestChangeDisplayMode()
		{
			var dataSource = _dataSources.AddGroup();
			var model = CreateMergedViewModel(dataSource);

			model.DisplayMode = DataSourceDisplayMode.CharacterCode;
			dataSource.DisplayMode.Should().Be(DataSourceDisplayMode.CharacterCode);

			model.DisplayMode = DataSourceDisplayMode.Filename;
			dataSource.DisplayMode.Should().Be(DataSourceDisplayMode.Filename);
		}

	}
}