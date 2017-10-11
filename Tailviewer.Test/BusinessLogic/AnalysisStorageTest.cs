using System.IO;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class AnalysisStorageTest
	{
		private ManualTaskScheduler _taskScheduler;
		private AnalysisStorage _storage;
		private InMemoryFilesystem _filesystem;
		private Mock<ILogAnalyserEngine> _logAnalyserEngine;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();
			_filesystem = new InMemoryFilesystem(new ImmediateTaskScheduler());

			var root = Path.GetPathRoot(Constants.AnalysisDirectory);
			_filesystem.AddRoot(root);

			_logAnalyserEngine = new Mock<ILogAnalyserEngine>();
			_storage = new AnalysisStorage(_taskScheduler,
				_filesystem,
				_logAnalyserEngine.Object);
		}

		[Test]
		public void TestCreateAnalysis1()
		{
			var analysis = _storage.CreateAnalysis(new AnalysisTemplate(), new AnalysisViewTemplate());
			analysis.Should().NotBeNull();
			analysis.Should().BeOfType<ActiveAnalysis>();
		}

		[Test]
		[Description("Verifies that creating a new analysis immediately writes it to disk")]
		public void TestCreateAnalysis2()
		{
			var analysis = _storage.CreateAnalysis(new AnalysisTemplate(), new AnalysisViewTemplate());

			var id = analysis.Id;
			var filename = Path.Combine(Constants.AnalysisDirectory, string.Format("{0}.{1}", id, Constants.AnalysisExtension));
			_filesystem.FileExists(filename).Result.Should().BeTrue("because the storage should've created a new file on disk");
		}

		[Test]
		public void TestRemove1()
		{
			_storage.AnalysisTemplates.Should().BeEmpty();
			var analysis = _storage.CreateAnalysis(new AnalysisTemplate(), new AnalysisViewTemplate());
			_storage.AnalysisTemplates.Should().HaveCount(1);
			IAnalysis unused;
			_storage.TryGetAnalysisFor(analysis.Id, out unused).Should().BeTrue();

			_storage.Remove(analysis.Id);
			_storage.AnalysisTemplates.Should().BeEmpty("because the only analysis should've been removed");
			_storage.TryGetAnalysisFor(analysis.Id, out unused).Should().BeFalse("because the only analysis should've been removed");
		}

		[Test]
		[Description("Verifies that the running analysis is disposed of when it's removed")]
		public void TestRemove2()
		{
			var analysis = _storage.CreateAnalysis(new AnalysisTemplate(), new AnalysisViewTemplate());
			((ActiveAnalysis)analysis).IsDisposed.Should().BeFalse();

			_storage.Remove(analysis.Id);
			((ActiveAnalysis)analysis).IsDisposed.Should().BeTrue("because the storage created the analysis so it should dispose of it as well");
		}
	}
}