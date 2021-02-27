using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Settings;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class FolderDataSourceViewModelTest
	{
		private Mock<IActionCenter> _actionCenter;
		private Mock<IApplicationSettings> _applicationSettings;

		[SetUp]
		public void Setup()
		{
			_actionCenter = new Mock<IActionCenter>();
			_applicationSettings = new Mock<IApplicationSettings>();
		}

		[Pure]
		private FolderDataSourceViewModel CreateFolderViewModel(IFolderDataSource dataSource)
		{
			return new FolderDataSourceViewModel(dataSource, _actionCenter.Object, _applicationSettings.Object);
		}

		[Test]
		public void TestConstruction1([Values(true, false)] bool recursive)
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.LogFileFolderPath).Returns(@"F:\logs\today");
			dataSource.Setup(x => x.LogFileSearchPattern).Returns("*.txt");
			dataSource.Setup(x => x.Recursive).Returns(recursive);
			dataSource.Setup(x => x.Settings).Returns(new DataSource());
			dataSource.Setup(x => x.OriginalSources).Returns(new List<ISingleDataSource>());

			var viewModel = CreateFolderViewModel(dataSource.Object);
			viewModel.CanBeRenamed.Should().BeFalse();
			viewModel.DisplayName.Should().Be("today");
			viewModel.DataSourceOrigin.Should().Be(@"F:\logs\today");
			viewModel.FileReport.Should().Be("Monitoring 0 files");

			viewModel.FolderPath.Should().Be(@"F:\logs\today");
			viewModel.SearchPattern.Should().Be("*.txt");
			viewModel.Recursive.Should().Be(recursive);
		}

		[Test]
		public void TestUpdateEmpty()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.LogFileFolderPath).Returns(@"F:\logs\today");
			dataSource.Setup(x => x.Settings).Returns(new DataSource());
			dataSource.Setup(x => x.OriginalSources).Returns(new List<ISingleDataSource>());
			dataSource.Setup(x => x.FilteredFileCount).Returns(0);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(0);

			var viewModel = CreateFolderViewModel(dataSource.Object);
			viewModel.Update();
			viewModel.FileReport.Should().Be("Monitoring 0 files");
		}

		[Test]
		public void TestUpdateOnewNewSource()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.Settings).Returns(new DataSource());

			var sources = new List<IFileDataSource>();
			var childDataSource = new Mock<IFileDataSource>();
			childDataSource.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(childDataSource.Object);
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			dataSource.Setup(x => x.FilteredFileCount).Returns(() => sources.Count);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(() => sources.Count);

			var viewModel = CreateFolderViewModel(dataSource.Object);
			viewModel.Observable.Should().BeEmpty();

			viewModel.Update();

			viewModel.Observable.Should().HaveCount(1);
			var childViewModel = viewModel.Observable.First();
			childViewModel.Should().NotBeNull();
			childViewModel.Parent.Should().BeSameAs(viewModel);

			viewModel.FileReport.Should().Be("Monitoring 1 file");
		}

		[Test]
		public void TestUpdateNoMoreSources()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.Settings).Returns(new DataSource());

			var sources = new List<IFileDataSource>();
			var childDataSource = new Mock<IFileDataSource>();
			childDataSource.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(childDataSource.Object);
			dataSource.Setup(x => x.OriginalSources).Returns(sources);

			var viewModel = CreateFolderViewModel(dataSource.Object);
			viewModel.Update();
			viewModel.Observable.Should().HaveCount(1);

			sources.Clear();
			viewModel.Update();
			viewModel.Observable.Should().BeEmpty();
		}

		[Test]
		public void TestUpdateNoChanges()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.Settings).Returns(new DataSource());

			var sources = new List<IFileDataSource>();
			var childDataSource = new Mock<IFileDataSource>();
			childDataSource.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(childDataSource.Object);
			dataSource.Setup(x => x.OriginalSources).Returns(sources);

			var viewModel = CreateFolderViewModel(dataSource.Object);

			viewModel.Update();
			viewModel.Observable.Should().HaveCount(1);
			var childViewModel = viewModel.Observable.First();

			viewModel.Update();
			viewModel.Observable.Should().HaveCount(1);
			viewModel.Observable.First().Should().BeSameAs(childViewModel);
		}

		[Test]
		public void TestUpdateTwoSources()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.Settings).Returns(new DataSource());

			var sources = new List<IFileDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			dataSource.Setup(x => x.FilteredFileCount).Returns(() => sources.Count);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(() => sources.Count);

			var viewModel = CreateFolderViewModel(dataSource.Object);
			viewModel.Observable.Should().BeEmpty();

			var child1 = new Mock<IFileDataSource>();
			child1.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(child1.Object);

			var child2 = new Mock<IFileDataSource>();
			child2.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(child2.Object);

			viewModel.Update();
			viewModel.FileReport.Should().Be("Monitoring 2 files");
		}

		[Test]
		public void TestUpdateFiltered3Files()
		{
			var dataSource = new Mock<IFolderDataSource>();
			var sources = new List<IFileDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			var child1 = new Mock<IFileDataSource>();
			child1.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(child1.Object);

			dataSource.Setup(x => x.FilteredFileCount).Returns(1);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(4);

			var viewModel = CreateFolderViewModel(dataSource.Object);
			viewModel.FileReport.Should().Be("Monitoring 1 file (3 not matching filter)");
		}

		[Test]
		public void TestUpdateSkippedFilesDueToLimitation()
		{
			var dataSource = new Mock<IFolderDataSource>();
			var sources = new List<IFileDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			for (int i = 0; i < 256; ++i)
			{
				var child = new Mock<IFileDataSource>();
				child.Setup(x => x.Settings).Returns(new DataSource());
				sources.Add(child.Object);
			}

			dataSource.Setup(x => x.FilteredFileCount).Returns(257);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(257);

			var viewModel = CreateFolderViewModel(dataSource.Object);
			viewModel.FileReport.Should().Be("Monitoring 256 files\r\nSkipping 1 file (due to internal limitation)");
		}

		[Test]
		public void TestUpdateFilterAndSkippedFilesDueToLimitation()
		{
			var dataSource = new Mock<IFolderDataSource>();
			var sources = new List<IFileDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			for (int i = 0; i < 256; ++i)
			{
				var child = new Mock<IFileDataSource>();
				child.Setup(x => x.Settings).Returns(new DataSource());
				sources.Add(child.Object);
			}

			dataSource.Setup(x => x.FilteredFileCount).Returns(260);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(300);

			var viewModel = CreateFolderViewModel(dataSource.Object);
			viewModel.FileReport.Should().Be("Monitoring 256 files (40 not matching filter)\r\nSkipping 4 files (due to internal limitation)");
		}

		[Test]
		[Description("Verifies that the folder path is applied only when editing is set to false")]
		public void TestChangeFolderPath()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(new List<ISingleDataSource>());
			var viewModel = CreateFolderViewModel(dataSource.Object);

			viewModel.IsEditing = true;
			viewModel.FolderPath = @"C:\bla";
			dataSource.Verify(x => x.Change(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);

			viewModel.IsEditing = false;
			dataSource.Verify(x => x.Change(@"C:\bla", It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
		}

		[Test]
		[Description("Verifies that the search pattern is applied only when editing is set to false")]
		public void TestChangeSearchPattern()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(new List<ISingleDataSource>());
			var viewModel = CreateFolderViewModel(dataSource.Object);

			viewModel.IsEditing = true;
			viewModel.SearchPattern = "*.txt;*.log";
			dataSource.Verify(x => x.Change(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);

			viewModel.IsEditing = false;
			dataSource.Verify(x => x.Change(It.IsAny<string>(), "*.txt;*.log", It.IsAny<bool>()), Times.Once);
		}

		[Test]
		[Description("Verifies that the recursive setting is applied only when editing is set to false")]
		public void TestChangeRecursive([Values(true, false)] bool recursive)
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(new List<ISingleDataSource>());
			var viewModel = CreateFolderViewModel(dataSource.Object);

			viewModel.IsEditing = true;
			viewModel.Recursive = recursive;
			dataSource.Verify(x => x.Change(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);

			viewModel.IsEditing = false;
			dataSource.Verify(x => x.Change(It.IsAny<string>(), It.IsAny<string>(), recursive), Times.Once);
		}

		[Test]
		public void TestRecursiveIsDirty([Values(true, false)] bool recursive)
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(new List<ISingleDataSource>());
			dataSource.Setup(x => x.Recursive).Returns(recursive);

			var viewModel = CreateFolderViewModel(dataSource.Object);

			viewModel.IsEditing = true;
			viewModel.Recursive = !recursive;
			viewModel.IsDirty.Should().BeTrue();

			viewModel.Recursive = recursive;
			viewModel.IsDirty.Should().BeFalse();
		}

		[Test]
		public void TestFolderPathIsDirty()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(new List<ISingleDataSource>());
			dataSource.Setup(x => x.LogFileFolderPath).Returns(@"F:\logs");

			var viewModel = CreateFolderViewModel(dataSource.Object);

			viewModel.IsEditing = true;
			viewModel.FolderPath = @"E:\logs";
			viewModel.IsDirty.Should().BeTrue();

			viewModel.FolderPath = @"F:\logs";
			viewModel.IsDirty.Should().BeFalse();
		}

		[Test]
		public void TestSearchPatternIsDirty()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(new List<ISingleDataSource>());
			dataSource.Setup(x => x.LogFileSearchPattern).Returns("*.bin");

			var viewModel = CreateFolderViewModel(dataSource.Object);

			viewModel.IsEditing = true;
			viewModel.SearchPattern = "*.txt";
			viewModel.IsDirty.Should().BeTrue();

			viewModel.SearchPattern = "*.bin";
			viewModel.IsDirty.Should().BeFalse();
		}
	}
}
