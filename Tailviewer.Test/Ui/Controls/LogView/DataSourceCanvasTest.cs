using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.LogView;

namespace Tailviewer.Test.Ui.Controls.LogView
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class DataSourceCanvasTest
	{
		[Test]
		public void TestCtor()
		{
			var canvas = new DataSourceCanvas();
			canvas.DataSources.Should().BeEmpty();
			canvas.DisplayMode.Should().Be(DataSourceDisplayMode.Filename);
		}

		[Test]
		public void TestUpdateLineNumbers1()
		{
			var canvas = new DataSourceCanvas();
			canvas.UpdateDataSources(null, new LogFileSection(0, 0), 0);
			canvas.DataSources.Should().BeEmpty();
		}

		[Test]
		public void TestUpdateLineNumbers2()
		{
			var canvas = new DataSourceCanvas();
			var dataSource = new Mock<IDataSource>();
			canvas.UpdateDataSources(dataSource.Object, new LogFileSection(0, 0), 0);
			canvas.DataSources.Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that the filenames of the original data source are displayed next to each line")]
		public void TestUpdateLineNumbers3()
		{
			var canvas = new DataSourceCanvas();
			var mergedDataSource = new Mock<IMergedDataSource>();
			var dataSource0 = new Mock<IDataSource>();
			dataSource0.Setup(x => x.FullFileName).Returns(@"A:\foo\bar.txt");

			var dataSource1 = new Mock<IDataSource>();
			dataSource1.Setup(x => x.FullFileName).Returns(@"B:\a really long file name.log");

			mergedDataSource.Setup(x => x.OriginalSources).Returns(new[] { dataSource0.Object, dataSource1.Object });
			var mergedLogFile = new Mock<ILogFile>();
			mergedLogFile.Setup(x => x.Count).Returns(2);
			mergedLogFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
				.Callback((LogFileSection section, LogLine[] lines) =>
				{
					lines[0] = new LogLine(0, 0, 0, new LogLineSourceId(1), "foo", LevelFlags.Trace, null);
					lines[1] = new LogLine(1, 1, 1, new LogLineSourceId(0), "bar", LevelFlags.Trace, null);
				});
			mergedDataSource.Setup(x => x.UnfilteredLogFile).Returns(mergedLogFile.Object);

			canvas.UpdateDataSources(mergedDataSource.Object, new LogFileSection(0, 2), 0);
			canvas.DataSources.Should().HaveCount(2);
			canvas.DataSources[0].Should().NotBeNull();
			canvas.DataSources[0].Text.Should().Be("a really long file nam");
			canvas.DataSources[1].Should().NotBeNull();
			canvas.DataSources[1].Text.Should().Be("bar.txt");
		}

		[Test]
		[Description("Verifies that the character codes of the original data source are displayed next to each line")]
		public void TestUpdateLineNumbers4()
		{
			var canvas = new DataSourceCanvas
			{
				DisplayMode = DataSourceDisplayMode.CharacterCode
			};

			var mergedDataSource = new Mock<IMergedDataSource>();
			var dataSource0 = new Mock<IDataSource>();
			dataSource0.Setup(x => x.CharacterCode).Returns("FB");

			var dataSource1 = new Mock<IDataSource>();
			dataSource1.Setup(x => x.CharacterCode).Returns(@"TH");

			mergedDataSource.Setup(x => x.OriginalSources).Returns(new[] { dataSource0.Object, dataSource1.Object });
			var mergedLogFile = new Mock<ILogFile>();
			mergedLogFile.Setup(x => x.Count).Returns(2);
			mergedLogFile.Setup(x => x.GetSection(It.IsAny<LogFileSection>(), It.IsAny<LogLine[]>()))
				.Callback((LogFileSection section, LogLine[] lines) =>
				{
					lines[0] = new LogLine(0, 0, 0, new LogLineSourceId(1), "foo", LevelFlags.Trace, null);
					lines[1] = new LogLine(1, 1, 1, new LogLineSourceId(0), "bar", LevelFlags.Trace, null);
				});
			mergedDataSource.Setup(x => x.UnfilteredLogFile).Returns(mergedLogFile.Object);

			canvas.UpdateDataSources(mergedDataSource.Object, new LogFileSection(0, 2), 0);
			canvas.DataSources.Should().HaveCount(2);
			canvas.DataSources[0].Should().NotBeNull();
			canvas.DataSources[0].Text.Should().Be("TH");
			canvas.DataSources[1].Should().NotBeNull();
			canvas.DataSources[1].Text.Should().Be("FB");
		}
	}
}
