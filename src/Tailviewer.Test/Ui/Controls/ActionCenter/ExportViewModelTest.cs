using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.Ui.Controls.ActionCenter;

namespace Tailviewer.Test.Ui.Controls.ActionCenter
{
	[TestFixture]
	public sealed class ExportViewModelTest
	{
		[Test]
		public void TestUpdate1()
		{
			var export = new Mock<IExportAction>();
			export.Setup(x => x.Progress).Returns(Percentage.HundredPercent);
			export.Setup(x => x.Exception).Returns(new ExportException("The directory 'foobar' does not exist and cannot be created"));

			var viewModel = new ExportViewModel(export.Object);
			viewModel.Update();

			viewModel.IsFinished.Should().BeTrue();
			viewModel.ErrorMessage.Should().Be("The directory 'foobar' does not exist and cannot be created");
		}
	}
}