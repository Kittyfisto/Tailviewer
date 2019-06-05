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
		public void TestUpdateOnewNewSource()
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
			viewModel.Observable.Should().BeEmpty();

			viewModel.Update();

			viewModel.Observable.Should().HaveCount(1);
			var childViewModel = viewModel.Observable.First();
			childViewModel.Should().NotBeNull();
			childViewModel.Parent.Should().BeSameAs(viewModel);
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
	}
}
