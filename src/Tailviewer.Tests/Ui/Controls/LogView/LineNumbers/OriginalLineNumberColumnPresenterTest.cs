using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Sources;
using Tailviewer.Settings;
using Tailviewer.Ui.LogView.LineNumbers;

namespace Tailviewer.Tests.Ui.Controls.LogView.LineNumbers
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class OriginalLineNumberColumnPresenterTest
	{
		private OriginalLineNumberColumnPresenter _column;
		private InMemoryLogSource _logSource;

		[SetUp]
		public void Setup()
		{
			_column = new OriginalLineNumberColumnPresenter(TextSettings.Default);

			_logSource = new InMemoryLogSource();
		}

		[Test]
		public void TestUpdateLineNumbers1()
		{
			const int count = 10;
			AddLines(count);

			_column.FetchValues(_logSource, new LogSourceSection(0, count), 0);
			var numbers = _column.LineNumbers;
			numbers.Should().NotBeNull();
			numbers.Should().HaveCount(count);
			numbers.Should().Equal(Enumerable.Range(0, count).Select(i => new LineNumberFormatter(i+1)));
		}

		[Test]
		[Description("Verifies that the canvas displays the original line numbers when displaying the section of a filtered log file")]
		public void TestUpdateLineNumbers2()
		{
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.LogEntryCount)).Returns(4);
			logFile.Setup(x => x.GetProperty(TextProperties.LineCount)).Returns(1000);
			logFile.Setup(x => x.GetColumn(It.Is<LogSourceSection>(y => y == new LogSourceSection(0, 4)),
			                               It.Is<IColumnDescriptor<int>>(y => y == GeneralColumns.OriginalLineNumber),
										   It.IsAny<int[]>(),
			                               It.IsAny<int>(),
			                               It.IsAny<LogSourceQueryOptions>()))
				.Callback((IReadOnlyList<LogLineIndex> section, IColumnDescriptor<int> unused, int[] indices, int unused2,LogSourceQueryOptions unused3) =>
				{
					indices[0] = 42;
					indices[1] = 101;
					indices[2] = 255;
					indices[3] = 512;
				});
			_column.FetchValues(logFile.Object, new LogSourceSection(0, 4), 0);
			_column.Width.Should().BeApproximately(24.8, 0.1, "because the canvas should reserve space for the original line count, which is 4 digits");
			_column.LineNumbers.Should().Equal(new LineNumberFormatter(42),
				new LineNumberFormatter(101),
				new LineNumberFormatter(255),
				new LineNumberFormatter(512));
		}

		private void AddLines(int count)
		{
			for (int i = 0; i < count; ++i)
			{
				_logSource.AddEntry("", LevelFlags.Fatal);
			}
		}
	}
}