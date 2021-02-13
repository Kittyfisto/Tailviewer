using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public abstract class AbstractReadOnlyLogEntryTest
	{
		protected abstract IReadOnlyLogEntry CreateDefault();

		protected abstract IReadOnlyLogEntry CreateEmpty();

		[Test]
		public void TestCreateDefault()
		{
			var entry = CreateDefault();
			entry.Columns.Should().Equal(LogFileColumns.Minimum);
			entry.DeltaTime.Should().Be(LogFileColumns.DeltaTime.DefaultValue);
			entry.ElapsedTime.Should().Be(LogFileColumns.ElapsedTime.DefaultValue);
			entry.Index.Should().Be(LogFileColumns.Index.DefaultValue);
			entry.LineNumber.Should().Be(LogFileColumns.LineNumber.DefaultValue);
			entry.LogEntryIndex.Should().Be(LogFileColumns.LogEntryIndex.DefaultValue);
			entry.LogLevel.Should().Be(LogFileColumns.LogLevel.DefaultValue);
			entry.OriginalIndex.Should().Be(LogFileColumns.OriginalIndex.DefaultValue);
			entry.OriginalLineNumber.Should().Be(LogFileColumns.OriginalLineNumber.DefaultValue);
			entry.OriginalDataSourceName.Should().Be(LogFileColumns.OriginalDataSourceName.DefaultValue);
			entry.RawContent.Should().Be(LogFileColumns.RawContent.DefaultValue);
			entry.Timestamp.Should().Be(LogFileColumns.Timestamp.DefaultValue);
			new Action(() =>
			{
				var unused = entry.SourceId;
			}).Should().Throw<NoSuchColumnException>();
		}

		[Test]
		public void TestTryGetValue1()
		{
			var column = new Mock<ILogFileColumnDescriptor<int>>();
			column.Setup(x => x.DefaultValue).Returns(42);

			var entry = CreateEmpty();
			int value;
			entry.TryGetValue(column.Object, out value).Should().BeFalse("because that column hasn't been added");
			value.Should().Be(42);
		}

		[Test]
		public void TestTryGetValue2()
		{
			var column = new Mock<ILogFileColumnDescriptor>();
			column.Setup(x => x.DefaultValue).Returns(42);

			var entry = CreateEmpty();
			object value;
			entry.TryGetValue(column.Object, out value).Should().BeFalse("because that column hasn't been added");
			value.Should().Be(42);
		}

		[Test]
		public void TestToStringEmpty()
		{
			var entry = CreateEmpty();
			entry.ToString().Should().Be("");
		}
	}
}