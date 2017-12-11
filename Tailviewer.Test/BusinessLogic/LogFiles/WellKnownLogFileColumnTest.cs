using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class WellKnownLogFileColumnTest
	{
		[Test]
		public void TestConstruction()
		{
			var column = new WellKnownLogFileColumn<double?>("foo", "bar");
			column.Id.Should().Be("foo");
			column.Name.Should().Be("bar");
			column.DataType.Should().Be<double?>();
		}

		[Test]
		public void TestEquality()
		{
			var column = new WellKnownLogFileColumn<double?>("foo", "bar");
			var otherColumn = new CustomLogFileColumn<double?>("foo", "bar");
			column.Equals(otherColumn).Should().BeFalse("because a well known column can never equal a custom column");
		}
	}
}