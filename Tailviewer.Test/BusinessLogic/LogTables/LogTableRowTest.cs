using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class LogTableRowTest
	{
		[Test]
		public void TestToString1()
		{
			var row = new LogTableRow();
			new Action(() => row.ToString()).ShouldNotThrow();
		}

		[Test]
		public void TestToString2()
		{
			var row = new LogTableRow(new object[]{42});
			row.ToString().Should().Be("42");
		}

		[Test]
		public void TestEquality()
		{
			new LogTableRow().Equals(new LogTableRow()).Should().BeTrue();
			new LogTableRow(42).Equals(new LogTableRow(42)).Should().BeTrue();
			new LogTableRow(42).Equals(new LogTableRow()).Should().BeFalse();
			new LogTableRow().Equals(new LogTableRow("foo")).Should().BeFalse();
			new LogTableRow("foo", 42).Equals(new LogTableRow("foo", 42)).Should().BeTrue();
		}
	}
}