using System;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogTables;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class LogEntryTest
	{
		[Test]
		public void TestToString1()
		{
			var row = new LogEntry();
			new Action(() => row.ToString()).ShouldNotThrow();
		}

		[Test]
		public void TestToString2()
		{
			var row = new LogEntry(new object[]{42});
			row.ToString().Should().Be("42");
		}

		[Test]
		public void TestToString3()
		{
			var row = new LogEntry(null, "42");
			row.ToString().Should().Be("null|42");
		}

		[Test]
		public void TestEquality()
		{
			new LogEntry().Equals(new LogEntry()).Should().BeTrue();
			new LogEntry(42).Equals(new LogEntry(42)).Should().BeTrue();
			new LogEntry(42).Equals(new LogEntry()).Should().BeFalse();
			new LogEntry().Equals(new LogEntry("foo")).Should().BeFalse();
			new LogEntry("foo", 42).Equals(new LogEntry("foo", 42)).Should().BeTrue();
		}
	}
}