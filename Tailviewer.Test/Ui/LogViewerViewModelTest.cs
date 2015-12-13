using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Settings;
using Tailviewer.Test.BusinessLogic;
using Tailviewer.Ui.ViewModels;

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
		[LocalTest("Won't run on AppVeyor right now - investitage why...")]
		[Description("Verifies listener modifications from previous log files are properly discarded")]
		public void TestFilter1()
		{
			using (var dataSource = new DataSource(new DataSourceSettings(LogFileTest.File20Mb)))
			{
				var model = new LogViewerViewModel(new DataSourceViewModel(dataSource), _dispatcher);
				dataSource.LogFile.Wait();

				model.StringFilter = "i";
				model.CurrentLogFile.Wait();
				// We have waited for that filter operation to finish, HOWEVER, did not invoke the dispatcher.
				// This causes all modifications from that operation to stay in the view-model's queue

				model.StringFilter = "in";
				model.StringFilter = "inf";
				model.StringFilter = "info";

				// Now we wait for the very last filter operation to complete
				model.CurrentLogFile.Wait();
				// And then dispatch ALL events at ONCE.
				// We expect the view model to completely ignore the old changes!
				_dispatcher.InvokeAll();

				model.AllLogEntryCount.Should().Be(165342);
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