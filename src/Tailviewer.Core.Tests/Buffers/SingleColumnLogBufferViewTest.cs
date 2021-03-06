﻿using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Buffers
{
	[TestFixture]
	public sealed class SingleColumnLogBufferViewTest
	{
		[Test]
		public void TestConstruction_LineNumber([Values(0, 10)] int count)
		{
			var buffer = new int[count];
			var view = new SingleColumnLogBufferView<int>(Core.Columns.LineNumber, buffer, 0, count);
			view.Count.Should().Be(count);
			view.Columns.Should().Equal(new object[] {Core.Columns.LineNumber});
			view.Contains(Core.Columns.LineNumber).Should().BeTrue();
			view.Contains(Core.Columns.OriginalLineNumber).Should().BeFalse();
			view.Contains(Core.Columns.Message).Should().BeFalse();
		}

		[Test]
		public void TestConstruction_RawContent([Values(2, 100)] int count)
		{
			var buffer = new string[count];
			var view = new SingleColumnLogBufferView<string>(Core.Columns.RawContent, buffer, 0, count);
			view.Count.Should().Be(count);
			view.Columns.Should().Equal(new object[] {Core.Columns.RawContent});
			view.Contains(Core.Columns.Message).Should().BeFalse();
			view.Contains(Core.Columns.RawContent).Should().BeTrue();
			view.Contains(Core.Columns.LineNumber).Should().BeFalse();
			view.Contains(Core.Columns.OriginalLineNumber).Should().BeFalse();
		}

		[Test]
		public void TestGetSetValue()
		{
			var buffer = new[]
			{
				"Good",
				"Morning",
				"Munich"
			};
			var view = new SingleColumnLogBufferView<string>(Core.Columns.RawContent, buffer, 1, 2);
			view.Count.Should().Be(2);
			view.Columns.Should().Equal(new object[] {Core.Columns.RawContent});

			view[0].RawContent.Should().Be("Morning");
			view[1].RawContent.Should().Be("Munich");

			view[1].RawContent = "What's up!?";
			buffer[2].Should().Be("What's up!?");
		}

		[Test]
		public void TestConstruction_Offset_TooSmall()
		{
			var offset = -1;
			var count = 1;
			var buffer = new int[offset + count];

			new Action(() => new SingleColumnLogBufferView<int>(Core.Columns.LineNumber, buffer, offset, count))
				.Should()
				.Throw<ArgumentOutOfRangeException>();
		}

		[Test]
		public void TestConstruction_BufferTooSmall()
		{
			var offset = 10;
			var count = 2;
			var buffer = new int[offset + count - 1];

			new Action(() => new SingleColumnLogBufferView<int>(Core.Columns.LineNumber, buffer, offset, count))
				.Should().Throw<ArgumentException>();
		}

		[Test]
		public void TestConstruction_BufferNull()
		{
			new Action(() => new SingleColumnLogBufferView<int>(Core.Columns.LineNumber, null, 0, 0))
				.Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestCopyFrom_ContiguousArray([Values(0, 1, 2)] int offset)
		{
			var count = 3;
			var surplus = 5;
			var originalBuffer = new int[offset + count + surplus];
			var buffer = new int[offset+count+surplus];

			for (int i = 0; i < offset + count + surplus; ++i)
			{
				originalBuffer[i] = buffer[i] = i + 1;
			}

			var view = new SingleColumnLogBufferView<int>(Core.Columns.LineNumber, buffer, offset, count);
			var desiredData = new[] {2, 5, 42};
			view.CopyFrom(Core.Columns.LineNumber, 0, desiredData, 0, 3);

			for (int i = 0; i < offset; ++i)
				buffer[i].Should().Be(originalBuffer[i]);

			buffer[offset + 0].Should().Be(desiredData[0]);
			buffer[offset + 1].Should().Be(desiredData[1]);
			buffer[offset + 2].Should().Be(desiredData[2]);

			for (int i = offset+count; i < offset+count+surplus; ++i)
				buffer[i].Should().Be(originalBuffer[i]);
		}

		[Test]
		public void TestCopyFrom_ContiguousArray_NoSuchColumn()
		{
			var offset = 42;
			var count = 3;
			var surplus = 5;
			var originalBuffer = new int[offset + count + surplus];
			var buffer = new int[offset+count+surplus];

			for (int i = 0; i < offset + count + surplus; ++i)
			{
				originalBuffer[i] = buffer[i] = i + 1;
			}

			var view = new SingleColumnLogBufferView<int>(Core.Columns.LineNumber, buffer, offset, count);
			var desiredData = new[] {2, 5, 42};
			new Action(() => view.CopyFrom(Core.Columns.OriginalLineNumber, 0, desiredData, 0, 3)).Should()
				.Throw<NoSuchColumnException>();

			for (int i = 0; i < offset + count + surplus; ++i)
				buffer[i].Should().Be(originalBuffer[i], "because the original data may not have been overwritten");
		}

		[Test]
		public void TestCopyFrom_Buffer([Values(0, 1, 2)] int offset)
		{
			var count = 3;
			var surplus = 5;
			var originalBuffer = new int[offset + count + surplus];
			var buffer = new int[offset+count+surplus];

			for (int i = 0; i < offset + count + surplus; ++i)
			{
				originalBuffer[i] = buffer[i] = i + 1;
			}

			var view = new SingleColumnLogBufferView<int>(Core.Columns.LineNumber, buffer, offset, count);
			var source = new LogBufferList(Core.Columns.OriginalLineNumber, Core.Columns.LineNumber);
			source.Add(new LogEntry{OriginalLineNumber = 10, LineNumber = 42});
			source.Add(new LogEntry{OriginalLineNumber = 12, LineNumber = 9001});

			view.CopyFrom(Core.Columns.LineNumber, 1, source, new[] {1, 0});

			for (int i = 0; i < offset; ++i)
				buffer[i].Should().Be(originalBuffer[i]);

			buffer[offset + 0].Should().Be(originalBuffer[offset + 0], "because we wanted to copy the data at index 1 of the view and thus the data at index 0 should be left unharmed");
			buffer[offset + 1].Should().Be(source[1].LineNumber, "because we wanted to reverse the order in which the data was copied from the source");
			buffer[offset + 2].Should().Be(source[0].LineNumber, "because we wanted to reverse the order in which the data was copied from the source");

			for (int i = offset+count; i < offset+count+surplus; ++i)
				buffer[i].Should().Be(originalBuffer[i]);
		}

		[Test]
		public void TestCopyFrom_Buffer_NoSuchColumn()
		{
			var offset = 5;
			var count = 3;
			var surplus = 5;
			var originalBuffer = new int[offset + count + surplus];
			var buffer = new int[offset+count+surplus];

			for (int i = 0; i < offset + count + surplus; ++i)
			{
				originalBuffer[i] = buffer[i] = i + 1;
			}

			var view = new SingleColumnLogBufferView<int>(Core.Columns.LineNumber, buffer, offset, count);
			var source = new LogBufferList(Core.Columns.OriginalLineNumber, Core.Columns.LineNumber);
			source.Add(new LogEntry{OriginalLineNumber = 10, LineNumber = 42});
			source.Add(new LogEntry{OriginalLineNumber = 12, LineNumber = 9001});

			new Action(() => view.CopyFrom(Core.Columns.OriginalLineNumber, 1, source, new[] {1, 0}))
				.Should().Throw<NoSuchColumnException>();

			for (int i = 0; i < offset + count + surplus; ++i)
				buffer[i].Should().Be(originalBuffer[i], "because the buffer may not have been overwritten");
		}

		[Test]
		public void TestCopyFrom_Buffer_NullColumn()
		{
			var offset = 5;
			var count = 3;
			var surplus = 5;
			var originalBuffer = new int[offset + count + surplus];
			var buffer = new int[offset+count+surplus];

			for (int i = 0; i < offset + count + surplus; ++i)
			{
				originalBuffer[i] = buffer[i] = i + 1;
			}

			var view = new SingleColumnLogBufferView<int>(Core.Columns.LineNumber, buffer, offset, count);
			var source = new LogBufferList(Core.Columns.OriginalLineNumber, Core.Columns.LineNumber);
			source.Add(new LogEntry{OriginalLineNumber = 10, LineNumber = 42});
			source.Add(new LogEntry{OriginalLineNumber = 12, LineNumber = 9001});

			new Action(() => view.CopyFrom(null, 1, source, new[] {1, 0}))
				.Should().Throw<ArgumentNullException>();

			for (int i = 0; i < offset + count + surplus; ++i)
				buffer[i].Should().Be(originalBuffer[i], "because the buffer may not have been overwritten");
		}

		[Test]
		public void TestFillDefault()
		{
			var offset = 5;
			var count = 3;
			var surplus = 5;
			var originalBuffer = new LogLineIndex[offset + count + surplus];
			var buffer = new LogLineIndex[offset+count+surplus];

			for (int i = 0; i < offset + count + surplus; ++i)
			{
				originalBuffer[i] = buffer[i] = i + 1;
			}

			var view = new SingleColumnLogBufferView<LogLineIndex>(Core.Columns.Index, buffer, offset, count);
			var fillOffset = 1;
			view.FillDefault(fillOffset, count - fillOffset);

			for (int i = 0; i < offset + fillOffset; ++i)
			{
				buffer[i].Should().Be(originalBuffer[i]);
			}

			for (int i = offset + fillOffset; i < offset + count; ++i)
			{
				buffer[i].Should().Be(Core.Columns.Index.DefaultValue);
			}

			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(originalBuffer[i]);
			}
		}

		[Test]
		public void TestFillDefault_SingleColumn()
		{
			var offset = 5;
			var count = 3;
			var surplus = 5;
			var originalBuffer = new LogLineIndex[offset + count + surplus];
			var buffer = new LogLineIndex[offset+count+surplus];

			for (int i = 0; i < offset + count + surplus; ++i)
			{
				originalBuffer[i] = buffer[i] = i + 1;
			}

			var view = new SingleColumnLogBufferView<LogLineIndex>(Core.Columns.Index, buffer, offset, count);
			var fillOffset = 1;
			view.FillDefault(Core.Columns.Index, fillOffset, count - fillOffset);

			for (int i = 0; i < offset + fillOffset; ++i)
			{
				buffer[i].Should().Be(originalBuffer[i]);
			}

			for (int i = offset + fillOffset; i < offset + count; ++i)
			{
				buffer[i].Should().Be(Core.Columns.Index.DefaultValue);
			}

			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(originalBuffer[i]);
			}
		}

		[Test]
		public void TestFillDefault_NoSuchColumn()
		{
			var offset = 5;
			var count = 3;
			var surplus = 5;
			var originalBuffer = new LogLineIndex[offset + count + surplus];
			var buffer = new LogLineIndex[offset+count+surplus];

			for (int i = 0; i < offset + count + surplus; ++i)
			{
				originalBuffer[i] = buffer[i] = i + 1;
			}

			var view = new SingleColumnLogBufferView<LogLineIndex>(Core.Columns.Index, buffer, offset, count);
			var fillOffset = 1;
			new Action(() => view.FillDefault(Core.Columns.OriginalIndex, fillOffset, count - fillOffset))
				.Should().Throw<NoSuchColumnException>();

			for (int i = 0; i <  offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(originalBuffer[i]);
			}
		}

		[Test]
		public void TestFill_SingleColumn()
		{
			var offset = 5;
			var count = 3;
			var surplus = 5;
			var originalBuffer = new LogLineIndex[offset + count + surplus];
			var buffer = new LogLineIndex[offset+count+surplus];

			for (int i = 0; i < offset + count + surplus; ++i)
			{
				originalBuffer[i] = buffer[i] = i + 1;
			}

			var view = new SingleColumnLogBufferView<LogLineIndex>(Core.Columns.Index, buffer, offset, count);
			var fillOffset = 1;
			view.Fill(Core.Columns.Index, 42,fillOffset, count - fillOffset);

			for (int i = 0; i < offset + fillOffset; ++i)
			{
				buffer[i].Should().Be(originalBuffer[i]);
			}

			for (int i = offset + fillOffset; i < offset + count; ++i)
			{
				buffer[i].Should().Be(42);
			}

			for (int i = offset + count; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(originalBuffer[i]);
			}
		}

		[Test]
		public void TestFill_NoSuchColumn()
		{
			var offset = 5;
			var count = 3;
			var surplus = 5;
			var originalBuffer = new LogLineIndex[offset + count + surplus];
			var buffer = new LogLineIndex[offset+count+surplus];

			for (int i = 0; i < offset + count + surplus; ++i)
			{
				originalBuffer[i] = buffer[i] = i + 1;
			}

			var view = new SingleColumnLogBufferView<LogLineIndex>(Core.Columns.Index, buffer, offset, count);
			var fillOffset = 1;
			new Action(() => view.Fill(Core.Columns.OriginalIndex, 42, fillOffset, count - fillOffset))
				.Should().Throw<NoSuchColumnException>();

			for (int i = 0; i < offset + count + surplus; ++i)
			{
				buffer[i].Should().Be(originalBuffer[i]);
			}
		}
	}
}
