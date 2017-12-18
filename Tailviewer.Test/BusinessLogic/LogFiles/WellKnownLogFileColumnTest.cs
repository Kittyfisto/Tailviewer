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
			var column = new WellKnownLogFileColumn<double?>("foo");
			column.Id.Should().Be("foo");
			column.DataType.Should().Be<double?>();
		}

		[Test]
		public void TestEquality()
		{
			var column = new WellKnownLogFileColumn<double?>("foo");
			var otherColumn = new CustomLogFileColumn<double?>("foo");
			column.Equals(otherColumn).Should().BeFalse("because a well known column can never equal a custom column");
		}
	}
}