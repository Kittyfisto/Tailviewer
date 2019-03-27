using System;
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

		[SetUp]
		public void Setup()
		{
			_plugin = new Mock<IDataSourceAnalyserPlugin>();
			_actualAnalyser = new Mock<IDataSourceAnalyser>();
			_plugin.Setup(x => x.Create(It.IsAny<AnalyserId>(), It.IsAny<ILogFile>(), It.IsAny<ILogAnalyserConfiguration>()))
				.Returns(() => _actualAnalyser.Object);
		}

		[Test]
		public void TestDispose()
		{
			var analyser = new DataSourceAnalyserProxy(_plugin.Object, AnalyserId.CreateNew(), null, null);

			_actualAnalyser.Setup(x => x.Dispose()).Throws<NullReferenceException>();
			new Action(() => analyser.Dispose()).ShouldNotThrow("because the proxy is supposed to handle failures of its plugin");
			_actualAnalyser.Verify(x => x.Dispose(), Times.Once, "because the proxy should have at least tried to dispose of the inner analyser");
		}

		[Test]
		public void TestAddLogFile()
		{
			var analyser = new DataSourceAnalyserProxy(_plugin.Object, AnalyserId.CreateNew(), null, null);

			_actualAnalyser.Setup(x => x.OnAddLogFile(It.IsAny<ILogFile>())).Throws<NullReferenceException>();
			new Action(() => analyser.OnAddLogFile(null)).ShouldNotThrow("because the proxy is supposed to handle failures of its plugin");
			_actualAnalyser.Verify(x => x.OnAddLogFile(It.IsAny<ILogFile>()), Times.Once, "because the proxy should have at least tried to call AddLogFile on the inner analyser");
		}

		[Test]
		public void TestRemoveLogFile()
		{
			var analyser = new DataSourceAnalyserProxy(_plugin.Object, AnalyserId.CreateNew(), null, null);

			_actualAnalyser.Setup(x => x.OnRemoveLogFile(It.IsAny<ILogFile>())).Throws<NullReferenceException>();
			new Action(() => analyser.OnRemoveLogFile(null)).ShouldNotThrow("because the proxy is supposed to handle failures of its plugin");
			_actualAnalyser.Verify(x => x.OnRemoveLogFile(It.IsAny<ILogFile>()), Times.Once, "because the proxy should have at least tried to call RemoveLogFile on the inner analyser");
		}
	}
}
