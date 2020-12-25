using System;
using System.Collections.Generic;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class LogViewerViewModelTest
	{
		private Mock<IActionCenter> _actionCenter;
		private Mock<IApplicationSettings> _settings;

		[SetUp]
		public void SetUp()
		{
			_actionCenter = new Mock<IActionCenter>();
			_settings = new Mock<IApplicationSettings>();
		}

		[Test]
		public void TestDataSourceDoesntExist1()
		{
			var dataSource = new Mock<ISingleDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.GetValue(LogFileProperties.EmptyReason)).Returns(ErrorFlags.SourceDoesNotExist);
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FullFileName).Returns(@"E:\Tailviewer\somefile.log");
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new FileDataSourceViewModel(dataSource.Object, _actionCenter.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Can't find \"somefile.log\"");
			model.NoEntriesSubtext.Should().Be("It was last seen at E:\\Tailviewer");
		}

		[Test]
		[Description("Verifies that the NoEntriesSubtext is cleared when the reason why no entries are showing up changes")]
		public void TestDataSourceDoesntExist2()
		{
			var dataSource = new Mock<ISingleDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.GetValue(LogFileProperties.EmptyReason)).Returns(ErrorFlags.SourceDoesNotExist);
			logFile.Setup(x => x.GetValue(LogFileProperties.Size)).Returns((Size?)null);
			var filteredLogFile = new Mock<ILogFile>();
			ILogFileListener listener = null;
			filteredLogFile.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			               .Callback((ILogFileListener l, TimeSpan t, int i) => listener = l);
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FullFileName).Returns(@"E:\Tailviewer\somefile.log");
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new FileDataSourceViewModel(dataSource.Object, _actionCenter.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Can't find \"somefile.log\"");
			model.NoEntriesSubtext.Should().Be("It was last seen at E:\\Tailviewer");

			logFile.Setup(x => x.GetValue(LogFileProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.GetValue(LogFileProperties.Size)).Returns(Size.Zero);
			listener.OnLogFileModified(logFile.Object, new LogFileSection(0, 0));
			model.Update();

			model.NoEntriesExplanation.Should().Be("The data source is empty");
			model.NoEntriesSubtext.Should().BeNull();
		}

		[Test]
		public void TestDataSourceCannotBeAccessed1()
		{
			var dataSource = new Mock<ISingleDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.GetValue(LogFileProperties.EmptyReason)).Returns(ErrorFlags.SourceCannotBeAccessed);
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FullFileName).Returns(@"E:\Tailviewer\somefile.log");
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new FileDataSourceViewModel(dataSource.Object, _actionCenter.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Unable to access \"somefile.log\"");
			model.NoEntriesSubtext.Should().Be("The file may be opened exclusively by another process or you are not authorized to view it");
		}

		[Test]
		public void TestDataSourceEmpty()
		{
			var dataSource = new Mock<ISingleDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.GetValue(LogFileProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.GetValue(LogFileProperties.Size)).Returns(Size.Zero);
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new FileDataSourceViewModel(dataSource.Object, _actionCenter.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("The data source is empty");
			model.NoEntriesSubtext.Should().BeNull();
		}

		/// <summary>
		/// </summary>
		/// <remarks>
		///     TODO: This should actually contain every possible combination of values besides <see cref="LevelFlags.All" />
		///     but I'm lazy...
		/// </remarks>
		public static IEnumerable<LevelFlags> NotAll => new[]
		{
			LevelFlags.Other,
			LevelFlags.Trace,
			LevelFlags.Debug,
			LevelFlags.Info,
			LevelFlags.Warning,
			LevelFlags.Error,
			LevelFlags.Fatal,
			LevelFlags.Debug | LevelFlags.Trace,
			LevelFlags.Debug | LevelFlags.Info,
			LevelFlags.Debug | LevelFlags.Warning,
			LevelFlags.Debug | LevelFlags.Error,
			LevelFlags.Debug | LevelFlags.Fatal,
		};

		[Test]
		public void TestLevelFilter1([ValueSource(nameof(NotAll))] LevelFlags flags)
		{
			var dataSource = new Mock<ISingleDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.GetValue(LogFileProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.Count).Returns(1);
			logFile.Setup(x => x.GetValue(LogFileProperties.Size)).Returns(Size.FromBytes(1));
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.LevelFilter).Returns(flags);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new FileDataSourceViewModel(dataSource.Object, _actionCenter.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);

			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Not a single log entry matches the level selection");
			model.NoEntriesSubtext.Should().BeNull();
		}

		[Test]
		public void TestQuickFilter()
		{
			var dataSource = new Mock<ISingleDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.GetValue(LogFileProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.Count).Returns(1);
			logFile.Setup(x => x.GetValue(LogFileProperties.Size)).Returns(Size.FromBytes(1));
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.SearchTerm).Returns("");
			dataSource.Setup(x => x.QuickFilterChain).Returns(new List<ILogEntryFilter> {new Mock<ILogEntryFilter>().Object});
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.All);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new FileDataSourceViewModel(dataSource.Object, _actionCenter.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Not a single log entry matches the activated quick filters");
			model.NoEntriesSubtext.Should().BeNull();
		}

		[Test]
		public void TestStringFilter()
		{
			var dataSource = new Mock<ISingleDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.GetValue(LogFileProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.Count).Returns(1);
			logFile.Setup(x => x.GetValue(LogFileProperties.Size)).Returns(Size.FromBytes(1));
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.SearchTerm).Returns("s");
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.All);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new FileDataSourceViewModel(dataSource.Object, _actionCenter.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Not a single log entry matches the log file filter");
			model.NoEntriesSubtext.Should().BeNull();
		}

		[Test]
		public void TestDataSourceNoFiles()
		{
			var dataSource = new Mock<IFolderDataSource>();
			var logFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(0);
			dataSource.Setup(x => x.FilteredFileCount).Returns(0);
			dataSource.Setup(x => x.OriginalSources).Returns(new List<IDataSource>());
			dataSource.Setup(x => x.FullFileName).Returns(@"F:\logs\today");

			var dataSourceModel = new FolderDataSourceViewModel(dataSource.Object, _actionCenter.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("The folder \"today\" does not contain any file");
			model.NoEntriesSubtext.Should().Be(@"F:\logs\today");
		}

		[Test]
		public void TestDataSourceNoMatchingFiles()
		{
			var dataSource = new Mock<IFolderDataSource>();
			var logFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(1);
			dataSource.Setup(x => x.FilteredFileCount).Returns(0);
			dataSource.Setup(x => x.OriginalSources).Returns(new List<IDataSource>());
			dataSource.Setup(x => x.FullFileName).Returns(@"C:\logs\yesterday");
			dataSource.Setup(x => x.LogFileSearchPattern).Returns("*.foo");

			var dataSourceModel = new FolderDataSourceViewModel(dataSource.Object, _actionCenter.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("The folder \"yesterday\" does not contain any file matching \"*.foo\"");
			model.NoEntriesSubtext.Should().Be(@"C:\logs\yesterday");
		}
	}
}