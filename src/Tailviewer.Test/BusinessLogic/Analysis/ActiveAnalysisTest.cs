using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Test.BusinessLogic.Analysis
{
	[TestFixture]
	public sealed class ActiveAnalysisTest
	{
		private ManualTaskScheduler _taskScheduler;
		private Mock<ILogAnalyserEngine> _logAnalyserEngine;
		private DataSourceAnalyserEngine _dataSourceAnalyserEngine;
		private AnalysisTemplate _template;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();
			_logAnalyserEngine = new Mock<ILogAnalyserEngine>();
			_logAnalyserEngine.Setup(x => x.CreateAnalysis(It.IsAny<ILogFile>(),
					It.IsAny<DataSourceAnalysisConfiguration>(),
					It.IsAny<IDataSourceAnalysisListener>()))
				.Returns(() => new Mock<IDataSourceAnalysisHandle>().Object);
			_dataSourceAnalyserEngine = new DataSourceAnalyserEngine(_logAnalyserEngine.Object);

			_template = new AnalysisTemplate();
		}

		[Test]
		public void TestAddLogFile()
		{
			var engine = new Mock<IDataSourceAnalyserEngine>();
			var analyser = new Mock<IDataSourceAnalyser>();
			engine.Setup(x => x.CreateAnalyser(It.IsAny<ILogFile>(), It.IsAny<AnalyserTemplate>()))
			      .Returns(analyser.Object);

			var activeAnalysis = new ActiveAnalysis(AnalysisId.CreateNew(), _template, _taskScheduler, engine.Object, TimeSpan.Zero);

			activeAnalysis.Add(AnalyserPluginId.Empty, new TestLogAnalyserConfiguration());

			analyser.Verify(x => x.OnLogFileAdded(It.IsAny<ILogFile>()), Times.Never, "because we haven't added any log file for analysis just yet");

			var logFile = new Mock<ILogFile>();
			activeAnalysis.Add(logFile.Object);
			analyser.Verify(x => x.OnLogFileAdded(logFile.Object), Times.Once, "because we've just added a log file for analysis and thus the analyser should have been notified");
		}

		[Test]
		public void TestRemoveLogFile()
		{
			var engine = new Mock<IDataSourceAnalyserEngine>();
			var analyser = new Mock<IDataSourceAnalyser>();
			engine.Setup(x => x.CreateAnalyser(It.IsAny<ILogFile>(), It.IsAny<AnalyserTemplate>()))
			      .Returns(analyser.Object);

			var activeAnalysis = new ActiveAnalysis(AnalysisId.CreateNew(), _template, _taskScheduler, engine.Object, TimeSpan.Zero);

			activeAnalysis.Add(AnalyserPluginId.Empty, new TestLogAnalyserConfiguration());

			analyser.Verify(x => x.OnLogFileRemoved(It.IsAny<ILogFile>()), Times.Never, "because we haven't removed any log file from analysis just yet");

			var logFile = new Mock<ILogFile>();
			activeAnalysis.Add(logFile.Object);
			analyser.Verify(x => x.OnLogFileRemoved(It.IsAny<ILogFile>()), Times.Never, "because we haven't removed any log file from analysis just yet");

			activeAnalysis.Remove(logFile.Object);
			analyser.Verify(x => x.OnLogFileRemoved(logFile.Object), Times.Once, "because we've just removed a log file from analysis and thus the analyser should have been notified");
		}

		[Test]
		public void TestConstructionEmptyTemplate()
		{
			var activeAnalysis = new ActiveAnalysis(AnalysisId.CreateNew(), _template, _taskScheduler, _dataSourceAnalyserEngine, TimeSpan.Zero);
			activeAnalysis.Analysers.Should().BeEmpty();
		}

		[Test]
		public void TestConstructionTwoAnalysers()
		{
			_template.Add(new AnalyserTemplate());
			_template.Add(new AnalyserTemplate());

			var activeAnalysis = new ActiveAnalysis(AnalysisId.CreateNew(), _template, _taskScheduler, _dataSourceAnalyserEngine, TimeSpan.Zero);
			activeAnalysis.Analysers.Should().HaveCount(2);

			_template.Analysers.Should().HaveCount(2, "because the template may not have been modified by the ctor");
		}

		[Test]
		public void TestAdd1()
		{
			var activeAnalysis = new ActiveAnalysis(AnalysisId.CreateNew(), _template, _taskScheduler, _dataSourceAnalyserEngine, TimeSpan.Zero);
			_template.Analysers.Should().BeEmpty();

			var configuration = new Mock<ILogAnalyserConfiguration>().Object;
			var analyser = activeAnalysis.Add(new AnalyserPluginId("foobar"), configuration);

			_template.Analysers.Should().HaveCount(1);
			var template = _template.Analysers.First();
			template.Id.Should().Be(analyser.Id);
			template.AnalyserPluginId.Should().Be(new AnalyserPluginId("foobar"));
			template.Configuration.Should().Be(configuration);
		}

		[Test]
		public void TestTryGetAnalyser()
		{
			var activeAnalysis = new ActiveAnalysis(AnalysisId.CreateNew(), _template, _taskScheduler, _dataSourceAnalyserEngine, TimeSpan.Zero);
			_template.Analysers.Should().BeEmpty();

			var configuration = new Mock<ILogAnalyserConfiguration>().Object;
			var analyser = activeAnalysis.Add(new AnalyserPluginId("foobar"), configuration);
			activeAnalysis.TryGetAnalyser(analyser.Id, out var actualAnalyser).Should().BeTrue();
			actualAnalyser.Should().BeSameAs(analyser);
		}

		[Test]
		public void TestTryGetNonExistentAnalyser()
		{
			var activeAnalysis = new ActiveAnalysis(AnalysisId.CreateNew(), _template, _taskScheduler, _dataSourceAnalyserEngine, TimeSpan.Zero);
			_template.Analysers.Should().BeEmpty();

			var configuration = new Mock<ILogAnalyserConfiguration>().Object;
			activeAnalysis.Add(new AnalyserPluginId("foobar"), configuration);
			activeAnalysis.TryGetAnalyser(AnalyserId.CreateNew(), out var actualAnalyser).Should().BeFalse();
			actualAnalyser.Should().BeNull();
		}

		[Test]
		public void TestAddRemove1()
		{
			var activeAnalysis = new ActiveAnalysis(AnalysisId.CreateNew(), _template, _taskScheduler, _dataSourceAnalyserEngine, TimeSpan.Zero);
			_template.Analysers.Should().BeEmpty();

			var analyser = activeAnalysis.Add(new AnalyserPluginId("foobar"), null);
			_template.Analysers.Should().HaveCount(1);

			activeAnalysis.Remove(analyser);
			_template.Analysers.Should().BeEmpty();
		}

		[Test]
		public void TestDispose()
		{
			var activeAnalysis = new ActiveAnalysis(AnalysisId.CreateNew(), _template, _taskScheduler, _dataSourceAnalyserEngine, TimeSpan.Zero);
			activeAnalysis.Dispose();

			_taskScheduler.PeriodicTaskCount.Should()
				.Be(0, "because all tasks should've been stopped when the group is disposed of");
		}
	}
}