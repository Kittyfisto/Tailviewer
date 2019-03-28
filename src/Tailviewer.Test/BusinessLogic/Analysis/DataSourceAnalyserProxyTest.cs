using System;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Test.BusinessLogic.Analysis
{
	[TestFixture]
	public sealed class DataSourceAnalyserProxyTest
	{
		private Mock<IDataSourceAnalyserPlugin> _plugin;
		private Mock<IDataSourceAnalyser> _actualAnalyser;
		private ManualTaskScheduler _scheduler;

		[SetUp]
		public void Setup()
		{
			_plugin = new Mock<IDataSourceAnalyserPlugin>();
			_actualAnalyser = new Mock<IDataSourceAnalyser>();
			_plugin.Setup(x => x.Create(It.IsAny<AnalyserId>(), It.IsAny<ITaskScheduler>(), It.IsAny<ILogFile>(), It.IsAny<ILogAnalyserConfiguration>()))
				.Returns(() => _actualAnalyser.Object);
			_scheduler = new ManualTaskScheduler();
		}

		[Test]
		public void TestDispose()
		{
			var analyser = new DataSourceAnalyserProxy(_plugin.Object, AnalyserId.CreateNew(), _scheduler, null, null);

			_actualAnalyser.Setup(x => x.Dispose()).Throws<NullReferenceException>();
			new Action(() => analyser.Dispose()).ShouldNotThrow("because the proxy is supposed to handle failures of its plugin");
			_actualAnalyser.Verify(x => x.Dispose(), Times.Once, "because the proxy should have at least tried to dispose of the inner analyser");
		}

		[Test]
		public void TestAddLogFile()
		{
			var analyser = new DataSourceAnalyserProxy(_plugin.Object, AnalyserId.CreateNew(), _scheduler, null, null);

			_actualAnalyser.Setup(x => x.OnLogFileAdded(It.IsAny<DataSourceId>(), It.IsAny<ILogFile>())).Throws<NullReferenceException>();
			var id = DataSourceId.CreateNew();
			var logFile = new Mock<ILogFile>().Object;
			new Action(() => analyser.OnLogFileAdded(id, logFile)).ShouldNotThrow("because the proxy is supposed to handle failures of its plugin");
			_actualAnalyser.Verify(x => x.OnLogFileAdded(id, logFile), Times.Once, "because the proxy should have at least tried to call AddLogFile on the inner analyser");
		}

		[Test]
		public void TestRemoveLogFile()
		{
			var analyser = new DataSourceAnalyserProxy(_plugin.Object, AnalyserId.CreateNew(), _scheduler, null, null);

			_actualAnalyser.Setup(x => x.OnLogFileRemoved(It.IsAny<DataSourceId>(), It.IsAny<ILogFile>())).Throws<NullReferenceException>();
			var id = DataSourceId.CreateNew();
			var logFile = new Mock<ILogFile>().Object;
			new Action(() => analyser.OnLogFileRemoved(id, logFile)).ShouldNotThrow("because the proxy is supposed to handle failures of its plugin");
			_actualAnalyser.Verify(x => x.OnLogFileRemoved(id, logFile), Times.Once, "because the proxy should have at least tried to call RemoveLogFile on the inner analyser");
		}
	}
}
