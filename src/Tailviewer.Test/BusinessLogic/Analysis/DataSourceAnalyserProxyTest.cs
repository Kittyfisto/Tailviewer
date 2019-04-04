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

		[Test]
		[Description("Verifies that the proxy handles exceptions thrown by the inner analyser")]
		public void TestSetConfiguration1()
		{
			var analyser = new DataSourceAnalyserProxy(_plugin.Object, AnalyserId.CreateNew(), _scheduler, null, null);
			_actualAnalyser.SetupSet(x => x.Configuration).Throws<NullReferenceException>();

			var config = new TestLogAnalyserConfiguration();
			new Action(() => analyser.Configuration = config).ShouldNotThrow();
			_actualAnalyser.VerifySet(x => x.Configuration = config, Times.Once);
		}

		[Test]
		[Description("Verifies that the proxy forwards the configuration to the ")]
		public void TestSetConfiguration2()
		{
			var analyser = new DataSourceAnalyserProxy(_plugin.Object, AnalyserId.CreateNew(), _scheduler, null, null);
			_actualAnalyser.SetupProperty(x => x.Configuration);

			var config = new TestLogAnalyserConfiguration();
			new Action(() => analyser.Configuration = config).ShouldNotThrow();
			_actualAnalyser.Object.Configuration.Should().Be(config);
		}

		[Test]
		[Description("Verifies that the proxy handles exceptions thrown by the inner analyser")]
		public void TestGetConfiguration1()
		{
			var analyser = new DataSourceAnalyserProxy(_plugin.Object, AnalyserId.CreateNew(), _scheduler, null, null);
			_actualAnalyser.Setup(x => x.Configuration).Throws<NullReferenceException>();

			analyser.Configuration.Should().BeNull();
			_actualAnalyser.Verify(x => x.Configuration, Times.Once);
		}

		[Test]
		public void TestGetConfiguration2()
		{
			var analyser = new DataSourceAnalyserProxy(_plugin.Object, AnalyserId.CreateNew(), _scheduler, null, null);
			_actualAnalyser.Setup(x => x.Configuration).Throws<NullReferenceException>();

			var config = new TestLogAnalyserConfiguration();
			_actualAnalyser.Setup(x => x.Configuration).Returns(config);
			analyser.Configuration.Should().BeSameAs(config);
		}
	}
}
