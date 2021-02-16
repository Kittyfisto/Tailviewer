using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Core.Columns;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFileColumnsTest
	{
		[Test]
		[Description("Verifies that the Minimum property doesn't regress")]
		public void TestWellKnownColumns()
		{
			LogColumns.Minimum.Should().Equal(new object[]
			{
				LogColumns.RawContent,
				LogColumns.Index,
				LogColumns.OriginalIndex,
				LogColumns.LogEntryIndex,
				LogColumns.LineNumber,
				LogColumns.OriginalLineNumber,
				LogColumns.OriginalDataSourceName,
				LogColumns.LogLevel,
				LogColumns.Timestamp,
				LogColumns.ElapsedTime,
				LogColumns.DeltaTime
			});

			LogColumns.Minimum.Should().NotContain(LogColumns.SourceId,
			                                           "because the source id column is specialized and only available in some cases");
		}

		[Test]
		[Description("Verifies that the Level property doesn't regress")]
		public void TestLevel()
		{
			LogColumns.LogLevel.Id.Should().Be("log_level");
			LogColumns.LogLevel.DataType.Should().Be<LevelFlags>();
		}

		[Test]
		[Description("Verifies that the index property doesn't regress")]
		public void TestIndex()
		{
			LogColumns.Index.Id.Should().Be("index");
			LogColumns.Index.DataType.Should().Be<LogLineIndex>();
			LogColumns.Index.DefaultValue.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the original index property doesn't regress")]
		public void TestOriginalIndex()
		{
			LogColumns.OriginalIndex.Id.Should().Be("original_index");
			LogColumns.OriginalIndex.DataType.Should().Be<LogLineIndex>();
			LogColumns.OriginalIndex.DefaultValue.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the log entry index property doesn't regress")]
		public void TestLogEntryIndex()
		{
			LogColumns.LogEntryIndex.Id.Should().Be("log_entry_index");
			LogColumns.LogEntryIndex.DataType.Should().Be<LogEntryIndex>();
			LogColumns.LogEntryIndex.DefaultValue.Should().Be(LogEntryIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the LineNumber property doesn't regress")]
		public void TestLineNumber()
		{
			LogColumns.LineNumber.Id.Should().Be("line_number");
			LogColumns.LineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the OriginalLineNumber property doesn't regress")]
		public void TestOriginalLineNumber()
		{
			LogColumns.OriginalLineNumber.Id.Should().Be("original_line_number");
			LogColumns.OriginalLineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the Timestamp property doesn't regress")]
		public void TestTimestamp()
		{
			LogColumns.Timestamp.Id.Should().Be("timestamp");
			LogColumns.Timestamp.DataType.Should().Be<DateTime?>();
		}

		[Test]
		[Description("Verifies that the DeltaTime property doesn't regress")]
		public void TestDeltaTime()
		{
			LogColumns.DeltaTime.Id.Should().Be("delta_time");
			LogColumns.DeltaTime.DataType.Should().Be<TimeSpan?>();
		}

		[Test]
		[Description("Verifies that the Message property doesn't regress")]
		public void TestMessage()
		{
			LogColumns.Message.Id.Should().Be("message");
			LogColumns.Message.DataType.Should().Be<string>();
		}

		[Test]
		[Description("Verifies that the ElapsedTime property doesn't regress")]
		public void TestElapsedTime()
		{
			LogColumns.ElapsedTime.Id.Should().Be("elapsed_time");
			LogColumns.ElapsedTime.DataType.Should().Be<TimeSpan?>();
		}

		[Test]
		[Description("Verifies that the RawContent property doesn't regress")]
		public void TestRawContent()
		{
			LogColumns.RawContent.Id.Should().Be("raw_content");
			LogColumns.RawContent.DataType.Should().Be<string>();
		}

		[Test]
		[Description("Verifies that the RawContentMaxPresentationWidth property doesn't regress")]
		public void TestRawContentMaxPresentationWidth()
		{
			LogColumns.RawContentMaxPresentationWidth.Id.Should().Be("raw_content_max_presentation_width");
			LogColumns.RawContentMaxPresentationWidth.DataType.Should().Be<float>();
		}

		[Test]
		[Description("Verifies that the PresentationStartingLineNumber property doesn't regress")]
		public void TestPresentationStartingLineNumber()
		{
			LogColumns.PresentationStartingLineNumber.Id.Should().Be("presentation_line_number");
			LogColumns.PresentationStartingLineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the PresentationLineCount property doesn't regress")]
		public void TestPresentationLineCount()
		{
			LogColumns.PresentationLineCount.Id.Should().Be("presentation_line_count");
			LogColumns.PresentationLineCount.DataType.Should().Be<int>();
		}

		[Test]
		public void TestCombine1()
		{
			LogColumns.Combine(new IColumnDescriptor[0], LogColumns.OriginalLineNumber)
			              .Should().Equal(LogColumns.OriginalLineNumber);
		}

		[Test]
		public void TestCombine2()
		{
			LogColumns.Combine(new IColumnDescriptor[]
			              {
				              LogColumns.PresentationLineCount
			              }, LogColumns.OriginalLineNumber)
			              .Should().Equal(LogColumns.PresentationLineCount, LogColumns.OriginalLineNumber);
		}

		[Test]
		[Description("Verifies that Combine doesn't introduce the same column twice")]
		public void TestCombine3()
		{
			LogColumns.Combine(new IColumnDescriptor[]
			              {
				              LogColumns.RawContent,
							  LogColumns.RawContentMaxPresentationWidth
			              }, LogColumns.RawContentMaxPresentationWidth)
			              .Should().Equal(LogColumns.RawContent, LogColumns.RawContentMaxPresentationWidth);
		}
	}
}