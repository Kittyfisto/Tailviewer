using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;
using Tailviewer.Ui.Controls.LogView.LineNumbers;

namespace Tailviewer.Test.Ui.Controls.LogView.LineNumbers
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class LineNumberColumnTest
	{
		private LineNumberColumn _column;
		private InMemoryLogFile _logFile;

		[SetUp]
		public void Setup()
		{
			_column = new LineNumberColumn();

			_logFile = new InMemoryLogFile();
		}

		[Test]
		public void TestUpdateLineNumbers1()
		{
			const int count = 10;
			AddLines(count);

			_column.UpdateLineNumbers(_logFile, new LogFileSection(0, count), 0);
			var numbers = _column.LineNumbers;
			numbers.Should().NotBeNull();
			numbers.Should().HaveCount(count);
			numbers.Should().Equal(Enumerable.Range(0, count).Select(i => new LineNumber(i)));
		}

		[Test]
		[Description("Verifies that the canvas displays the original line numbers when displaying the section of a filtered log file")]
		public void TestUpdateLineNumbers2()
		{
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Count).Returns(4);
			logFile.Setup(x => x.OriginalCount).Returns(1000);
			logFile.Setup(x => x.GetOriginalIndicesFrom(It.Is<LogFileSection>(y => y == new LogFileSection(0, 4)),
					It.IsAny<LogLineIndex[]>()))
				.Callback((LogFileSection section, LogLineIndex[] indices) =>
				{
					indices[0] = new LogLineIndex(42);
					indices[1] = new LogLineIndex(101);
					indices[2] = new LogLineIndex(255);
					indices[3] = new LogLineIndex(512);
				});
			_column.UpdateLineNumbers(logFile.Object, new LogFileSection(0, 4), 0);
			_column.Width.Should().BeApproximately(24.8, 0.1, "because the canvas should reserve space for the original line count, which is 4 digits");
			_column.LineNumbers.Should().Equal(new LineNumber(42),
				new LineNumber(101),
				new LineNumber(255),
				new LineNumber(512));
		}

		private void AddLines(int count)
		{
			for (int i = 0; i < count; ++i)
			{
				_logFile.AddEntry("", LevelFlags.Fatal);
			}
		}
	}
}