using System.Threading;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.Analysis
{
	[TestFixture]
	public sealed class LogAnalyserProxyTest
	{
		private ManualTaskScheduler _scheduler;

		[SetUp]
		public void Setup()
		{
			_scheduler = new ManualTaskScheduler();
		}

		[Test]
		public void TestConstructionWithoutConfiguration()
		{
			var logFile = new InMemoryLogFile();
			var plugin = new Mock<ILogAnalyserPlugin>();
			var listener = new Mock<IDataSourceAnalysisListener>();
			var proxy = new LogAnalyserProxy(_scheduler, logFile, plugin.Object, null, listener.Object);
			proxy.Start();

			plugin.Verify(x => x.Create(It.IsAny<ITaskScheduler>(), It.IsAny<ILogFile>(), It.IsAny<ILogAnalyserConfiguration>()),
				Times.Once, "because the proxy should try to create an analyser even when there's no configuration for it");
		}
	}
}
