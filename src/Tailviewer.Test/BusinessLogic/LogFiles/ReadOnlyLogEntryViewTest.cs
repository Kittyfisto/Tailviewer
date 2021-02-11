using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class ReadOnlyLogEntryViewTest
	{
		[Test]
		public void TestConstruction()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.RawContent, LogFileColumns.Index);
			view.Columns.Should().Equal(new object[] {LogFileColumns.RawContent, LogFileColumns.Index});
			entry.VerifyGet(x => x.Columns, Times.Never);
		}

		[Test]
		public void TestRawContent_ColumnAvailable()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.RawContent);

			entry.SetupGet(x => x.RawContent).Returns("I want a clondyke bar, I'm hungry");
			view.RawContent.Should().Be("I want a clondyke bar, I'm hungry");
		}

		[Test]
		public void TestRawContent_NoSuchColumn()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.RawContent;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.RawContent, Times.Never);
		}

		[Test]
		public void TestIndex_ColumnAvailable()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Index);

			entry.SetupGet(x => x.Index).Returns(new LogLineIndex(101));
			view.Index.Should().Be(new LogLineIndex(101));
		}

		[Test]
		public void TestIndex_NoSuchColumn()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.Index;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.Index, Times.Never);
		}

		[Test]
		public void TestOriginalIndex_ColumnAvailable()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.OriginalIndex);

			entry.SetupGet(x => x.OriginalIndex).Returns(new LogLineIndex(101));
			view.OriginalIndex.Should().Be(new LogLineIndex(101));
		}

		[Test]
		public void TestOriginalIndex_NoSuchColumn()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.OriginalIndex;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.OriginalIndex, Times.Never);
		}

		[Test]
		public void TestLogEntryIndex_ColumnAvailable()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.LogEntryIndex);

			entry.SetupGet(x => x.LogEntryIndex).Returns(new LogEntryIndex(101));
			view.LogEntryIndex.Should().Be(new LogEntryIndex(101));
		}

		[Test]
		public void TestLogEntryIndex_NoSuchColumn()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.LogEntryIndex;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.LogEntryIndex, Times.Never);
		}

		[Test]
		public void TestLineNumber_ColumnAvailable()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.LineNumber);

			entry.SetupGet(x => x.LineNumber).Returns(101);
			view.LineNumber.Should().Be(101);
		}

		[Test]
		public void TestLineNumber_NoSuchColumn()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.LineNumber;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.LineNumber, Times.Never);
		}

		[Test]
		public void TestOriginalLineNumber_ColumnAvailable()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.OriginalLineNumber);

			entry.SetupGet(x => x.OriginalLineNumber).Returns(101);
			view.OriginalLineNumber.Should().Be(101);
		}

		[Test]
		public void TestOriginalLineNumber_NoSuchColumn()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.OriginalLineNumber;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.OriginalLineNumber, Times.Never);
		}

		[Test]
		public void TestLogLevel_ColumnAvailable()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.LogLevel);

			entry.SetupGet(x => x.LogLevel).Returns(LevelFlags.Error);
			view.LogLevel.Should().Be(LevelFlags.Error);
		}

		[Test]
		public void TestLogLevel_NoSuchColumn()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.LogLevel;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.LogLevel, Times.Never);
		}

		[Test]
		public void TestTimestamp_ColumnAvailable()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Timestamp);

			entry.SetupGet(x => x.Timestamp).Returns(new DateTime(2021, 02, 11, 15, 42, 1));
			view.Timestamp.Should().Be(new DateTime(2021, 02, 11, 15, 42, 1));
		}

		[Test]
		public void TestTimestamp_NoSuchColumn()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.Timestamp;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.Timestamp, Times.Never);
		}

		[Test]
		public void TestElapsedTime_ColumnAvailable()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.ElapsedTime);

			entry.SetupGet(x => x.ElapsedTime).Returns(TimeSpan.FromSeconds(42));
			view.ElapsedTime.Should().Be(TimeSpan.FromSeconds(42));
		}

		[Test]
		public void TestElapsedTime_NoSuchColumn()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.ElapsedTime;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.ElapsedTime, Times.Never);
		}

		[Test]
		public void TestDeltaTime_ColumnAvailable()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.DeltaTime);

			entry.SetupGet(x => x.DeltaTime).Returns(TimeSpan.FromSeconds(42));
			view.DeltaTime.Should().Be(TimeSpan.FromSeconds(42));
		}

		[Test]
		public void TestDeltaTime_NoSuchColumn()
		{
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.DeltaTime;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.DeltaTime, Times.Never);
		}

		[Test]
		public void TestGetSetValue_ColumnAvailable()
		{
			var column = new Mock<ILogFileColumn>().Object;
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, column);

			entry.Setup(x => x.GetValue(column)).Returns("Foobar");
			view.GetValue(column).Should().Be("Foobar");
			entry.Verify(x => x.GetValue(column), Times.Once);
		}

		[Test]
		public void TestGetSetValue_NoSuchColumn()
		{
			var column = new Mock<ILogFileColumn>().Object;
			var anotherColumn = new Mock<ILogFileColumn>().Object;
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, column);

			new Action(() => view.GetValue(anotherColumn)).Should().Throw<NoSuchColumnException>();
			entry.Verify(x => x.GetValue(It.IsAny<ILogFileColumn>()), Times.Never);
		}

		[Test]
		public void TestGetSetValueTyped_ColumnAvailable()
		{
			var column = new Mock<ILogFileColumn<long>>().Object;
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, column);

			entry.Setup(x => x.GetValue(column)).Returns(342L);
			view.GetValue(column).Should().Be(342L);
			entry.Verify(x => x.GetValue<long>(column), Times.Once);
		}

		[Test]
		public void TestGetSetValueTyped_NoSuchColumn()
		{
			var column = new Mock<ILogFileColumn<long>>().Object;
			var anotherColumn = new Mock<ILogFileColumn<long>>().Object;
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, column);

			new Action(() => view.GetValue(anotherColumn)).Should().Throw<NoSuchColumnException>();
			entry.Verify(x => x.GetValue<long>(It.IsAny<ILogFileColumn<long>>()), Times.Never);
		}

		[Test]
		public void TestTryGetValue_ColumnAvailable()
		{
			var column = new Mock<ILogFileColumn>().Object;
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, column);

			object expectedValue = "Hello";
			entry.Setup(x => x.TryGetValue(column, out expectedValue)).Returns(true);
			view.TryGetValue(column, out var actualValue).Should().BeTrue();
			actualValue.Should().Be(expectedValue);
		}

		[Test]
		public void TestTryGetValue_NoSuchColumn()
		{
			var column = new Mock<ILogFileColumn>().Object;
			var otherColumn = new Mock<ILogFileColumn>();
			otherColumn.SetupGet(x => x.DefaultValue).Returns("<No Value>");
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, column);

			view.TryGetValue(otherColumn.Object, out var actualValue).Should().BeFalse();
			actualValue.Should().Be(otherColumn.Object.DefaultValue);
		}

		[Test]
		public void TestTryGetValueTyped_ColumnAvailable()
		{
			var column = new Mock<ILogFileColumn<string>>().Object;
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, column);

			var expectedValue = "Hello";
			entry.Setup(x => x.TryGetValue(column, out expectedValue)).Returns(true);
			view.TryGetValue(column, out var actualValue).Should().BeTrue();
			actualValue.Should().Be(expectedValue);
		}

		[Test]
		public void TestTryGetValueTyped_NoSuchColumn()
		{
			var column = new Mock<ILogFileColumn<string>>().Object;
			var otherColumn = new Mock<ILogFileColumn<string>>();
			otherColumn.SetupGet(x => x.DefaultValue).Returns("<No Value>");
			var entry = new Mock<IReadOnlyLogEntry>();
			var view = new ReadOnlyLogEntryView(entry.Object, column);

			view.TryGetValue(otherColumn.Object, out var actualValue).Should().BeFalse();
			actualValue.Should().Be(otherColumn.Object.DefaultValue);
		}
	}
}