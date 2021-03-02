using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Settings;
using Tailviewer.Ui.DataSourceTree;

namespace Tailviewer.Acceptance.Tests.Ui.LogView
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
			var applicationSettings = new Mock<IApplicationSettings>();
			var viewModel = new FolderDataSourceViewModel(dataSource.Object, actionCenter.Object, applicationSettings.Object);

			actionCenter.Verify(x => x.Add(It.Is<INotification>(y => y is OpenFolderAction)), Times.Never);

			var openInExplorerItem = viewModel.FileMenuItems.First(x => x.Header != null && x.Header == "Open Containing Folder");
			var openInExplorerCommand = openInExplorerItem.Command;
			openInExplorerCommand.Should().NotBeNull();
			openInExplorerCommand.CanExecute(null).Should().BeTrue();
			openInExplorerCommand.Execute(null);

			actionCenter.Verify(x => x.Add(It.Is<INotification>(y => y is OpenFolderAction)), Times.Once);
		}
	}
}
