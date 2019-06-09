using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class FolderDataSourceViewModelTest
	{
		[Test]
		public void TestConstruction1()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.LogFileFolderPath).Returns(@"F:\logs\today");
			dataSource.Setup(x => x.Settings).Returns(new DataSource());
			dataSource.Setup(x => x.OriginalSources).Returns(new List<ISingleDataSource>());
			var actionCenter = new Mock<IActionCenter>();

			var viewModel = new FolderDataSourceViewModel(dataSource.Object, actionCenter.Object);
			viewModel.CanBeRenamed.Should().BeFalse();
			viewModel.DisplayName.Should().Be("today");
			viewModel.DataSourceOrigin.Should().Be(@"F:\logs\today");
			viewModel.FileReport.Should().Be("Monitoring 0 files");
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
			var actionCenter = new Mock<IActionCenter>();

			var viewModel = new FolderDataSourceViewModel(dataSource.Object, actionCenter.Object);
			viewModel.Update();
			viewModel.FileReport.Should().Be("Monitoring 0 files");
		}

		[Test]
		public void TestUpdateOnewNewSource()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.Settings).Returns(new DataSource());

			var sources = new List<ISingleDataSource>();
			var childDataSource = new Mock<ISingleDataSource>();
			childDataSource.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(childDataSource.Object);
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			dataSource.Setup(x => x.FilteredFileCount).Returns(() => sources.Count);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(() => sources.Count);
			var actionCenter = new Mock<IActionCenter>();

			var viewModel = new FolderDataSourceViewModel(dataSource.Object, actionCenter.Object);
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

			var sources = new List<ISingleDataSource>();
			var childDataSource = new Mock<ISingleDataSource>();
			childDataSource.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(childDataSource.Object);
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			var actionCenter = new Mock<IActionCenter>();

			var viewModel = new FolderDataSourceViewModel(dataSource.Object, actionCenter.Object);
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

			var sources = new List<ISingleDataSource>();
			var childDataSource = new Mock<ISingleDataSource>();
			childDataSource.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(childDataSource.Object);
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			var actionCenter = new Mock<IActionCenter>();

			var viewModel = new FolderDataSourceViewModel(dataSource.Object, actionCenter.Object);

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

			var sources = new List<ISingleDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			dataSource.Setup(x => x.FilteredFileCount).Returns(() => sources.Count);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(() => sources.Count);
			var actionCenter = new Mock<IActionCenter>();

			var viewModel = new FolderDataSourceViewModel(dataSource.Object, actionCenter.Object);
			viewModel.Observable.Should().BeEmpty();

			var child1 = new Mock<ISingleDataSource>();
			child1.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(child1.Object);

			var child2 = new Mock<ISingleDataSource>();
			child2.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(child2.Object);

			viewModel.Update();
			viewModel.FileReport.Should().Be("Monitoring 2 files");
		}

		[Test]
		public void TestUpdateFiltered3Files()
		{
			var dataSource = new Mock<IFolderDataSource>();
			var sources = new List<ISingleDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			var child1 = new Mock<ISingleDataSource>();
			child1.Setup(x => x.Settings).Returns(new DataSource());
			sources.Add(child1.Object);

			dataSource.Setup(x => x.FilteredFileCount).Returns(1);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(4);
			var actionCenter = new Mock<IActionCenter>();

			var viewModel = new FolderDataSourceViewModel(dataSource.Object, actionCenter.Object);
			viewModel.FileReport.Should().Be("Monitoring 1 file (3 not matching filter)");
		}

		[Test]
		public void TestUpdateSkippedFilesDueToLimitation()
		{
			var dataSource = new Mock<IFolderDataSource>();
			var sources = new List<ISingleDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			for (int i = 0; i < 256; ++i)
			{
				var child = new Mock<ISingleDataSource>();
				child.Setup(x => x.Settings).Returns(new DataSource());
				sources.Add(child.Object);
			}

			dataSource.Setup(x => x.FilteredFileCount).Returns(257);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(257);
			var actionCenter = new Mock<IActionCenter>();

			var viewModel = new FolderDataSourceViewModel(dataSource.Object, actionCenter.Object);
			viewModel.FileReport.Should().Be("Monitoring 256 files\r\nSkipping 1 file (due to internal limitation)");
		}

		[Test]
		public void TestUpdateFilterAndSkippedFilesDueToLimitation()
		{
			var dataSource = new Mock<IFolderDataSource>();
			var sources = new List<ISingleDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(sources);
			for (int i = 0; i < 256; ++i)
			{
				var child = new Mock<ISingleDataSource>();
				child.Setup(x => x.Settings).Returns(new DataSource());
				sources.Add(child.Object);
			}

			dataSource.Setup(x => x.FilteredFileCount).Returns(260);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(300);
			var actionCenter = new Mock<IActionCenter>();

			var viewModel = new FolderDataSourceViewModel(dataSource.Object, actionCenter.Object);
			viewModel.FileReport.Should().Be("Monitoring 256 files (40 not matching filter)\r\nSkipping 4 files (due to internal limitation)");
		}
	}
}
