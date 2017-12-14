using System;
using System.Threading;
using System.Windows;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Ui.Controls.LogView.DeltaTimes;

namespace Tailviewer.Test.Ui.Controls.LogView.DeltaTimes
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class LogEntryDeltaTimeColumnTest
	{
		private LogEntryDeltaTimeColumn _column;
		private Mock<ILogFile> _logFile;

		[SetUp]
		public void Setup()
		{
			_column = new LogEntryDeltaTimeColumn();
			_logFile = new Mock<ILogFile>();
		}

		[Test]
		[Description("Verifies that values for the delta time column are not retrieved when the control is invisible")]
		public void TestUpdateLinesNotVisible([Values(Visibility.Collapsed, Visibility.Hidden)] Visibility visibility)
		{
			_column.Visibility = visibility;

			_column.UpdateLines(_logFile.Object, new LogFileSection(0, 1), 0);
			_logFile.Verify(x => x.GetColumn(It.IsAny<LogFileSection>(),
			                                 It.IsAny<ILogFileColumn<TimeSpan?>>(),
			                                 It.IsAny<TimeSpan?[]>(),
			                                 It.IsAny<int>()),
			                Times.Never, "because the control is invisible and thus no data should've been retrieved");
		}
	}
}