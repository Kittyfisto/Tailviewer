using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Ui.Controls.LogView;

namespace Tailviewer.Test.Ui.Controls.LogView
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class LineNumberCanvasTest
	{
		private LineNumberCanvas _canvas;
		private InMemoryLogFile _logFile;

		[SetUp]
		public void Setup()
		{
			_canvas = new LineNumberCanvas();

			_logFile = new InMemoryLogFile();
		}

		[Test]
		public void TestUpdateLineNumbers1()
		{
			const int count = 10;
			AddLines(count);

			_canvas.UpdateLineNumbers(_logFile, new LogFileSection(0, count), 0);
			var numbers = _canvas.LineNumbers;
			numbers.Should().NotBeNull();
			numbers.Should().HaveCount(count);
			numbers.Should().Equal(Enumerable.Range(0, count).Select(i => new LineNumber(i)));
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