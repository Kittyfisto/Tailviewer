using System;
using System.Threading;
using System.Windows.Controls.Primitives;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Ui.Controls.LogView;

namespace Tailviewer.Test.Ui.Controls
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public sealed class TextCanvasTest
	{
		private TextCanvas _control;

		[SetUp]
		public void SetUp()
		{
			_control = new TextCanvas(new ScrollBar(), new ScrollBar());
		}

		[Test]
		[Description("Verifies that the control doesn't throw upon resizing when the current line is set to an impossible value, with regards to the log file")]
		public void TestOnSizeChanged1()
		{
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Count).Returns(0);
			_control.LogFile = logFile.Object;
			_control.CurrentLine = 1;

			new Action(() => _control.OnSizeChanged()).ShouldNotThrow();

			_control.CurrentlyVisibleSection.Should().Be(new LogFileSection(0, 0));
		}
	}
}