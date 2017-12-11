using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFileColumnsTest
	{
		[Test]
		[Description("Verifies that the index property doesn't regress")]
		public void TestIndex()
		{
			LogFileColumns.Index.Id.Should().Be("index");
			LogFileColumns.Index.DataType.Should().Be<LogEntryIndex>();
		}

		[Test]
		[Description("Verifies that the original index property doesn't regress")]
		public void TestOriginalIndex()
		{
			LogFileColumns.OriginalIndex.Id.Should().Be("original_index");
			LogFileColumns.OriginalIndex.DataType.Should().Be<LogEntryIndex>();
		}

		[Test]
		[Description("Verifies that the LineNumber property doesn't regress")]
		public void TestLineNumber()
		{
			LogFileColumns.LineNumber.Id.Should().Be("line_number");
			LogFileColumns.LineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the OriginalLineNumber property doesn't regress")]
		public void TestOriginalLineNumber()
		{
			LogFileColumns.OriginalLineNumber.Id.Should().Be("original_line_number");
			LogFileColumns.OriginalLineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the Timestamp property doesn't regress")]
		public void TestTimestamp()
		{
			LogFileColumns.TimeStamp.Id.Should().Be("timestamp");
			LogFileColumns.TimeStamp.DataType.Should().Be<DateTime?>();
		}

		[Test]
		[Description("Verifies that the DeltaTime property doesn't regress")]
		public void TestDeltaTime()
		{
			LogFileColumns.DeltaTime.Id.Should().Be("delta_time");
			LogFileColumns.DeltaTime.DataType.Should().Be<TimeSpan?>();
		}

		[Test]
		[Description("Verifies that the ElapsedTime property doesn't regress")]
		public void TestElapsedTime()
		{
			LogFileColumns.ElapsedTime.Id.Should().Be("elapsed_time");
			LogFileColumns.ElapsedTime.DataType.Should().Be<TimeSpan?>();
		}

		[Test]
		[Description("Verifies that the RawContent property doesn't regress")]
		public void TestRawContent()
		{
			LogFileColumns.RawContent.Id.Should().Be("raw_content");
			LogFileColumns.RawContent.DataType.Should().Be<string>();
		}
	}
}