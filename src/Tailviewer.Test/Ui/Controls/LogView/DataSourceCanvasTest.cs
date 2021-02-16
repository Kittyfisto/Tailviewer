﻿using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Columns;
using Tailviewer.Core.Sources;
using Tailviewer.Settings;
using Tailviewer.Ui.Controls.LogView.DataSource;

namespace Tailviewer.Test.Ui.Controls.LogView
{
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public sealed class DataSourceCanvasTest
	{
		[Test]
		public void TestCtor()
		{
			var canvas = new DataSourceCanvas(TextSettings.Default);
			canvas.DataSources.Should().BeEmpty();
			canvas.DisplayMode.Should().Be(DataSourceDisplayMode.Filename);
		}

		[Test]
		public void TestUpdateLineNumbers1()
		{
			var canvas = new DataSourceCanvas(TextSettings.Default);
			canvas.UpdateDataSources(null, new LogFileSection(0, 0), 0);
			canvas.DataSources.Should().BeEmpty();
		}

		[Test]
		public void TestUpdateLineNumbers2()
		{
			var canvas = new DataSourceCanvas(TextSettings.Default);
			var dataSource = new Mock<IDataSource>();
			canvas.UpdateDataSources(dataSource.Object, new LogFileSection(0, 0), 0);
			canvas.DataSources.Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that the filenames of the original data source are displayed next to each line")]
		public void TestUpdateLineNumbers3()
		{
			var canvas = new DataSourceCanvas(TextSettings.Default);
			var multiDataSource = new Mock<IMultiDataSource>();
			var dataSource0 = new Mock<IDataSource>();
			dataSource0.Setup(x => x.FullFileName).Returns(@"A:\foo\bar.txt");

			var dataSource1 = new Mock<IDataSource>();
			dataSource1.Setup(x => x.FullFileName).Returns(@"B:\a really long file name.log");

			multiDataSource.Setup(x => x.OriginalSources).Returns(new[] { dataSource0.Object, dataSource1.Object });
			var mergedLogFile = new InMemoryLogSource(LogColumns.SourceId);
			mergedLogFile.Add(new Dictionary<IColumnDescriptor, object>
			{
				{LogColumns.SourceId, new LogLineSourceId(1) },
				{LogColumns.RawContent, "foo" }
			});
			mergedLogFile.Add(new Dictionary<IColumnDescriptor, object>
			{
				{LogColumns.SourceId, new LogLineSourceId(0) },
				{LogColumns.RawContent, "bar" }
			});
			multiDataSource.Setup(x => x.FilteredLogSource).Returns(mergedLogFile);

			canvas.UpdateDataSources(multiDataSource.Object, new LogFileSection(0, 2), 0);
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
			var canvas = new DataSourceCanvas(TextSettings.Default)
			{
				DisplayMode = DataSourceDisplayMode.CharacterCode
			};

			var multiDataSource = new Mock<IMultiDataSource>();
			var dataSource0 = new Mock<IDataSource>();
			dataSource0.Setup(x => x.CharacterCode).Returns("FB");

			var dataSource1 = new Mock<IDataSource>();
			dataSource1.Setup(x => x.CharacterCode).Returns(@"TH");

			multiDataSource.Setup(x => x.OriginalSources).Returns(new[] { dataSource0.Object, dataSource1.Object });
			var mergedLogFile = new InMemoryLogSource(LogColumns.SourceId);
			mergedLogFile.Add(new Dictionary<IColumnDescriptor, object>
			{
				{LogColumns.SourceId, new LogLineSourceId(1) },
				{LogColumns.RawContent, "foo" }
			});
			mergedLogFile.Add(new Dictionary<IColumnDescriptor, object>
			{
				{LogColumns.SourceId, new LogLineSourceId(0) },
				{LogColumns.RawContent, "bar" }
			});
			multiDataSource.Setup(x => x.FilteredLogSource).Returns(mergedLogFile);

			canvas.UpdateDataSources(multiDataSource.Object, new LogFileSection(0, 2), 0);
			canvas.DataSources.Should().HaveCount(2);
			canvas.DataSources[0].Should().NotBeNull();
			canvas.DataSources[0].Text.Should().Be("TH");
			canvas.DataSources[1].Should().NotBeNull();
			canvas.DataSources[1].Text.Should().Be("FB");
		}

		[Test]
		public void TestUpdateNoSources([Values(DataSourceDisplayMode.Filename, DataSourceDisplayMode.CharacterCode)] DataSourceDisplayMode displayMode)
		{
			var canvas = new DataSourceCanvas(TextSettings.Default)
			{
				DisplayMode = displayMode
			};
			var folderDataSource = new Mock<IFolderDataSource>();
			folderDataSource.Setup(x => x.FilteredLogSource).Returns(new Mock<ILogSource>().Object);
			var dataSource = new Mock<IDataSource>();
			folderDataSource.Setup(x => x.OriginalSources).Returns(new List<IDataSource>{dataSource.Object});

			new Action(() => canvas.UpdateDataSources(folderDataSource.Object, new LogFileSection(0, 2), 0))
				.Should().NotThrow();
		}
	}
}
