using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.AcceptanceTests.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Settings;
using Tailviewer.Ui.ViewModels;

namespace Tailviewer.AcceptanceTests.Ui.ViewModels
{
	[TestFixture]
	public sealed class LogViewerViewModelTest
	{
		private Mock<IActionCenter> _actionCenter;
		private ILogFileFactory _logFileFactory;
		private DefaultTaskScheduler _scheduler;
		private ApplicationSettings _settings;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			_scheduler = new DefaultTaskScheduler();
			_logFileFactory = new PluginLogFileFactory(_scheduler);
			_actionCenter = new Mock<IActionCenter>();
			_settings = new ApplicationSettings(Path.GetTempFileName());
		}

		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
			_scheduler.Dispose();
		}

		[SetUp]
		public void SetUp()
		{
		}

		[Test]
		[Description("Verifies listener modifications from previous log files are properly discarded")]
		public void TestSearch1()
		{
			using (
				var dataSource = new SingleDataSource(_logFileFactory, _scheduler,
					new DataSource(TextLogFileAcceptanceTest.File20Mb) {Id = DataSourceId.CreateNew()}))
			{
				var dataSourceModel = new SingleDataSourceViewModel(dataSource, _actionCenter.Object);
				var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings, TimeSpan.Zero);

				dataSourceModel.SearchTerm = "i";
				dataSource.FilteredLogFile.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue(TimeSpan.FromSeconds(20));
				// We have waited for that filter operation to finish, HOWEVER, did not invoke the dispatcher.
				// This causes all modifications from that operation to stay in the view-model's queue

				dataSourceModel.SearchTerm = "in";
				dataSourceModel.SearchTerm = "inf";
				dataSourceModel.SearchTerm = "info";

				// Now we wait for the very last filter operation to complete
				dataSource.FilteredLogFile.Property(x => x.EndOfSourceReached).ShouldEventually().BeTrue(TimeSpan.FromSeconds(20));
				// And then dispatch ALL events at ONCE.
				// We expect the view model to completely ignore the old changes!
				model.Update();

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
	}
}