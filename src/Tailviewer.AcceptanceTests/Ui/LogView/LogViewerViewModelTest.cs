using System;
using System.Diagnostics.Contracts;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.AcceptanceTests.BusinessLogic.Sources.Text;
using Tailviewer.BusinessLogic.ActionCenter;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Sources;
using Tailviewer.Core.Properties;
using Tailviewer.Settings;
using Tailviewer.Test;
using Tailviewer.Ui.DataSourceTree;
using Tailviewer.Ui.LogView;

namespace Tailviewer.AcceptanceTests.Ui.LogView
{
	[TestFixture]
	public sealed class LogViewerViewModelTest
	{
		private Mock<IActionCenter> _actionCenter;
		private ILogFileFactory _logFileFactory;
		private DefaultTaskScheduler _taskScheduler;
		private ApplicationSettings _settings;

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			_taskScheduler = new DefaultTaskScheduler();
			_logFileFactory = new SimplePluginLogFileFactory(_taskScheduler);
			_actionCenter = new Mock<IActionCenter>();
			_settings = new ApplicationSettings(PathEx.GetTempFileName());
		}

		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
			_taskScheduler.Dispose();
		}

		[SetUp]
		public void SetUp()
		{
		}

		[Pure]
		private FileDataSourceViewModel CreateViewModel(FileDataSource dataSource)
		{
			return new FileDataSourceViewModel(dataSource, _actionCenter.Object, _settings);
		}

		[Test]
		[Description("Verifies listener modifications from previous log files are properly discarded")]
		public void TestSearch1()
		{
			using (
				var dataSource = new FileDataSource(_logFileFactory, _taskScheduler,
					new DataSource(AbstractTextLogSourceAcceptanceTest.File20Mb) {Id = DataSourceId.CreateNew()}))
			{
				var dataSourceModel = CreateViewModel(dataSource);
				var model = new LogViewerViewModel(dataSourceModel, _actionCenter.Object, _settings, TimeSpan.Zero);

				dataSourceModel.Search.Term = "i";
				dataSource.FilteredLogSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(20)).Be(Percentage.HundredPercent);
				// We have waited for that filter operation to finish, HOWEVER, did not invoke the dispatcher.
				// This causes all modifications from that operation to stay in the view-model's queue

				dataSourceModel.Search.Term = "in";
				dataSourceModel.Search.Term = "inf";
				dataSourceModel.Search.Term = "info";

				// Now we wait for the very last filter operation to complete
				dataSource.FilteredLogSource.Property(x => x.GetProperty(GeneralProperties.PercentageProcessed)).ShouldAfter(TimeSpan.FromSeconds(20)).Be(Percentage.HundredPercent);
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