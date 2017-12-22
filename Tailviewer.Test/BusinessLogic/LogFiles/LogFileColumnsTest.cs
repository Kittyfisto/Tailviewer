using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFileColumnsTest
	{
		[Test]
		[Description("Verifies that the Minimum property doesn't regress")]
		public void TestWellKnownColumns()
		{
			LogFileColumns.Minimum.Should().Equal(new object[]
			{
				LogFileColumns.RawContent,
				LogFileColumns.Index,
				LogFileColumns.OriginalIndex,
				LogFileColumns.LogEntryIndex,
				LogFileColumns.LineNumber,
				LogFileColumns.OriginalLineNumber,
				LogFileColumns.LogLevel,
				LogFileColumns.Timestamp,
				LogFileColumns.ElapsedTime,
				LogFileColumns.DeltaTime
			});
		}

		[Test]
		[Description("Verifies that the Level property doesn't regress")]
		public void TestLevel()
		{
			LogFileColumns.LogLevel.Id.Should().Be("log_level");
			LogFileColumns.LogLevel.DataType.Should().Be<LevelFlags>();
		}

		[Test]
		[Description("Verifies that the index property doesn't regress")]
		public void TestIndex()
		{
			LogFileColumns.Index.Id.Should().Be("index");
			LogFileColumns.Index.DataType.Should().Be<LogLineIndex>();
			LogFileColumns.Index.DefaultValue.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the original index property doesn't regress")]
		public void TestOriginalIndex()
		{
			LogFileColumns.OriginalIndex.Id.Should().Be("original_index");
			LogFileColumns.OriginalIndex.DataType.Should().Be<LogLineIndex>();
			LogFileColumns.OriginalIndex.DefaultValue.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the log entry index property doesn't regress")]
		public void TestLogEntryIndex()
		{
			LogFileColumns.LogEntryIndex.Id.Should().Be("log_entry_index");
			LogFileColumns.LogEntryIndex.DataType.Should().Be<LogEntryIndex>();
			LogFileColumns.LogEntryIndex.DefaultValue.Should().Be(LogEntryIndex.Invalid);
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
			LogFileColumns.Timestamp.Id.Should().Be("timestamp");
			LogFileColumns.Timestamp.DataType.Should().Be<DateTime?>();
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