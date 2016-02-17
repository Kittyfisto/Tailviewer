using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class MergedLogFileAcceptanceTest
		: AbstractTest
	{
		[Test]
		[Ignore("Doesn't work anymore because the actual source file is not ordered by time strictly ascending - several lines are completely out of order")]
		[Description("Verifies that the MergedLogFile represents the very same content than its single source")]
		public void Test20Mb()
		{
			using (var source = new LogFile(LogFileTest.File20Mb))
			using (var merged = new MergedLogFile(source))
			{
				source.Start();
				source.Wait();

				merged.Start(TimeSpan.FromMilliseconds(1));
				merged.Wait();

				merged.Count.Should().Be(source.Count);
				merged.DebugCount.Should().Be(source.DebugCount);
				merged.InfoCount.Should().Be(source.InfoCount);
				merged.WarningCount.Should().Be(source.WarningCount);
				merged.ErrorCount.Should().Be(source.ErrorCount);
				merged.FatalCount.Should().Be(source.FatalCount);
				merged.FileSize.Should().Be(source.FileSize);
				merged.StartTimestamp.Should().Be(source.StartTimestamp);

				var sourceLines = source.GetSection(new LogFileSection(0, source.Count));
				var mergedLines = merged.GetSection(new LogFileSection(0, merged.Count));
				for (int i = 0; i < source.Count; ++i)
				{
					var mergedLine = mergedLines[i];
					var sourceLine = sourceLines[i];
					mergedLine.Should().Be(sourceLine);
				}
			}
		}

		[Test]
		public void Test2Sources()
		{
			using (var source1 = new LogFile(LogFileTest.File2Entries))
			using (var source2 = new LogFile(LogFileTest.File2Lines))
			using (var merged = new MergedLogFile(source1, source2))
			{
				merged.Start(TimeSpan.Zero);

				source1.Start();
				source1.Wait();

				source2.Start();
				source2.Wait();

				WaitUntil(() => merged.Count == 8, TimeSpan.FromSeconds(2))
					.Should().BeTrue("Because the merged file should've been finished after 2 seconds");
				merged.DebugCount.Should().Be(4);
				merged.InfoCount.Should().Be(4);
				merged.WarningCount.Should().Be(0);
				merged.ErrorCount.Should().Be(0);
				merged.FatalCount.Should().Be(0);
				merged.FileSize.Should().Be(source1.FileSize + source2.FileSize);
				merged.StartTimestamp.Should().Be(source1.StartTimestamp);

				var source1Lines = source1.GetSection(new LogFileSection(0, source1.Count));
				var source2Lines = source2.GetSection(new LogFileSection(0, source2.Count));
				var mergedLines = merged.GetSection(new LogFileSection(0, merged.Count));

				// The reason why this appears to be out of order is because Tailviewer ignores the
				// milliseconds of a timespan. This must be fixed..
				mergedLines[0].Should().Be(new LogLine(0, 0, source1Lines[0]));
				mergedLines[1].Should().Be(new LogLine(1, 0, source1Lines[1]));
				mergedLines[2].Should().Be(new LogLine(2, 0, source1Lines[2]));
				mergedLines[3].Should().Be(new LogLine(3, 1, source2Lines[0]));
				mergedLines[4].Should().Be(new LogLine(4, 2, source1Lines[3]));
				mergedLines[5].Should().Be(new LogLine(5, 2, source1Lines[4]));
				mergedLines[6].Should().Be(new LogLine(6, 2, source1Lines[5]));
				mergedLines[7].Should().Be(new LogLine(7, 3, source2Lines[1]));
			}
		}
	}
}