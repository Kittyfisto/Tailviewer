using System;
using System.Collections.Generic;
using FluentAssertions;
using Metrolib;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Scheduling;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Settings;
using Tailviewer.Test.BusinessLogic.LogFiles;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class LogViewerViewModelTest
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_scheduler = new TaskScheduler();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_scheduler.Dispose();
		}

		[SetUp]
		public void SetUp()
		{
			_dispatcher = new ManualDispatcher();
		}

		private ManualDispatcher _dispatcher;
		private TaskScheduler _scheduler;

		[Test]
		public void TestDataSourceDoesntExist1()
		{
			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Exists).Returns(false);
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FullFileName).Returns(@"E:\Tailviewer\somefile.log");
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new SingleDataSourceViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Can't find \"somefile.log\"");
			model.NoEntriesSubtext.Should().Be("It was last seen at E:\\Tailviewer");
		}

		[Test]
		[Description("Verifies that the NoEntriesSubtext is cleared when the reason why no entries are showing up changes")]
		public void TestDataSourceDoesntExist2()
		{
			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Exists).Returns(false);
			var filteredLogFile = new Mock<ILogFile>();
			ILogFileListener listener = null;
			filteredLogFile.Setup(x => x.AddListener(It.IsAny<ILogFileListener>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
			               .Callback((ILogFileListener l, TimeSpan t, int i) => listener = l);
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FullFileName).Returns(@"E:\Tailviewer\somefile.log");
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new SingleDataSourceViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Can't find \"somefile.log\"");
			model.NoEntriesSubtext.Should().Be("It was last seen at E:\\Tailviewer");

			logFile.Setup(x => x.Exists).Returns(true);
			listener.OnLogFileModified(logFile.Object, new LogFileSection(0, 0));
			_dispatcher.InvokeAll();

			model.NoEntriesExplanation.Should().Be("The data source is empty");
			model.NoEntriesSubtext.Should().BeNull();
		}

		[Test]
		public void TestDataSourceEmpty()
		{
			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Exists).Returns(true);
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new SingleDataSourceViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("The data source is empty");
			model.NoEntriesSubtext.Should().BeNull();
		}

		[Test]
		[LocalTest("Won't run on AppVeyor right now - investitage why...")]
		[Description("Verifies listener modifications from previous log files are properly discarded")]
		public void TestSearch1()
		{
			using (var dataSource = new SingleDataSource(_scheduler, new DataSource(LogFileTest.File20Mb) {Id = Guid.NewGuid()}))
			{
				var dataSourceModel = new SingleDataSourceViewModel(dataSource);
				var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
				dataSource.UnfilteredLogFile.Wait();

				dataSourceModel.SearchTerm = "i";
				dataSource.FilteredLogFile.Wait();
				// We have waited for that filter operation to finish, HOWEVER, did not invoke the dispatcher.
				// This causes all modifications from that operation to stay in the view-model's queue

				dataSourceModel.SearchTerm = "in";
				dataSourceModel.SearchTerm = "inf";
				dataSourceModel.SearchTerm = "info";

				// Now we wait for the very last filter operation to complete
				dataSource.FilteredLogFile.Wait();
				// And then dispatch ALL events at ONCE.
				// We expect the view model to completely ignore the old changes!
				_dispatcher.InvokeAll();

				/*model.LogEntryCount.Should().Be(5);
				model.LogEntries.Select(x => x.Message)
				     .Should().Equal(new[]
					     {
						     "2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver"
						     ,
						     "2015-10-07 19:50:59,081 [8092, 1] INFO  SharpRemote.SocketRemotingEndPointServer (null) - EndPoint '<Unnamed>' listening on 0.0.0.0:49152"
						     ,
						     "2015-10-07 19:50:59,171 [8092, 6] INFO  SharpRemote.AbstractIPSocketRemotingEndPoint (null) - <Unnamed>: Connected to 127.0.0.1:10348"
						     ,
						     "2015-10-07 19:51:42,481 [8092, EndPoint '<Unnamed>' Socket Reading] INFO  SharpRemote.AbstractSocketRemotingEndPoint (null) - Disconnecting socket '<Unnamed>' from 127.0.0.1:10348: ReadFailure"
						     ,
						     "2015-10-07 19:51:42,483 [8092, 6] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Parent process terminated unexpectedly (exit code: -1), shutting down..."
					     });*/
			}
		}

		[Test]
		public void TestLevelFilter()
		{
			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Exists).Returns(true);
			logFile.Setup(x => x.Count).Returns(1);
			logFile.Setup(x => x.FileSize).Returns(Size.FromBytes(1));
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.None);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new SingleDataSourceViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Not a single log entry matches the level selection");
			model.NoEntriesSubtext.Should().BeNull();
		}

		[Test]
		public void TestQuickFilter()
		{
			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Exists).Returns(true);
			logFile.Setup(x => x.Count).Returns(1);
			logFile.Setup(x => x.FileSize).Returns(Size.FromBytes(1));
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.SearchTerm).Returns("");
			dataSource.Setup(x => x.QuickFilterChain).Returns(new List<ILogEntryFilter> {new Mock<ILogEntryFilter>().Object});
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.All);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new SingleDataSourceViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Not a single log entry matches the activated quick filters");
			model.NoEntriesSubtext.Should().BeNull();
		}

		[Test]
		public void TestStringFilter()
		{
			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Exists).Returns(true);
			logFile.Setup(x => x.Count).Returns(1);
			logFile.Setup(x => x.FileSize).Returns(Size.FromBytes(1));
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.UnfilteredLogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.SearchTerm).Returns("s");
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.All);
			dataSource.Setup(x => x.Search).Returns(new Mock<ILogFileSearch>().Object);

			var dataSourceModel = new SingleDataSourceViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Not a single log entry matches the log file filter");
			model.NoEntriesSubtext.Should().BeNull();
		}
	}
}