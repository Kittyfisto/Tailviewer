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
			entry.Columns.Should().Equal(Columns.Minimum);
			entry.DeltaTime.Should().Be(Columns.DeltaTime.DefaultValue);
			entry.ElapsedTime.Should().Be(Columns.ElapsedTime.DefaultValue);
			entry.Index.Should().Be(Columns.Index.DefaultValue);
			entry.LineNumber.Should().Be(Columns.LineNumber.DefaultValue);
			entry.LogEntryIndex.Should().Be(Columns.LogEntryIndex.DefaultValue);
			entry.LogLevel.Should().Be(Columns.LogLevel.DefaultValue);
			entry.OriginalIndex.Should().Be(Columns.OriginalIndex.DefaultValue);
			entry.OriginalLineNumber.Should().Be(Columns.OriginalLineNumber.DefaultValue);
			entry.OriginalDataSourceName.Should().Be(Columns.OriginalDataSourceName.DefaultValue);
			entry.RawContent.Should().Be(Columns.RawContent.DefaultValue);
			entry.Timestamp.Should().Be(Columns.Timestamp.DefaultValue);
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