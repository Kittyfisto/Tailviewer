using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Entries
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
			entry.Columns.Should().Equal(Core.Columns.Minimum);
			entry.DeltaTime.Should().Be(Core.Columns.DeltaTime.DefaultValue);
			entry.ElapsedTime.Should().Be(Core.Columns.ElapsedTime.DefaultValue);
			entry.Index.Should().Be(Core.Columns.Index.DefaultValue);
			entry.LineNumber.Should().Be(Core.Columns.LineNumber.DefaultValue);
			entry.LogEntryIndex.Should().Be(Core.Columns.LogEntryIndex.DefaultValue);
			entry.LogLevel.Should().Be(Core.Columns.LogLevel.DefaultValue);
			entry.OriginalIndex.Should().Be(Core.Columns.OriginalIndex.DefaultValue);
			entry.OriginalLineNumber.Should().Be(Core.Columns.OriginalLineNumber.DefaultValue);
			entry.OriginalDataSourceName.Should().Be(Core.Columns.OriginalDataSourceName.DefaultValue);
			entry.RawContent.Should().Be(Core.Columns.RawContent.DefaultValue);
			entry.Timestamp.Should().Be(Core.Columns.Timestamp.DefaultValue);
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