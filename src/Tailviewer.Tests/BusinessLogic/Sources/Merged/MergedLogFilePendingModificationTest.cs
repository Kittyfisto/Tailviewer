using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core.Sources.Merged;

namespace Tailviewer.Tests.BusinessLogic.Sources.Merged
{
	[TestFixture]
	public sealed class MergedLogFilePendingModificationTest
	{
		[Test]
		public void TestOptimizeEmpty()
		{
			var input = new MergedLogSourcePendingModification[0];
			MergedLogSourcePendingModification.Optimize(input).Should().BeEmpty();
		}

		[Test]
		public void TestOptimizeOneSourceAppendReset()
		{
			var source = new Mock<ILogSource>();
			var input = new[]
			{
				new MergedLogSourcePendingModification(source.Object, LogSourceModification.Appended(1, 2)),
				new MergedLogSourcePendingModification(source.Object, LogSourceModification.Reset())
			};
			MergedLogSourcePendingModification.Optimize(input).Should().Equal(new object[]
			{
				new MergedLogSourcePendingModification(source.Object, LogSourceModification.Reset())
			});
		}

		[Test]
		public void TestOptimizeOneSourceAppendResetTwice()
		{
			var source = new Mock<ILogSource>();
			var input = new[]
			{
				new MergedLogSourcePendingModification(source.Object, LogSourceModification.Appended(1, 2)),
				new MergedLogSourcePendingModification(source.Object, LogSourceModification.Reset()),
				new MergedLogSourcePendingModification(source.Object, LogSourceModification.Appended(0, 100)),
				new MergedLogSourcePendingModification(source.Object, LogSourceModification.Reset())
			};
			MergedLogSourcePendingModification.Optimize(input).Should().Equal(new object[]
			{
				new MergedLogSourcePendingModification(source.Object, LogSourceModification.Reset())
			});
		}

		[Test]
		public void TestOptimizeTwoSourcesAppendReset()
		{
			var source1 = new Mock<ILogSource>();
			var source2 = new Mock<ILogSource>();
			var input = new[]
			{
				new MergedLogSourcePendingModification(source1.Object, LogSourceModification.Appended(1, 2)),
				new MergedLogSourcePendingModification(source2.Object, LogSourceModification.Reset())
			};
			MergedLogSourcePendingModification.Optimize(input).Should().Equal(new object[]
			{
				new MergedLogSourcePendingModification(source1.Object, LogSourceModification.Appended(1, 2)),
				new MergedLogSourcePendingModification(source2.Object, LogSourceModification.Reset())
			});
		}

		[Test]
		public void TestOptimizeTwoSourcesAppendReset2()
		{
			var source1 = new Mock<ILogSource>();
			var source2 = new Mock<ILogSource>();
			var input = new[]
			{
				new MergedLogSourcePendingModification(source1.Object, LogSourceModification.Appended(1, 2)),
				new MergedLogSourcePendingModification(source2.Object, LogSourceModification.Appended(0, 42)),
				new MergedLogSourcePendingModification(source2.Object, LogSourceModification.Reset())
			};
			MergedLogSourcePendingModification.Optimize(input).Should().Equal(new object[]
			{
				new MergedLogSourcePendingModification(source1.Object, LogSourceModification.Appended(1, 2)),
				new MergedLogSourcePendingModification(source2.Object, LogSourceModification.Reset())
			});
		}
	}
}
