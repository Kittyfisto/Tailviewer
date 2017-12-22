using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.FileExplorer;

namespace Tailviewer.Test.BusinessLogic.ActionCenter
{
	[TestFixture]
	public class OpenFolderActionTest
	{
		[Test]
		public void TestOpenFolder()
		{
			var fileExplorer = new Mock<IFileExplorer>();
			fileExplorer.Setup(f => f.SelectFile(It.IsAny<string>(), It.IsAny<IProgress<Percentage>>()))
				.Throws<DirectoryNotFoundException>();

			var openFolderAction = new OpenFolderAction("unknown directory", fileExplorer.Object);
			openFolderAction.Property(x => x.Progress).ShouldEventually().Be(Percentage.HundredPercent);
			openFolderAction.Exception.Should().BeOfType<OpenFolderException>();
			openFolderAction.Exception.Message.Should().Be("Unable to find directory 'unknown directory'.");
		}
	}
}