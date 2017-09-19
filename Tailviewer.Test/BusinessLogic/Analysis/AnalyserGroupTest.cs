using System;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Test.BusinessLogic.Analysis
{
	[TestFixture]
	public sealed class AnalyserGroupTest
	{
		private ManualTaskScheduler _taskScheduler;
		private Mock<IAnalysisEngine> _analysisEngine;

		[SetUp]
		public void Setup()
		{
			_taskScheduler = new ManualTaskScheduler();
			_analysisEngine = new Mock<IAnalysisEngine>();
		}

		[Test]
		public void TestDispose()
		{
			var group = new AnalyserGroup(_taskScheduler, _analysisEngine.Object, TimeSpan.Zero);
			group.Dispose();

			_taskScheduler.PeriodicTaskCount.Should()
				.Be(0, "because all tasks should've been stopped when the group is disposed of");
		}
	}
}