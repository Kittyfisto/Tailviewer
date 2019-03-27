using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Archiver.Plugins;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Test.BusinessLogic.Analysis
{
	[TestFixture]
	public sealed class DataSourceAnalyserEngineTest
	{
		private Mock<ILogAnalyserEngine> _logAnalyserEngine;

		[SetUp]
		public void Setup()
		{
			_logAnalyserEngine = new Mock<ILogAnalyserEngine>();
			_logAnalyserEngine.Setup(x => x.CreateAnalysis(It.IsAny<ILogFile>(),
			                                               It.IsAny<DataSourceAnalysisConfiguration>(),
			                                               It.IsAny<IDataSourceAnalysisListener>()))
			                  .Returns(() => new TestLogAnalyser());
		}

		[Test]
		public void TestCreateAnalyser()
		{
			var engine = new DataSourceAnalyserEngine(_logAnalyserEngine.Object);
			var logFile = new Mock<ILogFile>();
			var analyser = engine.CreateAnalyser(logFile.Object, new AnalyserTemplate
			{
				LogAnalyserPluginId = new LogAnalyserFactoryId("Some Log Analyser Plugin")
			});
			analyser.Should().NotBeNull();
			_logAnalyserEngine.Verify(x => x.CreateAnalysis(logFile.Object, It.IsAny<DataSourceAnalysisConfiguration>(), It.IsAny<IDataSourceAnalysisListener>()),
			                          Times.Once, "because we have specified a LogAnalyserPluginId and thus the corresponding engine should've been used to perform the analysis");
		}

		[Test]
		public void TestCreateCustomDataSourceAnalyser()
		{
			var id = new DataSourceAnalyserPluginId("Some Data Source Analyser Plugin");
			var plugin = new Mock<IDataSourceAnalyserPlugin>();
			plugin.Setup(x => x.Id).Returns(id);
			var engine = new DataSourceAnalyserEngine(_logAnalyserEngine.Object, CreatePluginLoader(plugin.Object));
			var logFile = new Mock<ILogFile>();
			var configuration = new TestLogAnalyserConfiguration();
			var analyser = engine.CreateAnalyser(logFile.Object, new AnalyserTemplate
			{
				DataSourceAnalyserPluginId = id,
				Configuration = configuration
			});
			analyser.Should().NotBeNull();

			_logAnalyserEngine.Verify(x => x.CreateAnalysis(It.IsAny<ILogFile>(), It.IsAny<DataSourceAnalysisConfiguration>(), It.IsAny<IDataSourceAnalysisListener>()),
			                          Times.Never, "because we have specified a DataSourceAnalyserPluginId and thus the corresponding plugin should've been used to create the data source analyser");
			plugin.Verify(x => x.Create(It.IsAny<AnalyserId>(), logFile.Object, configuration),
			              Times.Once, "because we have specified a DataSourceAnalyserPluginId and thus its plugin should've been loaded and used to create the analyser");
		}

		[Test]
		public void TestCreateCustomDataSourceAnalyserNotFound()
		{
			var plugin = new Mock<IDataSourceAnalyserPlugin>();
			plugin.Setup(x => x.Id).Returns(new DataSourceAnalyserPluginId("Plugin A"));
			var engine = new DataSourceAnalyserEngine(_logAnalyserEngine.Object, CreatePluginLoader(plugin.Object));
			var logFile = new Mock<ILogFile>();
			var configuration = new TestLogAnalyserConfiguration();
			var analyser = engine.CreateAnalyser(logFile.Object, new AnalyserTemplate
			{
				DataSourceAnalyserPluginId = new DataSourceAnalyserPluginId("Plugin B"),
				Configuration = configuration
			});
			analyser.Should().NotBeNull();
			analyser.Should().BeOfType<NoAnalyser>();

			_logAnalyserEngine.Verify(x => x.CreateAnalysis(It.IsAny<ILogFile>(), It.IsAny<DataSourceAnalysisConfiguration>(), It.IsAny<IDataSourceAnalysisListener>()),
			                          Times.Never, "because we have specified a DataSourceAnalyserPluginId and thus the corresponding plugin should've been used to create the data source analyser");
			plugin.Verify(x => x.Create(It.IsAny<AnalyserId>(), logFile.Object, configuration),
			              Times.Never, "because that plugin has a different id than we one we supplied with the analyser template");
		}

		[Test]
		public void TestRemoveAnalyser()
		{
			var id = new DataSourceAnalyserPluginId("Some Data Source Analyser Plugin");
			var plugin = new Mock<IDataSourceAnalyserPlugin>();
			plugin.Setup(x => x.Id).Returns(id);
			var pluginAnalyser = new Mock<IDataSourceAnalyser>();
			plugin.Setup(x => x.Create(It.IsAny<AnalyserId>(),
			                           It.IsAny<ILogFile>(),
			                           It.IsAny<ILogAnalyserConfiguration>()))
			      .Returns(pluginAnalyser.Object);
			var engine = new DataSourceAnalyserEngine(_logAnalyserEngine.Object, CreatePluginLoader(plugin.Object));
			var logFile = new Mock<ILogFile>();
			var configuration = new TestLogAnalyserConfiguration();
			var analyser = engine.CreateAnalyser(logFile.Object, new AnalyserTemplate
			{
				DataSourceAnalyserPluginId = id,
				Configuration = configuration
			});
			analyser.Should().NotBeNull();

			pluginAnalyser.Verify(x => x.Dispose(), Times.Never, "because the analyser is still in used and thus may not have been disposed of yet");
			engine.RemoveAnalyser(analyser);
			pluginAnalyser.Verify(x => x.Dispose(), Times.Once, "because now that the analyser has been removed, it should've been disposed of");
		}

		private IPluginLoader CreatePluginLoader(IDataSourceAnalyserPlugin pluginObject)
		{
			var loader = new PluginRegistry();
			loader.Register(pluginObject);
			return loader;
		}
	}
}
