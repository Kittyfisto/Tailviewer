using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Columns
{
	[TestFixture]
	public sealed class CustomColumnDescriptorTest
	{
		[Test]
		public void TestConstruction1([Values("stuff", "foo")] string id)
		{
			var column = new CustomColumnDescriptor<DateTime>(id);
			column.Id.Should().Be(id);
			column.DataType.Should().Be<DateTime>();
		}

		[Test]
		public void TestConstruction2()
		{
			var column = new CustomColumnDescriptor<TimeSpan>("foo");
			column.Id.Should().Be("foo");
			column.DataType.Should().Be<TimeSpan>();
		}

		[Test]
		[Description("Verifies that two columns are equal if they share the same id")]
		public void TestEquality1()
		{
			const string id = "foo";
			var column = new CustomColumnDescriptor<string>(id);
			column.Equals(column).Should().BeTrue();

			var equalColumn = new CustomColumnDescriptor<string>(id);
			column.Equals(equalColumn).Should().BeTrue();
			column.GetHashCode().Should().Be(equalColumn.GetHashCode());
		}

		[Test]
		[Description("Verifies that two columns are not equal if have different ids")]
		public void TestEquality2()
		{
			const string id1 = "foo";
			const string id2 = "bar";
			var column = new CustomColumnDescriptor<string>(id1);
			column.Equals(column).Should().BeTrue();

			var otherColumn = new CustomColumnDescriptor<string>(id2);
			column.Equals(otherColumn).Should().BeFalse();
		}

		[Test]
		[Description("Verifies that a custom column never equals a well-known column, even when it uses the same id")]
		public void TestEquality3()
		{
			var index = Core.Columns.Index;
			var column = new CustomColumnDescriptor<LogLineIndex>(index.Id);
			column.Equals(index).Should().BeFalse("because a well-known-column is fundamentally different from a custom column and thus the two may never be equal");
		}
	}
}