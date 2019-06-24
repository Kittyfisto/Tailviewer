using System.Threading;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.Analysis
{
	[TestFixture]
	public sealed class LogAnalyserProxyTest
	{
		private ServiceContainer _services;
		private ManualTaskScheduler _scheduler;

		[SetUp]
		public void Setup()
		{
			_services = new ServiceContainer();
			_scheduler = new ManualTaskScheduler();
			_services.RegisterInstance<ITaskScheduler>(_scheduler);
		}

		[Test]
		public void TestConstructionWithoutConfiguration()
		{
			var logFile = new InMemoryLogFile();
			var plugin = new Mock<ILogAnalyserPlugin>();
			var listener = new Mock<IDataSourceAnalysisListener>();
			var proxy = new LogAnalyserProxy(_services, logFile, plugin.Object, null, listener.Object);
			proxy.Start();

			plugin.Verify(x => x.Create(It.IsAny<IServiceContainer>(), It.IsAny<ILogFile>(), It.IsAny<ILogAnalyserConfiguration>()),
				Times.Once, "because the proxy should try to create an analyser even when there's no configuration for it");
		}
	}
}
