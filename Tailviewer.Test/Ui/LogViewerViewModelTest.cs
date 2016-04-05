using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Filters;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Test.BusinessLogic;
using Tailviewer.Ui.ViewModels;
using DataSource = Tailviewer.Settings.DataSource;

namespace Tailviewer.Test.Ui
{
	[TestFixture]
	public sealed class LogViewerViewModelTest
	{
		private ManualDispatcher _dispatcher;

		[SetUp]
		public void SetUp()
		{
			_dispatcher = new ManualDispatcher();
		}

		[Test]
		public void TestDataSourceDoesntExist()
		{
			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Exists).Returns(false);
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.LogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FullFileName).Returns(@"E:\Tailviewer\somefile.log");
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);

			var dataSourceModel = new SingleDataSourceViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Can't find \"somefile.log\"");
			model.NoEntriesSubtext.Should().Be("It was last seen at E:\\Tailviewer");
		}

		[Test]
		public void TestDataSourceEmpty()
		{
			var dataSource = new Mock<IDataSource>();
			var logFile = new Mock<ILogFile>();
			logFile.Setup(x => x.Exists).Returns(true);
			var filteredLogFile = new Mock<ILogFile>();
			dataSource.Setup(x => x.LogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);

			var dataSourceModel = new SingleDataSourceViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("The data source is empty");
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
			dataSource.Setup(x => x.LogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.None);

			var dataSourceModel = new SingleDataSourceViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Not a single log entry matches the level selection");
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
			dataSource.Setup(x => x.LogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.StringFilter).Returns("s");
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.All);

			var dataSourceModel = new SingleDataSourceViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Not a single log entry matches the log file filter");
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
			dataSource.Setup(x => x.LogFile).Returns(logFile.Object);
			dataSource.Setup(x => x.FilteredLogFile).Returns(filteredLogFile.Object);
			dataSource.Setup(x => x.StringFilter).Returns("");
			dataSource.Setup(x => x.QuickFilterChain).Returns(new List<ILogEntryFilter> {new Mock<ILogEntryFilter>().Object});
			dataSource.Setup(x => x.LevelFilter).Returns(LevelFlags.All);

			var dataSourceModel = new SingleDataSourceViewModel(dataSource.Object);
			var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
			model.LogEntryCount.Should().Be(0);
			model.NoEntriesExplanation.Should().Be("Not a single log entry matches the activated quick filters");
		}

		[Test]
		[LocalTest("Won't run on AppVeyor right now - investitage why...")]
		[Description("Verifies listener modifications from previous log files are properly discarded")]
		public void TestFilter1()
		{
			using (var dataSource = new SingleDataSource(new DataSource(LogFileTest.File20Mb){Id = Guid.NewGuid()}))
			{
				var dataSourceModel = new SingleDataSourceViewModel(dataSource);
				var model = new LogViewerViewModel(dataSourceModel, _dispatcher, TimeSpan.Zero);
				dataSource.LogFile.Wait();

				dataSourceModel.StringFilter = "i";
				dataSource.FilteredLogFile.Wait();
				// We have waited for that filter operation to finish, HOWEVER, did not invoke the dispatcher.
				// This causes all modifications from that operation to stay in the view-model's queue

				dataSourceModel.StringFilter = "in";
				dataSourceModel.StringFilter = "inf";
				dataSourceModel.StringFilter = "info";

				// Now we wait for the very last filter operation to complete
				dataSource.FilteredLogFile.Wait();
				// And then dispatch ALL events at ONCE.
				// We expect the view model to completely ignore the old changes!
				_dispatcher.InvokeAll();

				model.LogEntryCount.Should().Be(5);
				model.LogEntries.Select(x => x.Message)
					 .Should().Equal(new[]
					     {
						     "2015-10-07 19:50:58,982 [8092, 1] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Silo Server starting, args (1): \"14056\", without custom type resolver",
						     "2015-10-07 19:50:59,081 [8092, 1] INFO  SharpRemote.SocketRemotingEndPointServer (null) - EndPoint '<Unnamed>' listening on 0.0.0.0:49152",
						     "2015-10-07 19:50:59,171 [8092, 6] INFO  SharpRemote.AbstractIPSocketRemotingEndPoint (null) - <Unnamed>: Connected to 127.0.0.1:10348",
						     "2015-10-07 19:51:42,481 [8092, EndPoint '<Unnamed>' Socket Reading] INFO  SharpRemote.AbstractSocketRemotingEndPoint (null) - Disconnecting socket '<Unnamed>' from 127.0.0.1:10348: ReadFailure",
							 "2015-10-07 19:51:42,483 [8092, 6] INFO  SharpRemote.Hosting.OutOfProcessSiloServer (null) - Parent process terminated unexpectedly (exit code: -1), shutting down..."
					     });
			}
		}
	}
}