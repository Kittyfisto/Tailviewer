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
	public sealed class LogEntryViewTest
	{
		[Test]
		public void TestConstruction()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.RawContent, LogFileColumns.Index);
			view.Columns.Should().Equal(new object[] {LogFileColumns.RawContent, LogFileColumns.Index});
			entry.VerifyGet(x => x.Columns, Times.Never);
		}

		[Test]
		public void TestRawContent_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.RawContent);

			entry.SetupProperty(x => x.RawContent);
			entry.Object.RawContent = "I want a clondyke bar, I'm hungry";
			view.RawContent.Should().Be("I want a clondyke bar, I'm hungry");

			view.RawContent = "I'm still hungry";
			entry.Object.RawContent.Should().Be("I'm still hungry");
		}

		[Test]
		public void TestRawContent_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.RawContent;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.RawContent, Times.Never);

			new Action(() => { view.RawContent = "fwadwwa"; }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.RawContent = "fwadwwa", Times.Never);
		}

		[Test]
		public void TestIndex_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Index);

			entry.SetupProperty(x => x.Index);
			entry.Object.Index = new LogLineIndex(101);
			view.Index.Should().Be(new LogLineIndex(101));

			view.Index = new LogLineIndex(9001);
			entry.Object.Index.Should().Be(new LogLineIndex(9001));
		}

		[Test]
		public void TestIndex_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.Index;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.Index, Times.Never);

			new Action(() => { view.Index = new LogLineIndex(9001); }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.Index = new LogLineIndex(9001), Times.Never);
		}

		[Test]
		public void TestOriginalIndex_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.OriginalIndex);

			entry.SetupProperty(x => x.OriginalIndex);
			entry.Object.OriginalIndex = new LogLineIndex(101);
			view.OriginalIndex.Should().Be(new LogLineIndex(101));

			view.OriginalIndex = new LogLineIndex(9001);
			entry.Object.OriginalIndex.Should().Be(new LogLineIndex(9001));
		}

		[Test]
		public void TestOriginalIndex_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.OriginalIndex;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.OriginalIndex, Times.Never);

			new Action(() => { view.OriginalIndex = new LogLineIndex(9001); }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.OriginalIndex = new LogLineIndex(9001), Times.Never);
		}

		[Test]
		public void TestOriginalDataSourceName_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.OriginalDataSourceName);

			entry.SetupProperty(x => x.OriginalDataSourceName);
			entry.Object.OriginalDataSourceName = "F:\\important.txt";
			view.OriginalDataSourceName.Should().Be("F:\\important.txt");

			view.OriginalDataSourceName = "A:\\another_one.log";
			entry.Object.OriginalDataSourceName.Should().Be("A:\\another_one.log");
		}

		[Test]
		public void TestOriginalDataSourceName_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.OriginalDataSourceName;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.OriginalDataSourceName, Times.Never);

			new Action(() => { view.OriginalDataSourceName = "F:\\important.txt"; }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.OriginalDataSourceName = "F:\\important.txt", Times.Never);
		}

		[Test]
		public void TestSourceId_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.SourceId);

			entry.SetupProperty(x => x.SourceId);
			entry.Object.SourceId = new LogLineSourceId(101);
			view.SourceId.Should().Be(new LogLineSourceId(101));

			view.SourceId = new LogLineSourceId(201);
			entry.Object.SourceId.Should().Be(new LogLineSourceId(201));
		}

		[Test]
		public void TestSourceId_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.SourceId;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.SourceId, Times.Never);

			new Action(() => { view.SourceId = new LogLineSourceId(101); }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.SourceId = new LogLineSourceId(101), Times.Never);
		}

		[Test]
		public void TestLogEntryIndex_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.LogEntryIndex);

			entry.SetupProperty(x => x.LogEntryIndex);
			entry.Object.LogEntryIndex = new LogEntryIndex(101);
			view.LogEntryIndex.Should().Be(new LogEntryIndex(101));

			view.LogEntryIndex = new LogEntryIndex(9001);
			entry.Object.LogEntryIndex.Should().Be(new LogEntryIndex(9001));
		}

		[Test]
		public void TestLogEntryIndex_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.LogEntryIndex;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.LogEntryIndex, Times.Never);

			new Action(() => { view.LogEntryIndex = new LogEntryIndex(9001); }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.LogEntryIndex = new LogEntryIndex(9001), Times.Never);
		}

		[Test]
		public void TestLineNumber_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.LineNumber);

			entry.SetupProperty(x => x.LineNumber);
			entry.Object.LineNumber = 101;
			view.LineNumber.Should().Be(101);

			view.LineNumber = 9001;
			entry.Object.LineNumber.Should().Be(9001);
		}

		[Test]
		public void TestLineNumber_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.LineNumber;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.LineNumber, Times.Never);

			new Action(() => { view.LineNumber = 9001; }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.LineNumber = 9001, Times.Never);
		}

		[Test]
		public void TestOriginalLineNumber_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.OriginalLineNumber);

			entry.SetupProperty(x => x.OriginalLineNumber);
			entry.Object.OriginalLineNumber = 101;
			view.OriginalLineNumber.Should().Be(101);

			view.OriginalLineNumber = 9001;
			entry.Object.OriginalLineNumber.Should().Be(9001);
		}

		[Test]
		public void TestOriginalLineNumber_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.OriginalLineNumber;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.OriginalLineNumber, Times.Never);

			new Action(() => { view.OriginalLineNumber = 9001; }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.OriginalLineNumber = 9001, Times.Never);
		}

		[Test]
		public void TestLogLevel_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.LogLevel);

			entry.SetupProperty(x => x.LogLevel);
			entry.Object.LogLevel = LevelFlags.Error;
			view.LogLevel.Should().Be(LevelFlags.Error);

			view.LogLevel = LevelFlags.Warning;
			entry.Object.LogLevel.Should().Be(LevelFlags.Warning);
		}

		[Test]
		public void TestLogLevel_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.LogLevel;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.LogLevel, Times.Never);

			new Action(() => { view.LogLevel = LevelFlags.Fatal; }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.LogLevel = LevelFlags.Fatal, Times.Never);
		}

		[Test]
		public void TestTimestamp_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Timestamp);

			entry.SetupProperty(x => x.Timestamp);
			entry.Object.Timestamp = new DateTime(2021, 02, 11, 15, 42, 1);
			view.Timestamp.Should().Be(new DateTime(2021, 02, 11, 15, 42, 1));

			view.Timestamp = new DateTime(201, 02, 11, 15, 43, 1);
			entry.Object.Timestamp.Should().Be(new DateTime(201, 02, 11, 15, 43, 1));
		}

		[Test]
		public void TestTimestamp_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.Timestamp;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.Timestamp, Times.Never);

			new Action(() => { view.Timestamp = new DateTime(201, 02, 11, 15, 44, 1); }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.Timestamp = new DateTime(201, 02, 11, 15, 44, 1), Times.Never);
		}

		[Test]
		public void TestElapsedTime_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.ElapsedTime);

			entry.SetupProperty(x => x.ElapsedTime);
			entry.Object.ElapsedTime = TimeSpan.FromSeconds(42);
			view.ElapsedTime.Should().Be(TimeSpan.FromSeconds(42));

			view.ElapsedTime = TimeSpan.FromSeconds(9001);
			entry.Object.ElapsedTime.Should().Be(TimeSpan.FromSeconds(9001));
		}

		[Test]
		public void TestElapsedTime_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.ElapsedTime;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.ElapsedTime, Times.Never);

			new Action(() => { view.ElapsedTime = TimeSpan.FromSeconds(9001); }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.ElapsedTime = TimeSpan.FromSeconds(9001), Times.Never);
		}

		[Test]
		public void TestDeltaTime_ColumnAvailable()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.DeltaTime);

			entry.SetupProperty(x => x.DeltaTime);
			entry.Object.DeltaTime = TimeSpan.FromSeconds(42);
			view.DeltaTime.Should().Be(TimeSpan.FromSeconds(42));

			view.DeltaTime = TimeSpan.FromSeconds(9001);
			entry.Object.DeltaTime.Should().Be(TimeSpan.FromSeconds(9001));
		}

		[Test]
		public void TestDeltaTime_NoSuchColumn()
		{
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, LogFileColumns.Message);

			new Action(() =>
			{
				var unused = view.DeltaTime;
			}).Should().Throw<NoSuchColumnException>();
			entry.VerifyGet(x => x.DeltaTime, Times.Never);

			new Action(() => { view.DeltaTime = TimeSpan.FromSeconds(9001); }).Should().Throw<NoSuchColumnException>();
			entry.VerifySet(x => x.DeltaTime = TimeSpan.FromSeconds(9001), Times.Never);
		}

		[Test]
		public void TestGetSetValue_ColumnAvailable()
		{
			var column = new Mock<ILogFileColumnDescriptor>().Object;
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, column);

			entry.Setup(x => x.GetValue(column)).Returns("Foobar");
			view.GetValue(column).Should().Be("Foobar");
			entry.Verify(x => x.GetValue(column), Times.Once);

			view.SetValue(column, "Hello");
			entry.Verify(x => x.SetValue(column, "Hello"), Times.Once);
		}

		[Test]
		public void TestGetSetValue_NoSuchColumn()
		{
			var column = new Mock<ILogFileColumnDescriptor>().Object;
			var anotherColumn = new Mock<ILogFileColumnDescriptor>().Object;
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, column);

			new Action(() => view.GetValue(anotherColumn)).Should().Throw<NoSuchColumnException>();
			entry.Verify(x => x.GetValue(It.IsAny<ILogFileColumnDescriptor>()), Times.Never);

			new Action(() => view.SetValue(anotherColumn, "Foo")).Should().Throw<NoSuchColumnException>();
			entry.Verify(x => x.SetValue(column, It.IsAny<object>()), Times.Never);
		}

		[Test]
		public void TestGetSetValueTyped_ColumnAvailable()
		{
			var column = new Mock<ILogFileColumnDescriptor<long>>().Object;
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, column);

			entry.Setup(x => x.GetValue(column)).Returns(342L);
			view.GetValue(column).Should().Be(342L);
			entry.Verify(x => x.GetValue<long>(column), Times.Once);

			view.SetValue(column, 41231231L);
			entry.Verify(x => x.SetValue<long>(column, 41231231L), Times.Once);
		}

		[Test]
		public void TestGetSetValueTyped_NoSuchColumn()
		{
			var column = new Mock<ILogFileColumnDescriptor<long>>().Object;
			var anotherColumn = new Mock<ILogFileColumnDescriptor<long>>().Object;
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, column);

			new Action(() => view.GetValue(anotherColumn)).Should().Throw<NoSuchColumnException>();
			entry.Verify(x => x.GetValue<long>(It.IsAny<ILogFileColumnDescriptor<long>>()), Times.Never);

			new Action(() => view.SetValue<long>(anotherColumn, 98L)).Should().Throw<NoSuchColumnException>();
			entry.Verify(x => x.SetValue<long>(column, It.IsAny<long>()), Times.Never);
		}

		[Test]
		public void TestTryGetValue_ColumnAvailable()
		{
			var column = new Mock<ILogFileColumnDescriptor>().Object;
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, column);

			object expectedValue = "Hello";
			entry.Setup(x => x.TryGetValue(column, out expectedValue)).Returns(true);
			view.TryGetValue(column, out var actualValue).Should().BeTrue();
			actualValue.Should().Be(expectedValue);
		}

		[Test]
		public void TestTryGetValue_NoSuchColumn()
		{
			var column = new Mock<ILogFileColumnDescriptor>().Object;
			var otherColumn = new Mock<ILogFileColumnDescriptor>();
			otherColumn.SetupGet(x => x.DefaultValue).Returns("<No Value>");
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, column);

			view.TryGetValue(otherColumn.Object, out var actualValue).Should().BeFalse();
			actualValue.Should().Be(otherColumn.Object.DefaultValue);
		}

		[Test]
		public void TestTryGetValueTyped_ColumnAvailable()
		{
			var column = new Mock<ILogFileColumnDescriptor<string>>().Object;
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, column);

			var expectedValue = "Hello";
			entry.Setup(x => x.TryGetValue(column, out expectedValue)).Returns(true);
			view.TryGetValue(column, out var actualValue).Should().BeTrue();
			actualValue.Should().Be(expectedValue);
		}

		[Test]
		public void TestTryGetValueTyped_NoSuchColumn()
		{
			var column = new Mock<ILogFileColumnDescriptor<string>>().Object;
			var otherColumn = new Mock<ILogFileColumnDescriptor<string>>();
			otherColumn.SetupGet(x => x.DefaultValue).Returns("<No Value>");
			var entry = new Mock<ILogEntry>();
			var view = new LogEntryView(entry.Object, column);

			view.TryGetValue(otherColumn.Object, out var actualValue).Should().BeFalse();
			actualValue.Should().Be(otherColumn.Object.DefaultValue);
		}
	}
}