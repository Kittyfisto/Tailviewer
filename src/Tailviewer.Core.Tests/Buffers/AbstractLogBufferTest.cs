﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Buffers
{
	[TestFixture]
	public abstract class AbstractLogBufferTest
		: AbstractReadOnlyLogBufferTest
	{
		protected abstract ILogBuffer Create(IEnumerable<IReadOnlyLogEntry> entries);

		[Test]
		public void TestGetSetValue()
		{
			var buffer = Create(new[] {new LogEntry(Core.Columns.RawContent, Core.Columns.LogLevel) {RawContent = "Hello, World!", LogLevel = LevelFlags.Error}});
			buffer.Count.Should().Be(1);
			var logEntry = buffer[0];
			logEntry.GetValue(Core.Columns.RawContent).Should().Be("Hello, World!");

			logEntry.SetValue(Core.Columns.RawContent, "Foobar 2000");
			logEntry.GetValue((IColumnDescriptor) Core.Columns.RawContent).Should().Be("Foobar 2000");
			logEntry.TryGetValue(Core.Columns.RawContent, out var rawContent).Should().BeTrue();
			rawContent.Should().Be("Foobar 2000");
			logEntry.TryGetValue((IColumnDescriptor)Core.Columns.RawContent, out var rawContent2).Should().BeTrue();
			rawContent2.Should().Be("Foobar 2000");

			logEntry.SetValue((IColumnDescriptor) Core.Columns.RawContent, "Sup?");
			logEntry.GetValue(Core.Columns.RawContent).Should().Be("Sup?");
		}

		[Test]
		public void TestGetValue_NoSuchColumn()
		{
			var buffer = Create(new[] {new LogEntry(Core.Columns.Message, Core.Columns.LogLevel) {Message = "Hello, World!", LogLevel = LevelFlags.Error}});
			buffer.Count.Should().Be(1);
			var logEntry = buffer[0];
			new Action(() => logEntry.GetValue(Core.Columns.RawContent)).Should().Throw<ColumnNotRetrievedException>();
			new Action(() => logEntry.GetValue((IColumnDescriptor)Core.Columns.RawContent)).Should().Throw<ColumnNotRetrievedException>();
			
			new Action(() => logEntry.GetValue(Core.Columns.OriginalDataSourceName)).Should().Throw<ColumnNotRetrievedException>();
			new Action(() => logEntry.GetValue((IColumnDescriptor)Core.Columns.OriginalDataSourceName)).Should().Throw<ColumnNotRetrievedException>();

			logEntry.TryGetValue(Core.Columns.Index, out var indexValue).Should().BeFalse();
			indexValue.Should().Be(Core.Columns.Index.DefaultValue);

			logEntry.TryGetValue((IColumnDescriptor)Core.Columns.Index, out var indexValue2).Should().BeFalse();
			indexValue2.Should().Be(Core.Columns.Index.DefaultValue);
		}

		[Test]
		public void TestSetValue_NoSuchColumn()
		{
			var buffer = Create(new[] {new LogEntry(Core.Columns.Message, Core.Columns.LogLevel) {Message = "Hello, World!", LogLevel = LevelFlags.Error}});
			buffer.Count.Should().Be(1);
			var logEntry = buffer[0];
			new Action(() => logEntry.SetValue(Core.Columns.RawContent, "Foo")).Should().Throw<NoSuchColumnException>();
			new Action(() => logEntry.SetValue((IColumnDescriptor)Core.Columns.RawContent, "bar")).Should().Throw<NoSuchColumnException>();
			
			new Action(() => logEntry.SetValue(Core.Columns.OriginalDataSourceName, "logs.log")).Should().Throw<NoSuchColumnException>();
			new Action(() => logEntry.SetValue((IColumnDescriptor)Core.Columns.OriginalDataSourceName, "logs.log")).Should().Throw<NoSuchColumnException>();
		}

		[Test]
		public void TestFillDefault_OneColumn()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal },
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info}
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(2);

			buffer[0].Message.Should().Be("Foo");
			buffer[1].Message.Should().Be("Bar");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal);
			buffer[1].LogLevel.Should().Be(LevelFlags.Info);

			buffer.FillDefault(Core.Columns.Message, 0, 1);
			buffer[0].Message.Should().Be(Core.Columns.Message.DefaultValue);
			buffer[1].Message.Should().Be("Bar");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the log level may not have been modified");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the log level may not have been modified");

			buffer.FillDefault(Core.Columns.LogLevel, 1, 1);
			buffer[0].Message.Should().Be(Core.Columns.Message.DefaultValue);
			buffer[1].Message.Should().Be("Bar");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the log level may not have been modified");
			buffer[1].LogLevel.Should().Be(Core.Columns.LogLevel.DefaultValue);
		}

		[Test]
		public void TestFillDefault_NoSuchColumn()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal },
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info }
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(2);

			new Action(() => buffer.FillDefault(Core.Columns.RawContent, 0, 2))
				.Should().Throw<NoSuchColumnException>();

			buffer[0].Message.Should().Be("Foo", "because the message may not have been modified");
			buffer[1].Message.Should().Be("Bar", "because the message may not have been modified");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the log level may not have been modified");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the log level may not have been modified");
		}

		[Test]
		public void TestFillDefault()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal },
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info },
				new LogEntry(columns) { Message = "Sup", LogLevel = LevelFlags.Debug }
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(3);

			buffer.FillDefault(1, 2);
			buffer[0].Message.Should().Be("Foo", "because we wanted to fill everything from log entry 1 onwards with default values - 0 should have been spared");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because we wanted to fill everything from log entry 1 onwards with default values - 0 should have been spared");

			buffer[1].Message.Should().Be(Core.Columns.Message.DefaultValue);
			buffer[1].LogLevel.Should().Be(Core.Columns.LogLevel.DefaultValue);
			buffer[2].Message.Should().Be(Core.Columns.Message.DefaultValue);
			buffer[2].LogLevel.Should().Be(Core.Columns.LogLevel.DefaultValue);
		}

		[Test]
		public void TestFillDefault_InvalidOffset()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal },
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info },
				new LogEntry(columns) { Message = "Sup", LogLevel = LevelFlags.Debug }
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(3);

			new Action(() => buffer.FillDefault(-1, 2)).Should().Throw<ArgumentOutOfRangeException>();
			buffer[0].Message.Should().Be("Foo", "because the data may not have been modified");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the data may not have been modified");
			buffer[1].Message.Should().Be("Bar", "because the data may not have been modified");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the data may not have been modified");
			buffer[2].Message.Should().Be("Sup", "because the data may not have been modified");
			buffer[2].LogLevel.Should().Be(LevelFlags.Debug, "because the data may not have been modified");
		}

		[Test]
		public void TestFillDefault_InvalidOffsetPlusCount()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal },
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info },
				new LogEntry(columns) { Message = "Sup", LogLevel = LevelFlags.Debug }
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(3);

			new Action(() => buffer.FillDefault(1, 3)).Should().Throw<ArgumentException>();
			buffer[0].Message.Should().Be("Foo", "because the data may not have been modified");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the data may not have been modified");
			buffer[1].Message.Should().Be("Bar", "because the data may not have been modified");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the data may not have been modified");
			buffer[2].Message.Should().Be("Sup", "because the data may not have been modified");
			buffer[2].LogLevel.Should().Be(LevelFlags.Debug, "because the data may not have been modified");
		}

		[Test]
		public void TestCopyFrom_OneColumn()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal },
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info },
				new LogEntry(columns) { Message = "Sup", LogLevel = LevelFlags.Debug }
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(3);
			
			var source = new string[]
			{
				"This",
				"is",
				"too",
				"much",
				"work"
			};
			buffer.CopyFrom(Core.Columns.Message, 1, source, 1, 2);

			new Action(() => buffer.FillDefault(1, 3)).Should().Throw<ArgumentException>();
			buffer[0].Message.Should().Be("Foo", "because the data may not have been modified");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the data may not have been modified");
			buffer[1].Message.Should().Be("is");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the data may not have been modified");
			buffer[2].Message.Should().Be("too");
			buffer[2].LogLevel.Should().Be(LevelFlags.Debug, "because the data may not have been modified");
		}

		[Test]
		public void TestCopyFrom_NoSuchColumn()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal },
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info },
				new LogEntry(columns) { Message = "Sup", LogLevel = LevelFlags.Debug }
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(3);

			var source = new string[]
			{
				"This",
				"is",
				"too",
				"much",
				"work"
			};
			new Action(() => buffer.CopyFrom(Core.Columns.RawContent, 0, source, 1, 3)).Should()
				.Throw<NoSuchColumnException>();

			new Action(() => buffer.FillDefault(1, 3)).Should().Throw<ArgumentException>();
			buffer[0].Message.Should().Be("Foo", "because the data may not have been modified");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the data may not have been modified");
			buffer[1].Message.Should().Be("Bar", "because the data may not have been modified");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the data may not have been modified");
			buffer[2].Message.Should().Be("Sup", "because the data may not have been modified");
			buffer[2].LogLevel.Should().Be(LevelFlags.Debug, "because the data may not have been modified");
		}

		[Test]
		public void TestCopyFrom_NullColumn()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal },
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info },
				new LogEntry(columns) { Message = "Sup", LogLevel = LevelFlags.Debug }
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(3);

			var source = new string[]
			{
				"This",
				"is",
				"too",
				"much",
				"work"
			};
			new Action(() => buffer.CopyFrom(null, 0, source, 1, 3)).Should()
				.Throw<ArgumentNullException>();

			new Action(() => buffer.FillDefault(1, 3)).Should().Throw<ArgumentException>();
			buffer[0].Message.Should().Be("Foo", "because the data may not have been modified");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the data may not have been modified");
			buffer[1].Message.Should().Be("Bar", "because the data may not have been modified");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the data may not have been modified");
			buffer[2].Message.Should().Be("Sup", "because the data may not have been modified");
			buffer[2].LogLevel.Should().Be(LevelFlags.Debug, "because the data may not have been modified");
		}

		[Test]
		public void TestCopyFrom_InvalidDestinationIndex()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal },
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info },
				new LogEntry(columns) { Message = "Sup", LogLevel = LevelFlags.Debug }
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(3);

			var source = new string[]
			{
				"This",
				"is",
				"too",
				"much",
				"work"
			};
			new Action(() => buffer.CopyFrom(Core.Columns.Message, -1, source, 1, 3)).Should()
			                                                        .Throw<ArgumentOutOfRangeException>();

			new Action(() => buffer.FillDefault(1, 3)).Should().Throw<ArgumentException>();
			buffer[0].Message.Should().Be("Foo", "because the data may not have been modified");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the data may not have been modified");
			buffer[1].Message.Should().Be("Bar", "because the data may not have been modified");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the data may not have been modified");
			buffer[2].Message.Should().Be("Sup", "because the data may not have been modified");
			buffer[2].LogLevel.Should().Be(LevelFlags.Debug, "because the data may not have been modified");
		}

		[Test]
		public void TestCopyFrom_InvalidDestinationLength()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal },
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info },
				new LogEntry(columns) { Message = "Sup", LogLevel = LevelFlags.Debug }
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(3);

			var source = new string[]
			{
				"This",
				"is",
				"too",
				"much",
				"work"
			};
			new Action(() => buffer.CopyFrom(Core.Columns.Message, 0, source, 1, 4)).Should()
				.Throw<ArgumentOutOfRangeException>();

			new Action(() => buffer.FillDefault(1, 3)).Should().Throw<ArgumentException>();
			buffer[0].Message.Should().Be("Foo", "because the data may not have been modified");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the data may not have been modified");
			buffer[1].Message.Should().Be("Bar", "because the data may not have been modified");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the data may not have been modified");
			buffer[2].Message.Should().Be("Sup", "because the data may not have been modified");
			buffer[2].LogLevel.Should().Be(LevelFlags.Debug, "because the data may not have been modified");
		}

		[Test]
		public void TestCopyFrom_InvalidSourceLength()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal },
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info },
				new LogEntry(columns) { Message = "Sup", LogLevel = LevelFlags.Debug }
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(3);

			var source = new string[]
			{
				"This",
				"is"
			};
			new Action(() => buffer.CopyFrom(Core.Columns.Message, 1, source, 0, 3)).Should()
				.Throw<ArgumentOutOfRangeException>();

			new Action(() => buffer.FillDefault(1, 3)).Should().Throw<ArgumentException>();
			buffer[0].Message.Should().Be("Foo", "because the data may not have been modified");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the data may not have been modified");
			buffer[1].Message.Should().Be("Bar", "because the data may not have been modified");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the data may not have been modified");
			buffer[2].Message.Should().Be("Sup", "because the data may not have been modified");
			buffer[2].LogLevel.Should().Be(LevelFlags.Debug, "because the data may not have been modified");
		}

		[Test]
		public void TestCopyFromLogBuffer_OneColumn()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel, Core.Columns.LineNumber};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal, LineNumber = 2},
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info, LineNumber = 12},
				new LogEntry(columns) { Message = "Sup", LogLevel = LevelFlags.Debug, LineNumber = 42}
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(3);

			var source = new LogBufferList(columns)
			{
				new LogEntry(columns) {Message = "Hello", LogLevel = LevelFlags.Trace, LineNumber = 4},
				new LogEntry(columns) {Message = "World", LogLevel = LevelFlags.Trace, LineNumber = 5},
				new LogEntry(columns) {Message = "What", LogLevel = LevelFlags.Warning, LineNumber = 8},
				new LogEntry(columns) {Message = "Goes?", LogLevel = LevelFlags.None, LineNumber = 9}
			};
			buffer.CopyFrom(Core.Columns.LineNumber, 1, source, new[] {3, 1});

			new Action(() => buffer.FillDefault(1, 3)).Should().Throw<ArgumentException>();
			buffer[0].Message.Should().Be("Foo", "because the data may not have been modified");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the data may not have been modified");
			buffer[0].LineNumber.Should().Be(2, "because the data may not have been modified");
			buffer[1].Message.Should().Be("Bar", "because the data may not have been modified");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the data may not have been modified");
			buffer[1].LineNumber.Should().Be(9);
			buffer[2].Message.Should().Be("Sup", "because the data may not have been modified");
			buffer[2].LogLevel.Should().Be(LevelFlags.Debug, "because the data may not have been modified");
			buffer[2].LineNumber.Should().Be(5);
		}

		[Test]
		public void TestCopyFromLogBuffer_NoSuchColumn()
		{
			var columns = new IColumnDescriptor[] {Core.Columns.Message, Core.Columns.LogLevel};
			var entries = new[]
			{
				new LogEntry(columns) { Message = "Foo", LogLevel = LevelFlags.Fatal},
				new LogEntry(columns) { Message = "Bar", LogLevel = LevelFlags.Info},
				new LogEntry(columns) { Message = "Sup", LogLevel = LevelFlags.Debug}
			};

			var buffer = Create(entries);
			buffer.Count.Should().Be(3);

			var source = new LogBufferList(Core.Columns.Message, Core.Columns.LogLevel, Core.Columns.LineNumber)
			{
				new LogEntry(columns) {Message = "Hello", LogLevel = LevelFlags.Trace, LineNumber = 4},
				new LogEntry(columns) {Message = "World", LogLevel = LevelFlags.Trace, LineNumber = 5},
				new LogEntry(columns) {Message = "What", LogLevel = LevelFlags.Warning, LineNumber = 8},
				new LogEntry(columns) {Message = "Goes?", LogLevel = LevelFlags.None, LineNumber = 9}
			};
			new Action(() => buffer.CopyFrom(Core.Columns.LineNumber, 1, source, new[] {3, 1}))
				.Should().Throw<NoSuchColumnException>();

			new Action(() => buffer.FillDefault(1, 3)).Should().Throw<ArgumentException>();
			buffer[0].Message.Should().Be("Foo", "because the data may not have been modified");
			buffer[0].LogLevel.Should().Be(LevelFlags.Fatal, "because the data may not have been modified");
			buffer[1].Message.Should().Be("Bar", "because the data may not have been modified");
			buffer[1].LogLevel.Should().Be(LevelFlags.Info, "because the data may not have been modified");
			buffer[2].Message.Should().Be("Sup", "because the data may not have been modified");
			buffer[2].LogLevel.Should().Be(LevelFlags.Debug, "because the data may not have been modified");
		}
	}
}
