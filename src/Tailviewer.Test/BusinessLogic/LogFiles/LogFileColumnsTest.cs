using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
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
			Columns.Minimum.Should().Equal(new object[]
			{
				Columns.RawContent,
				Columns.Index,
				Columns.OriginalIndex,
				Columns.LogEntryIndex,
				Columns.LineNumber,
				Columns.OriginalLineNumber,
				Columns.OriginalDataSourceName,
				Columns.LogLevel,
				Columns.Timestamp,
				Columns.ElapsedTime,
				Columns.DeltaTime
			});

			Columns.Minimum.Should().NotContain(Columns.SourceId,
			                                           "because the source id column is specialized and only available in some cases");
		}

		[Test]
		[Description("Verifies that the Level property doesn't regress")]
		public void TestLevel()
		{
			Columns.LogLevel.Id.Should().Be("log_level");
			Columns.LogLevel.DataType.Should().Be<LevelFlags>();
		}

		[Test]
		[Description("Verifies that the index property doesn't regress")]
		public void TestIndex()
		{
			Columns.Index.Id.Should().Be("index");
			Columns.Index.DataType.Should().Be<LogLineIndex>();
			Columns.Index.DefaultValue.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the original index property doesn't regress")]
		public void TestOriginalIndex()
		{
			Columns.OriginalIndex.Id.Should().Be("original_index");
			Columns.OriginalIndex.DataType.Should().Be<LogLineIndex>();
			Columns.OriginalIndex.DefaultValue.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the log entry index property doesn't regress")]
		public void TestLogEntryIndex()
		{
			Columns.LogEntryIndex.Id.Should().Be("log_entry_index");
			Columns.LogEntryIndex.DataType.Should().Be<LogEntryIndex>();
			Columns.LogEntryIndex.DefaultValue.Should().Be(LogEntryIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the LineNumber property doesn't regress")]
		public void TestLineNumber()
		{
			Columns.LineNumber.Id.Should().Be("line_number");
			Columns.LineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the OriginalLineNumber property doesn't regress")]
		public void TestOriginalLineNumber()
		{
			Columns.OriginalLineNumber.Id.Should().Be("original_line_number");
			Columns.OriginalLineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the Timestamp property doesn't regress")]
		public void TestTimestamp()
		{
			Columns.Timestamp.Id.Should().Be("timestamp");
			Columns.Timestamp.DataType.Should().Be<DateTime?>();
		}

		[Test]
		[Description("Verifies that the DeltaTime property doesn't regress")]
		public void TestDeltaTime()
		{
			Columns.DeltaTime.Id.Should().Be("delta_time");
			Columns.DeltaTime.DataType.Should().Be<TimeSpan?>();
		}

		[Test]
		[Description("Verifies that the Message property doesn't regress")]
		public void TestMessage()
		{
			Columns.Message.Id.Should().Be("message");
			Columns.Message.DataType.Should().Be<string>();
		}

		[Test]
		[Description("Verifies that the ElapsedTime property doesn't regress")]
		public void TestElapsedTime()
		{
			Columns.ElapsedTime.Id.Should().Be("elapsed_time");
			Columns.ElapsedTime.DataType.Should().Be<TimeSpan?>();
		}

		[Test]
		[Description("Verifies that the RawContent property doesn't regress")]
		public void TestRawContent()
		{
			Columns.RawContent.Id.Should().Be("raw_content");
			Columns.RawContent.DataType.Should().Be<string>();
		}

		[Test]
		[Description("Verifies that the RawContentMaxPresentationWidth property doesn't regress")]
		public void TestRawContentMaxPresentationWidth()
		{
			Columns.RawContentMaxPresentationWidth.Id.Should().Be("raw_content_max_presentation_width");
			Columns.RawContentMaxPresentationWidth.DataType.Should().Be<float>();
		}

		[Test]
		[Description("Verifies that the PresentationStartingLineNumber property doesn't regress")]
		public void TestPresentationStartingLineNumber()
		{
			Columns.PresentationStartingLineNumber.Id.Should().Be("presentation_line_number");
			Columns.PresentationStartingLineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the PresentationLineCount property doesn't regress")]
		public void TestPresentationLineCount()
		{
			Columns.PresentationLineCount.Id.Should().Be("presentation_line_count");
			Columns.PresentationLineCount.DataType.Should().Be<int>();
		}

		[Test]
		public void TestCombine1()
		{
			Columns.Combine(new IColumnDescriptor[0], Columns.OriginalLineNumber)
			              .Should().Equal(Columns.OriginalLineNumber);
		}

		[Test]
		public void TestCombine2()
		{
			Columns.Combine(new IColumnDescriptor[]
			              {
				              Columns.PresentationLineCount
			              }, Columns.OriginalLineNumber)
			              .Should().Equal(Columns.PresentationLineCount, Columns.OriginalLineNumber);
		}

		[Test]
		[Description("Verifies that Combine doesn't introduce the same column twice")]
		public void TestCombine3()
		{
			Columns.Combine(new IColumnDescriptor[]
			              {
				              Columns.RawContent,
							  Columns.RawContentMaxPresentationWidth
			              }, Columns.RawContentMaxPresentationWidth)
			              .Should().Equal(Columns.RawContent, Columns.RawContentMaxPresentationWidth);
		}
	}
}