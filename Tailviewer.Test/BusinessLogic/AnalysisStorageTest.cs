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
			_filesystem = new InMemoryFilesystem();
			_logAnalyserEngine = new Mock<ILogAnalyserEngine>();
			_storage = new AnalysisStorage(_taskScheduler,
				_filesystem,
				_logAnalyserEngine.Object);
		}

		[Test]
		public void TestCreateAnalysis()
		{
			var analysis = _storage.CreateAnalysis(new AnalysisTemplate(), new AnalysisViewTemplate());
			analysis.Should().NotBeNull();
			analysis.Should().BeOfType<ActiveAnalysis>();
		}
	}
}