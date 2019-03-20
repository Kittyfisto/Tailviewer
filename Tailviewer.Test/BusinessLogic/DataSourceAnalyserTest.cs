using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class DataSourceAnalyserTest
	{
		private InMemoryLogFile _logFile;
		private Mock<ILogAnalyserEngine> _engine;
		private AnalyserTemplate _template;

		[SetUp]
		public void Setup()
		{
			_template = new AnalyserTemplate
			{
				Configuration = new TestLogAnalyserConfiguration()
			};

			_logFile = new InMemoryLogFile();
			_engine = new Mock<ILogAnalyserEngine>();
			_engine.Setup(x => x.CreateAnalysis(It.IsAny<ILogFile>(), It.IsAny<DataSourceAnalysisConfiguration>(),
			                                    It.IsAny<IDataSourceAnalysisListener>()))
			       .Returns(() => new TestLogAnalyser());
		}

		[Test]
		public void TestConstruction()
		{
			var analyser = new DataSourceAnalyser(_template, _logFile, _engine.Object);
			analyser.Configuration.Should().BeSameAs(_template.Configuration);
		}
	}
}
