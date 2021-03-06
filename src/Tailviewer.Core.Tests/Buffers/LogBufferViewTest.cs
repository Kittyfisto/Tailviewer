﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Buffers
{
	[TestFixture]
	public sealed class LogBufferViewTest
	{
		[Test]
		public void TestConstruction()
		{
			var inner = new Mock<ILogBuffer>();
			var view = new LogBufferView(inner.Object, Core.Columns.LogLevel, Core.Columns.Message);
			view.Columns.Should().Equal(new object[] {Core.Columns.LogLevel, Core.Columns.Message});
			view.Contains(Core.Columns.LogLevel).Should().BeTrue();
			view.Contains(Core.Columns.Message).Should().BeTrue();
			view.Contains(Core.Columns.Index).Should().BeFalse();
			view.Contains(Core.Columns.LogEntryIndex).Should().BeFalse();

			inner.Setup(x => x.Count).Returns(42);
			view.Count.Should().Be(42);
		}

		[Test]
		public void TestCopyFromArray()
		{
			var inner = new Mock<ILogBuffer>();
			var view = new LogBufferView(inner.Object, Core.Columns.LogLevel, Core.Columns.Message);

			var source = new LevelFlags[101];
			view.CopyFrom(Core.Columns.LogLevel, 42, source, 2, 98);
			inner.Verify(x => x.CopyFrom(Core.Columns.LogLevel, 42, source, 2, 98), Times.Once);
		}

		[Test]
		public void TestCopyFromArray_NoSuchColumn()
		{
			var inner = new Mock<ILogBuffer>();
			var view = new LogBufferView(inner.Object, Core.Columns.LogLevel, Core.Columns.Message);

			var source = new DateTime?[101];
			new Action(() => view.CopyFrom(Core.Columns.Timestamp, 42, source, 2, 98)).Should().Throw<NoSuchColumnException>();
			inner.Verify(x => x.CopyFrom(Core.Columns.Timestamp, It.IsAny<int>(), It.IsAny<DateTime?[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
		}

		[Test]
		public void TestCopyFromLogFile_Contiguous()
		{
			var inner = new Mock<ILogBuffer>();
			var view = new LogBufferView(inner.Object, Core.Columns.LogLevel, Core.Columns.Message);

			var source = new Mock<ILogSource>();
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache);
			view.CopyFrom(Core.Columns.LogLevel, 42, source.Object, new LogSourceSection(2, 98), queryOptions);
			inner.Verify(x => x.CopyFrom(Core.Columns.LogLevel, 42, source.Object, new LogSourceSection(2, 98), queryOptions), Times.Once);
		}

		[Test]
		public void TestCopyFromLogFile_Contiguous_NoSuchColumn()
		{
			var inner = new Mock<ILogBuffer>();
			var view = new LogBufferView(inner.Object, Core.Columns.LogLevel, Core.Columns.Message);

			var source = new Mock<ILogSource>();
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache);
			new Action(() => view.CopyFrom(Core.Columns.Timestamp, 42, source.Object, new LogSourceSection(2, 98), queryOptions)).Should().Throw<NoSuchColumnException>();
			inner.Verify(x => x.CopyFrom(Core.Columns.Timestamp, It.IsAny<int>(), It.IsAny<ILogSource>(), It.IsAny<LogSourceSection>(), It.IsAny<LogSourceQueryOptions>()), Times.Never);
		}

		[Test]
		public void TestCopyFromLogFile_Noncontiguous()
		{
			var inner = new Mock<ILogBuffer>();
			var view = new LogBufferView(inner.Object, Core.Columns.LogLevel, Core.Columns.Message);

			var source = new Mock<ILogSource>();
			var sourceIndices = new[] {new LogLineIndex(1), new LogLineIndex(42)};
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache);
			view.CopyFrom(Core.Columns.LogLevel, 42, source.Object, sourceIndices, queryOptions);
			inner.Verify(x => x.CopyFrom(Core.Columns.LogLevel, 42, source.Object, sourceIndices, queryOptions), Times.Once);
		}

		[Test]
		public void TestCopyFromLogFile_Noncontiguous_NoSuchColumn()
		{
			var inner = new Mock<ILogBuffer>();
			var view = new LogBufferView(inner.Object, Core.Columns.LogLevel, Core.Columns.Message);

			var source = new Mock<ILogSource>();
			var sourceIndices = new[] {new LogLineIndex(1), new LogLineIndex(42)};
			var queryOptions = new LogSourceQueryOptions(LogSourceQueryMode.FromCache);
			new Action(() => view.CopyFrom(Core.Columns.Timestamp, 42, source.Object, sourceIndices, queryOptions)).Should().Throw<NoSuchColumnException>();
			inner.Verify(x => x.CopyFrom(Core.Columns.Timestamp, It.IsAny<int>(), It.IsAny<ILogSource>(), It.IsAny<IReadOnlyList<LogLineIndex>>(), It.IsAny<LogSourceQueryOptions>()), Times.Never);
		}

		[Test]
		public void TestFillDefault()
		{
			var inner = new Mock<ILogBuffer>();
			var view = new LogBufferView(inner.Object, Core.Columns.LogLevel, Core.Columns.Message);

			view.FillDefault(123, 456);
			inner.Verify(x => x.FillDefault(123, 456), Times.Once);
		}

		[Test]
		public void TestFillDefaultColumn()
		{
			var inner = new Mock<ILogBuffer>();
			var view = new LogBufferView(inner.Object, Core.Columns.LogLevel, Core.Columns.Message);

			view.FillDefault(Core.Columns.LogLevel, 123, 456);
			inner.Verify(x => x.FillDefault(Core.Columns.LogLevel, 123, 456), Times.Once);
		}

		[Test]
		public void TestFillDefaultColumn_NoSuchColumn()
		{
			var inner = new Mock<ILogBuffer>();
			var view = new LogBufferView(inner.Object, Core.Columns.LogLevel, Core.Columns.Message);

			new Action(() => view.FillDefault(Core.Columns.DeltaTime, 123, 456))
				.Should().Throw<NoSuchColumnException>();
			inner.Verify(x => x.FillDefault(It.IsAny<IColumnDescriptor>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
		}

		[Test]
		public void TestEnumerable()
		{
			var inner = new LogBufferList(Core.Columns.Minimum);
			inner.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{Core.Columns.RawContent, "Hello!"}, {Core.Columns.Timestamp, new DateTime(2021, 02, 11, 18, 49, 32)}}));
			var view = new LogBufferView(inner, Core.Columns.RawContent);

			var logEntries = view.ToList<ILogEntry>();
			logEntries.Should().HaveCount(1);
			var logEntryView = logEntries[0];
			logEntryView.Columns.Should().Equal(new object[] {Core.Columns.RawContent});
			logEntryView.RawContent.Should().Be("Hello!");
			logEntryView.RawContent = "What's up?";

			inner[0].RawContent.Should().Be("What's up?");
		}

		[Test]
		public void TestReadOnlyEnumerable()
		{
			var inner = new LogBufferList(Core.Columns.Minimum);
			inner.Add(new ReadOnlyLogEntry(new Dictionary<IColumnDescriptor, object>{{Core.Columns.RawContent, "Hello!"}, {Core.Columns.Timestamp, new DateTime(2021, 02, 11, 18, 49, 32)}}));
			var view = new LogBufferView(inner, Core.Columns.RawContent);

			var logEntries = view.ToList<IReadOnlyLogEntry>();
			logEntries.Should().HaveCount(1);
			var logEntryView = logEntries[0];
			logEntryView.Columns.Should().Equal(new object[] {Core.Columns.RawContent});
			logEntryView.RawContent.Should().Be("Hello!");
		}
	}
}
