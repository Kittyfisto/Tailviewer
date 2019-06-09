using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.AcceptanceTests.Ui.ViewModels
{
	[TestFixture]
	public sealed class FolderDataSourceViewModelTest
	{
		[Test]
		public void TestOpenInExplorer()
		{
			var dataSource = new Mock<IFolderDataSource>();
			dataSource.Setup(x => x.OriginalSources).Returns(new List<ISingleDataSource>());
			dataSource.Setup(x => x.LogFileFolderPath).Returns(@"C:\foo\bar\logs");

			var actionCenter = new Mock<IActionCenter>();
			var viewModel = new FolderDataSourceViewModel(dataSource.Object, actionCenter.Object);

			actionCenter.Verify(x => x.Add(It.Is<INotification>(y => y is OpenFolderAction)), Times.Never);

			viewModel.OpenInExplorerCommand.Should().NotBeNull();
			viewModel.OpenInExplorerCommand.CanExecute(null).Should().BeTrue();
			viewModel.OpenInExplorerCommand.Execute(null);

			actionCenter.Verify(x => x.Add(It.Is<INotification>(y => y is OpenFolderAction)), Times.Once);
		}
	}
}
