using FluentAssertions;
using NUnit.Framework;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class WellKnownLogFileColumnTest
	{
		[Test]
		public void TestConstructionNullable()
		{
			var column = new WellKnownLogFileColumn<double?>("foo");
			column.Id.Should().Be("foo");
			column.DataType.Should().Be<double?>();
			column.DefaultValue.Should().BeNull();
			((ILogFileColumn) column).DefaultValue.Should().BeNull();
		}

		[Test]
		public void TestConstructionValueType()
		{
			var column = new WellKnownLogFileColumn<float>("foo");
			column.Id.Should().Be("foo");
			column.DataType.Should().Be<float>();
			column.DefaultValue.Should().Be(0.0f);
			((ILogFileColumn)column).DefaultValue.Should().Be(0.0f);
		}

		[Test]
		public void TestConstructionReferenceType()
		{
			var column = new WellKnownLogFileColumn<string>("foo");
			column.Id.Should().Be("foo");
			column.DataType.Should().Be<string>();
			column.DefaultValue.Should().BeNull();
			((ILogFileColumn)column).DefaultValue.Should().BeNull();
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