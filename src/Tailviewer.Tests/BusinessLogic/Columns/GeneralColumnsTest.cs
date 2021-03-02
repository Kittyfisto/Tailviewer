using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core.Columns;

namespace Tailviewer.Tests.BusinessLogic.Columns
{
	[TestFixture]
	public sealed class GeneralColumnsTest
	{
		[Test]
		[Description("Verifies that the Minimum property doesn't regress")]
		public void TestWellKnownColumns()
		{
			GeneralColumns.Minimum.Should().Equal(new object[]
			{
				GeneralColumns.RawContent,
				GeneralColumns.Index,
				GeneralColumns.OriginalIndex,
				GeneralColumns.LogEntryIndex,
				GeneralColumns.LineNumber,
				GeneralColumns.OriginalLineNumber,
				GeneralColumns.OriginalDataSourceName,
				GeneralColumns.LogLevel,
				GeneralColumns.Timestamp,
				GeneralColumns.ElapsedTime,
				GeneralColumns.DeltaTime
			});

			GeneralColumns.Minimum.Should().NotContain(GeneralColumns.SourceId,
			                                           "because the source id column is specialized and only available in some cases");
		}

		[Test]
		[Description("Verifies that the Level property doesn't regress")]
		public void TestLevel()
		{
			GeneralColumns.LogLevel.Id.Should().Be("log_level");
			GeneralColumns.LogLevel.DataType.Should().Be<LevelFlags>();
		}

		[Test]
		[Description("Verifies that the index property doesn't regress")]
		public void TestIndex()
		{
			GeneralColumns.Index.Id.Should().Be("index");
			GeneralColumns.Index.DataType.Should().Be<LogLineIndex>();
			GeneralColumns.Index.DefaultValue.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the original index property doesn't regress")]
		public void TestOriginalIndex()
		{
			GeneralColumns.OriginalIndex.Id.Should().Be("original_index");
			GeneralColumns.OriginalIndex.DataType.Should().Be<LogLineIndex>();
			GeneralColumns.OriginalIndex.DefaultValue.Should().Be(LogLineIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the log entry index property doesn't regress")]
		public void TestLogEntryIndex()
		{
			GeneralColumns.LogEntryIndex.Id.Should().Be("log_entry_index");
			GeneralColumns.LogEntryIndex.DataType.Should().Be<LogEntryIndex>();
			GeneralColumns.LogEntryIndex.DefaultValue.Should().Be(LogEntryIndex.Invalid);
		}

		[Test]
		[Description("Verifies that the LineNumber property doesn't regress")]
		public void TestLineNumber()
		{
			GeneralColumns.LineNumber.Id.Should().Be("line_number");
			GeneralColumns.LineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the OriginalLineNumber property doesn't regress")]
		public void TestOriginalLineNumber()
		{
			GeneralColumns.OriginalLineNumber.Id.Should().Be("original_line_number");
			GeneralColumns.OriginalLineNumber.DataType.Should().Be<int>();
		}

		[Test]
		[Description("Verifies that the Timestamp property doesn't regress")]
		public void TestTimestamp()
		{
			GeneralColumns.Timestamp.Id.Should().Be("timestamp");
			GeneralColumns.Timestamp.DataType.Should().Be<DateTime?>();
		}

		[Test]
		[Description("Verifies that the DeltaTime property doesn't regress")]
		public void TestDeltaTime()
		{
			GeneralColumns.DeltaTime.Id.Should().Be("delta_time");
			GeneralColumns.DeltaTime.DataType.Should().Be<TimeSpan?>();
		}

		[Test]
		[Description("Verifies that the Message property doesn't regress")]
		public void TestMessage()
		{
			GeneralColumns.Message.Id.Should().Be("message");
			GeneralColumns.Message.DataType.Should().Be<string>();
		}

		[Test]
		[Description("Verifies that the ElapsedTime property doesn't regress")]
		public void TestElapsedTime()
		{
			GeneralColumns.ElapsedTime.Id.Should().Be("elapsed_time");
			GeneralColumns.ElapsedTime.DataType.Should().Be<TimeSpan?>();
		}

		[Test]
		[Description("Verifies that the RawContent property doesn't regress")]
		public void TestRawContent()
		{
			GeneralColumns.RawContent.Id.Should().Be("raw_content");
			GeneralColumns.RawContent.DataType.Should().Be<string>();
		}

		[Test]
		public void TestCombine1()
		{
			GeneralColumns.Combine(new IColumnDescriptor[0], GeneralColumns.OriginalLineNumber)
			              .Should().Equal(GeneralColumns.OriginalLineNumber);
		}

		[Test]
		public void TestCombine2()
		{
			GeneralColumns.Combine(new IColumnDescriptor[]
			              {
				              GeneralColumns.LineNumber
			              }, GeneralColumns.OriginalLineNumber)
			              .Should().Equal(GeneralColumns.LineNumber, GeneralColumns.OriginalLineNumber);
		}

		[Test]
		[Description("Verifies that Combine doesn't introduce the same column twice")]
		public void TestCombine3()
		{
			GeneralColumns.Combine(new IColumnDescriptor[]
			              {
				              GeneralColumns.RawContent,
							  GeneralColumns.LineNumber
			              }, GeneralColumns.LineNumber)
			              .Should().Equal(GeneralColumns.RawContent, GeneralColumns.LineNumber);
		}
	}
}