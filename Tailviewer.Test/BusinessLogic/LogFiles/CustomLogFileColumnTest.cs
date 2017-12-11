using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class CustomLogFileColumnTest
	{
		[Test]
		public void TestConstruction1([Values(1, 42, "foo")] object id)
		{
			var column = new CustomLogFileColumn<DateTime>(id, "StartTime");
			column.Id.Should().Be(id);
			column.Name.Should().Be("StartTime");
			column.DataType.Should().Be<DateTime>();
		}

		[Test]
		public void TestConstruction2()
		{
			var column = new CustomLogFileColumn<TimeSpan>("foo", "Elapsed Elefants");
			column.Id.Should().Be("foo");
			column.Name.Should().Be("Elapsed Elefants");
			column.DataType.Should().Be<TimeSpan>();
		}

		[Test]
		[Description("Verifies that two columns are equal if they share the same id")]
		public void TestEquality1()
		{
			const string id = "foo";
			var column = new CustomLogFileColumn<string>(id, "FOO");
			column.Equals(column).Should().BeTrue();

			var equalColumn = new CustomLogFileColumn<string>(id, "FOO");
			column.Equals(equalColumn).Should().BeTrue();
			column.GetHashCode().Should().Be(equalColumn.GetHashCode());
		}

		[Test]
		[Description("Verifies that two columns are not equal if have different ids")]
		public void TestEquality2()
		{
			const string id1 = "foo";
			const string id2 = "bar";
			var column = new CustomLogFileColumn<string>(id1, "FOO");
			column.Equals(column).Should().BeTrue();

			var otherColumn = new CustomLogFileColumn<string>(id2, "FOO");
			column.Equals(otherColumn).Should().BeFalse();
		}

		[Test]
		[Description("Verifies that a custom column never equals a well-known column, even when it uses the same id")]
		public void TestEquality3()
		{
			var index = LogFileColumns.Index;
			var column = new CustomLogFileColumn<LogLineIndex>(index.Id, index.Name);
			column.Equals(index).Should().BeFalse("because a well-known-column is fundamentally different from a custom column and thus the two may never be equal");
		}
	}
}