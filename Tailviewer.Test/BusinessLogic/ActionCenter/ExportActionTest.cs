using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.Exporter;
using Tailviewer.Core;

namespace Tailviewer.Test.BusinessLogic.ActionCenter
{
	[TestFixture]
	public sealed class ExportActionTest
	{
		[Test]
		[Description("Verifies that an appropriate error is displayed when the drive of the export folder doesn't exist")]
		public void TestExport1()
		{
			var exporter = new Mock<ILogFileToFileExporter>();
			exporter.Setup(x => x.Export(It.IsAny<IProgress<Percentage>>()))
				.Throws<DirectoryNotFoundException>();

			var action = new ExportAction(exporter.Object, "", "some shitty folder");
			action.Property(x => x.Progress).ShouldEventually().Be(Percentage.HundredPercent);
			action.Exception.Should().BeOfType<ExportException>();
			action.Exception.Message.Should().Be("Unable to find or create directory 'some shitty folder'. Maybe its drive isn't connected or the medium is read only...");
		}
	}
}