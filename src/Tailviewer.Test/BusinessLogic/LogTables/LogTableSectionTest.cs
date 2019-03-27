using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.LogTables;

namespace Tailviewer.Test.BusinessLogic.LogTables
{
	[TestFixture]
	public sealed class LogTableSectionTest
	{
		[Test]
		public void TestCtor()
		{
			var section = new LogTableSection(42, 1000);
			section.Index.Should().Be(42);
			section.Count.Should().Be(1000);
		}

		[Test]
		public void TestEquality()
		{
			new LogTableSection().Equals(new LogTableSection()).Should().BeTrue();
			new LogTableSection().Equals(new LogTableSection(0, 0)).Should().BeTrue();
			new LogTableSection().Equals(new LogTableSection(1, 1)).Should().BeFalse();
			new LogTableSection(1, 1).Equals(new LogTableSection()).Should().BeFalse();
			new LogTableSection(1, 100).Equals(new LogTableSection(1, 101)).Should().BeFalse();
			new LogTableSection(1, 100).Equals(new LogTableSection(2, 100)).Should().BeFalse();
		}

		[Test]
		public void TestToString()
		{
			new LogTableSection().ToString().Should().Be("[#0, #0]");
			new LogTableSection(42, 100).ToString().Should().Be("[#42, #100]");
		}
	}
}