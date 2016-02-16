using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;

namespace Tailviewer.Test.BusinessLogic
{
	[TestFixture]
	public sealed class MergedLogFileAcceptanceTest
	{
		public const string File20Mb = @"TestData\20Mb.txt";

		[Test]
		[Description("Verifies that the MergedLogFile represents the very same content than its single source")]
		public void Test20Mb()
		{
			using (var source = new LogFile(File20Mb))
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
				mergedLines.Should().Equal(sourceLines);
			}
		}
	}
}