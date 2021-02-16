using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Sources.Merged;

namespace Tailviewer.Test.BusinessLogic.LogFiles.Merged
{
	[TestFixture]
	public sealed class MergedLogFilePendingModificationTest
	{
		[Test]
		public void TestOptimizeEmpty()
		{
			var input = new MergedLogFilePendingModification[0];
			MergedLogFilePendingModification.Optimize(input).Should().BeEmpty();
		}

		[Test]
		public void TestOptimizeOneSourceAppendReset()
		{
			var source = new Mock<ILogSource>();
			var input = new[]
			{
				new MergedLogFilePendingModification(source.Object, new LogFileSection(1, 2)),
				new MergedLogFilePendingModification(source.Object, LogFileSection.Reset)
			};
			MergedLogFilePendingModification.Optimize(input).Should().Equal(new object[]
			{
				new MergedLogFilePendingModification(source.Object, LogFileSection.Reset)
			});
		}

		[Test]
		public void TestOptimizeOneSourceAppendResetTwice()
		{
			var source = new Mock<ILogSource>();
			var input = new[]
			{
				new MergedLogFilePendingModification(source.Object, new LogFileSection(1, 2)),
				new MergedLogFilePendingModification(source.Object, LogFileSection.Reset),
				new MergedLogFilePendingModification(source.Object, new LogFileSection(0, 100)),
				new MergedLogFilePendingModification(source.Object, LogFileSection.Reset)
			};
			MergedLogFilePendingModification.Optimize(input).Should().Equal(new object[]
			{
				new MergedLogFilePendingModification(source.Object, LogFileSection.Reset)
			});
		}

		[Test]
		public void TestOptimizeTwoSourcesAppendReset()
		{
			var source1 = new Mock<ILogSource>();
			var source2 = new Mock<ILogSource>();
			var input = new[]
			{
				new MergedLogFilePendingModification(source1.Object, new LogFileSection(1, 2)),
				new MergedLogFilePendingModification(source2.Object, LogFileSection.Reset)
			};
			MergedLogFilePendingModification.Optimize(input).Should().Equal(new object[]
			{
				new MergedLogFilePendingModification(source1.Object, new LogFileSection(1, 2)),
				new MergedLogFilePendingModification(source2.Object, LogFileSection.Reset)
			});
		}

		[Test]
		public void TestOptimizeTwoSourcesAppendReset2()
		{
			var source1 = new Mock<ILogSource>();
			var source2 = new Mock<ILogSource>();
			var input = new[]
			{
				new MergedLogFilePendingModification(source1.Object, new LogFileSection(1, 2)),
				new MergedLogFilePendingModification(source2.Object, new LogFileSection(0, 42)),
				new MergedLogFilePendingModification(source2.Object, LogFileSection.Reset)
			};
			MergedLogFilePendingModification.Optimize(input).Should().Equal(new object[]
			{
				new MergedLogFilePendingModification(source1.Object, new LogFileSection(1, 2)),
				new MergedLogFilePendingModification(source2.Object, LogFileSection.Reset)
			});
		}
	}
}
