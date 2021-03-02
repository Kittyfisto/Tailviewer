using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Api.Tests;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core.Filters;
using Tailviewer.Core.Properties;
using Tailviewer.Settings;
using Tailviewer.Ui.DataSourceTree;
using Tailviewer.Ui.LogView;

namespace Tailviewer.Tests.Ui
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

		[Pure]
		private FolderDataSourceViewModel CreateFolderViewModel(IFolderDataSource dataSource)
		{
			return new FolderDataSourceViewModel(dataSource, _actionCenter.Object, _settings.Object);
		}

		[Pure]
		private FileDataSourceViewModel CreateFileViewModel(IFileDataSource dataSource)
		{
			return new FileDataSourceViewModel(dataSource, _actionCenter.Object, _settings.Object);
		}

		[Test]
		public void TestDataSourceDoesntExist1()
		{
			var dataSource = new Mock<IFileDataSource>();
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.SourceDoesNotExist);
			var filteredLogFile = new Mock<ILogSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.FullFileName).Returns(@"E:\Tailviewer\somefile.log");
			dataSource.Setup(x => x.FilteredLogSource).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogSourceSearch>().Object);

			var dataSourceModel = CreateFileViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Data source does not exist");
			model.NoEntriesAction.Should().Be("The data source 'somefile.log' was last seen E:\\Tailviewer");
		}

		[Test]
		[Description("Verifies that the NoEntriesSubtext is cleared when the reason why no entries are showing up changes")]
		public void TestDataSourceDoesntExist2()
		{
			var dataSource = new Mock<IFileDataSource>();
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.SourceDoesNotExist);
			logFile.Setup(x => x.GetProperty(GeneralProperties.Size)).Returns((Size?)null);
			var filteredLogFile = new Mock<ILogSource>();
			ILogSourceListener listener = null;
			filteredLogFile.Setup(x => x.AddListener(It.IsAny<ILogSourceListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			               .Callback((ILogSourceListener l, TimeSpan t, int i) => listener = l);
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.FullFileName).Returns(@"E:\Tailviewer\somefile.log");
			dataSource.Setup(x => x.FilteredLogSource).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogSourceSearch>().Object);

			var dataSourceModel = CreateFileViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Data source does not exist");
			model.NoEntriesAction.Should().Be("The data source 'somefile.log' was last seen E:\\Tailviewer");

			logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.GetProperty(GeneralProperties.Size)).Returns(Size.Zero);
			listener.OnLogFileModified(logFile.Object, LogSourceModification.Appended(0, 0));
			model.Update();

			model.NoEntriesExplanation.Should().Be("Data source is empty");
			model.NoEntriesAction.Should().BeNull();
		}

		[Test]
		public void TestDataSourceCannotBeAccessed1()
		{
			var dataSource = new Mock<IFileDataSource>();
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.SourceCannotBeAccessed);
			var filteredLogFile = new Mock<ILogSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.FullFileName).Returns(@"E:\Tailviewer\somefile.log");
			dataSource.Setup(x => x.FilteredLogSource).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogSourceSearch>().Object);

			var dataSourceModel = CreateFileViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Data source cannot be opened");
			model.NoEntriesAction.Should().Be("The file 'somefile.log' may be opened exclusively by another process or you are not authorized to view it");
		}

		[Test]
		public void TestDataSourceEmpty()
		{
			var dataSource = new Mock<IFileDataSource>();
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.GetProperty(GeneralProperties.Size)).Returns(Size.Zero);
			var filteredLogFile = new Mock<ILogSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogSource).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogSourceSearch>().Object);

			var dataSourceModel = CreateFileViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Data source is empty");
			model.NoEntriesAction.Should().BeNull();
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
			var dataSource = new Mock<IFileDataSource>();
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.GetProperty(GeneralProperties.LogEntryCount)).Returns(1);
			logFile.Setup(x => x.GetProperty(GeneralProperties.Size)).Returns(Size.FromBytes(1));
			var filteredLogFile = new Mock<ILogSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogSource).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.LevelFilter).Returns(flags);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogSourceSearch>().Object);

			var dataSourceModel = CreateFileViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);

			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Nothing matches level filter");
			model.NoEntriesAction.Should().Be("Try filtering by different levels or display everything regardless of its level again");
		}

		[Test]
		public void TestQuickFilter()
		{
			var dataSource = new Mock<IFileDataSource>();
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.GetProperty(GeneralProperties.LogEntryCount)).Returns(1);
			logFile.Setup(x => x.GetProperty(GeneralProperties.Size)).Returns(Size.FromBytes(1));
			var filteredLogFile = new Mock<ILogSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogSource).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.SearchTerm).Returns("");
			dataSource.Setup(x => x.LogEntryFilter).Returns(new SubstringFilter("foo", true));
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.All);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogSourceSearch>().Object);

			var dataSourceModel = CreateFileViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Nothing matches quick filter");
			model.NoEntriesAction.Should().Be("Try filtering by different terms or disable all quick filters");
		}

		[Test]
		public void TestStringFilter()
		{
			var dataSource = new Mock<IFileDataSource>();
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.GetProperty(GeneralProperties.LogEntryCount)).Returns(1);
			logFile.Setup(x => x.GetProperty(GeneralProperties.Size)).Returns(Size.FromBytes(1));
			var filteredLogFile = new Mock<ILogSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogSource).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.LogEntryFilter).Returns(new SubstringFilter("s", true));
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.All);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogSourceSearch>().Object);

			var dataSourceModel = CreateFileViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Nothing matches quick filter");
			model.NoEntriesAction.Should().Be("Try filtering by different terms or disable all quick filters");
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/307")]
		public void TestTimeFilter()
		{
			var dataSource = new Mock<IFileDataSource>();
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.GetProperty(GeneralProperties.LogEntryCount)).Returns(1);
			logFile.Setup(x => x.GetProperty(GeneralProperties.Size)).Returns(Size.FromBytes(1));
			var filteredLogFile = new Mock<ILogSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogSource).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.LogEntryFilter).Returns(FilterExpression.Parse("today contains $timestamp"));
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.All);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogSourceSearch>().Object);

			var dataSourceModel = CreateFileViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Nothing matches quick filter");
			model.NoEntriesAction.Should().Be("No log entry matches \"today contains $timestamp\".\r\nTry changing your filter(s) or disable them again");
		}

		[Test]
		public void TestDataSourceNoFiles()
		{
			var dataSource = new Mock<IFolderDataSource>();
			var logFile = new Mock<ILogSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(0);
			dataSource.Setup(x => x.FilteredFileCount).Returns(0);
			dataSource.Setup(x => x.OriginalSources).Returns(new List<IDataSource>());
			dataSource.Setup(x => x.FullFileName).Returns(@"F:\logs\today");

			var dataSourceModel = CreateFolderViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("The folder \"today\" does not contain any file");
			model.NoEntriesAction.Should().Be(@"F:\logs\today");
		}

		[Test]
		public void TestDataSourceNoMatchingFiles()
		{
			var dataSource = new Mock<IFolderDataSource>();
			var logFile = new Mock<ILogSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.UnfilteredFileCount).Returns(1);
			dataSource.Setup(x => x.FilteredFileCount).Returns(0);
			dataSource.Setup(x => x.OriginalSources).Returns(new List<IDataSource>());
			dataSource.Setup(x => x.FullFileName).Returns(@"C:\logs\yesterday");
			dataSource.Setup(x => x.LogFileSearchPattern).Returns("*.foo");

			var dataSourceModel = CreateFolderViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("The folder \"yesterday\" does not contain any file matching \"*.foo\"");
			model.NoEntriesAction.Should().Be(@"C:\logs\yesterday");
		}

		[Test]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/308")]
		public void TestScreenCleared()
		{
			var dataSource = new Mock<IFileDataSource>();
			var logFile = new Mock<ILogSource>();
			logFile.Setup(x => x.GetProperty(GeneralProperties.EmptyReason)).Returns(ErrorFlags.None);
			logFile.Setup(x => x.GetProperty(GeneralProperties.LogEntryCount)).Returns(1);
			logFile.Setup(x => x.GetProperty(GeneralProperties.Size)).Returns(Size.FromBytes(1));
			var filteredLogFile = new Mock<ILogSource>();
			dataSource.Setup(x => x.UnfilteredLogSource).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogSource).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.SearchTerm).Returns("");
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.All);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogSourceSearch>().Object);
			dataSource.Setup(x => x.ScreenCleared).Returns(true);
			dataSource.Setup(x => x.QuickFilterChain).Returns((IEnumerable<ILogEntryFilter>)null);

			var dataSourceViewModel = new Mock<IFileDataSourceViewModel>();
			dataSourceViewModel.Setup(x => x.DataSource).Returns(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceViewModel.Object, _actionCenter.Object, _settings.Object, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("The screen was cleared");
			model.NoEntriesAction.Should().Be("No new log entries have been added to the data source since the screen was cleared. Try waiting for a little longer or show all log entries again.");
		}
	}
}