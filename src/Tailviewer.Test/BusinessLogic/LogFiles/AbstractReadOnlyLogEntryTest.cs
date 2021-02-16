using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Core.Columns;

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
			entry.Columns.Should().Equal(LogColumns.Minimum);
			entry.DeltaTime.Should().Be(LogColumns.DeltaTime.DefaultValue);
			entry.ElapsedTime.Should().Be(LogColumns.ElapsedTime.DefaultValue);
			entry.Index.Should().Be(LogColumns.Index.DefaultValue);
			entry.LineNumber.Should().Be(LogColumns.LineNumber.DefaultValue);
			entry.LogEntryIndex.Should().Be(LogColumns.LogEntryIndex.DefaultValue);
			entry.LogLevel.Should().Be(LogColumns.LogLevel.DefaultValue);
			entry.OriginalIndex.Should().Be(LogColumns.OriginalIndex.DefaultValue);
			entry.OriginalLineNumber.Should().Be(LogColumns.OriginalLineNumber.DefaultValue);
			entry.OriginalDataSourceName.Should().Be(LogColumns.OriginalDataSourceName.DefaultValue);
			entry.RawContent.Should().Be(LogColumns.RawContent.DefaultValue);
			entry.Timestamp.Should().Be(LogColumns.Timestamp.DefaultValue);
			new Action(() =>
			{
				var unused = entry.SourceId;
			}).Should().Throw<NoSuchColumnException>();
		}

		[Test]
		public void TestTryGetValue1()
		{
			var column = new Mock<IColumnDescriptor<int>>();
			column.Setup(x => x.DefaultValue).Returns(42);

			var entry = CreateEmpty();
			int value;
			entry.TryGetValue(column.Object, out value).Should().BeFalse("because that column hasn't been added");
			value.Should().Be(42);
		}

		[Test]
		public void TestTryGetValue2()
		{
			var column = new Mock<IColumnDescriptor>();
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