using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Core.Columns;

namespace Tailviewer.Tests.BusinessLogic.Entries
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
			entry.Columns.Should().Equal(GeneralColumns.Minimum);
			entry.DeltaTime.Should().Be(GeneralColumns.DeltaTime.DefaultValue);
			entry.ElapsedTime.Should().Be(GeneralColumns.ElapsedTime.DefaultValue);
			entry.Index.Should().Be(GeneralColumns.Index.DefaultValue);
			entry.LineNumber.Should().Be(GeneralColumns.LineNumber.DefaultValue);
			entry.LogEntryIndex.Should().Be(GeneralColumns.LogEntryIndex.DefaultValue);
			entry.LogLevel.Should().Be(GeneralColumns.LogLevel.DefaultValue);
			entry.OriginalIndex.Should().Be(GeneralColumns.OriginalIndex.DefaultValue);
			entry.OriginalLineNumber.Should().Be(GeneralColumns.OriginalLineNumber.DefaultValue);
			entry.OriginalDataSourceName.Should().Be(GeneralColumns.OriginalDataSourceName.DefaultValue);
			entry.RawContent.Should().Be(GeneralColumns.RawContent.DefaultValue);
			entry.Timestamp.Should().Be(GeneralColumns.Timestamp.DefaultValue);
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